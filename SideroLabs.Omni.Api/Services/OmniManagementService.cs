using System.Text;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Management;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of Omni Management Service using real gRPC calls
/// This implements the actual ManagementService from the management.proto file
/// </summary>
/// <remarks>
/// Initializes a new instance of the OmniManagementService class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
internal class OmniManagementService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : OmniServiceBase(options, channel, authenticator), IManagementService, IDisposable
{
	private readonly ManagementService.ManagementServiceClient _grpcClient = new(channel);

	/// <inheritdoc />
	public async Task<string> GetKubeConfigAsync(
		bool serviceAccount = false,
		TimeSpan? serviceAccountTtl = null,
		string? serviceAccountUser = null,
		string[]? serviceAccountGroups = null,
		CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Getting kubeconfig (serviceAccount: {ServiceAccount})", serviceAccount);

		var request = new Management.KubeconfigRequest
		{
			ServiceAccount = serviceAccount,
			ServiceAccountUser = serviceAccountUser ?? "",
		};

		if (serviceAccountTtl.HasValue)
		{
			request.ServiceAccountTtl = Duration.FromTimeSpan(serviceAccountTtl.Value);
		}

		if (serviceAccountGroups != null)
		{
			request.ServiceAccountGroups.AddRange(serviceAccountGroups);
		}

		var callOptions = CreateCallOptions("/management.ManagementService/Kubeconfig");
		var response = await _grpcClient.KubeconfigAsync(request, callOptions);

		// Decode the Base64-encoded byte array to string
		var decodedConfig = Encoding.UTF8.GetString(response.Kubeconfig.ToByteArray());

		Logger.LogInformation("Successfully retrieved kubeconfig ({Size} bytes)", response.Kubeconfig.Length);
		return decodedConfig;
	}

	/// <inheritdoc />
	public async Task<string> GetTalosConfigAsync(bool admin = false, CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Getting talosconfig (raw: {Admin})", admin);

		var request = new Management.TalosconfigRequest
		{
			Raw = admin  // The proto uses 'raw' instead of 'admin'
		};

		var callOptions = CreateCallOptions("/management.ManagementService/Talosconfig");
		var response = await _grpcClient.TalosconfigAsync(request, callOptions);

		// Decode the Base64-encoded byte array to string
		var decodedConfig = Encoding.UTF8.GetString(response.Talosconfig.ToByteArray());

		Logger.LogInformation("Successfully retrieved talosconfig ({Size} bytes)", response.Talosconfig.Length);
		return decodedConfig;
	}

	/// <inheritdoc />
	public async Task<string> GetOmniConfigAsync(CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Getting omniconfig");

		var request = new Empty();
		var callOptions = CreateCallOptions("/management.ManagementService/Omniconfig");
		var response = await _grpcClient.OmniconfigAsync(request, callOptions);

		// Decode the Base64-encoded byte array to string
		var decodedConfig = Encoding.UTF8.GetString(response.Omniconfig.ToByteArray());

		Logger.LogInformation("Successfully retrieved omniconfig ({Size} bytes)", response.Omniconfig.Length);
		return decodedConfig;
	}

	/// <inheritdoc />
	public async Task<string> CreateServiceAccountAsync(
		string armoredPgpPublicKey,
		bool useUserRole = false,
		string? role = null,
		CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Creating service account (useUserRole: {UseUserRole}, role: {Role})", useUserRole, role);

		var request = new Management.CreateServiceAccountRequest
		{
			ArmoredPgpPublicKey = armoredPgpPublicKey,
			UseUserRole = useUserRole,
			Role = role ?? "",
			Name = $"service-account-{DateTime.UtcNow:yyyyMMdd-HHmmss}" // Generate a name if not provided
		};

		var callOptions = CreateCallOptions("/management.ManagementService/CreateServiceAccount");
		var response = await _grpcClient.CreateServiceAccountAsync(request, callOptions);

		Logger.LogInformation("Successfully created service account with public key ID: {PublicKeyId}", response.PublicKeyId);
		return response.PublicKeyId;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<Interfaces.ServiceAccountInfo>> ListServiceAccountsAsync(CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Listing service accounts");

		var request = new Empty();
		var callOptions = CreateCallOptions("/management.ManagementService/ListServiceAccounts");
		var response = await _grpcClient.ListServiceAccountsAsync(request, callOptions);

		var serviceAccounts = response.ServiceAccounts.Select(sa => new Interfaces.ServiceAccountInfo
		{
			Name = sa.Name,
			Role = sa.Role,
			PgpPublicKeys = [.. sa.PgpPublicKeys.Select(key => new Interfaces.PgpPublicKeyInfo
			{
				Id = key.Id,
				Armored = key.Armored,
				Expiration = key.Expiration.ToDateTimeOffset()
			})]
		}).ToList();

		Logger.LogInformation("Successfully listed {Count} service accounts", serviceAccounts.Count);
		return serviceAccounts;
	}

	/// <inheritdoc />
	public async Task<string> RenewServiceAccountAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Renewing service account: {Name}", name);

		var request = new Management.RenewServiceAccountRequest
		{
			Name = name,
			ArmoredPgpPublicKey = armoredPgpPublicKey
		};

		var callOptions = CreateCallOptions("/management.ManagementService/RenewServiceAccount");
		var response = await _grpcClient.RenewServiceAccountAsync(request, callOptions);

		Logger.LogInformation("Successfully renewed service account {Name} with public key ID: {PublicKeyId}", name, response.PublicKeyId);
		return response.PublicKeyId;
	}

	/// <inheritdoc />
	public async Task DestroyServiceAccountAsync(string name, CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Destroying service account: {Name}", name);

		var request = new Management.DestroyServiceAccountRequest
		{
			Name = name
		};

		var callOptions = CreateCallOptions("/management.ManagementService/DestroyServiceAccount");
		await _grpcClient.DestroyServiceAccountAsync(request, callOptions);

		Logger.LogInformation("Successfully destroyed service account: {Name}", name);
	}

	/// <inheritdoc />
	public async Task ValidateConfigAsync(string config, CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Validating configuration ({Length} characters)", config.Length);

		var request = new Management.ValidateConfigRequest
		{
			Config = config
		};

		var callOptions = CreateCallOptions("/management.ManagementService/ValidateConfig");
		await _grpcClient.ValidateConfigAsync(request, callOptions);

		Logger.LogInformation("Configuration validation successful");
	}

	/// <inheritdoc />
	public async Task<(bool Ok, string Reason)> KubernetesUpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Running Kubernetes upgrade pre-checks for version: {Version}", newVersion);

		var request = new Management.KubernetesUpgradePreChecksRequest
		{
			NewVersion = newVersion
		};

		var callOptions = CreateCallOptions("/management.ManagementService/KubernetesUpgradePreChecks");
		var response = await _grpcClient.KubernetesUpgradePreChecksAsync(request, callOptions);

		Logger.LogInformation("Kubernetes upgrade pre-checks completed: {Ok} - {Reason}", response.Ok, response.Reason);
		return (response.Ok, response.Reason);
	}

	/// <inheritdoc />
	public async Task<(string SchematicId, string PxeUrl)> CreateSchematicAsync(
		string[]? extensions = null,
		string[]? extraKernelArgs = null,
		Dictionary<uint, string>? metaValues = null,
		CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Creating schematic with {ExtensionCount} extensions and {KernelArgCount} kernel args",
			extensions?.Length ?? 0, extraKernelArgs?.Length ?? 0);

		var request = new Management.CreateSchematicRequest();

		if (extensions != null)
		{
			request.Extensions.AddRange(extensions);
		}

		if (extraKernelArgs != null)
		{
			request.ExtraKernelArgs.AddRange(extraKernelArgs);
		}

		if (metaValues != null)
		{
			foreach (var kvp in metaValues)
			{
				request.MetaValues[kvp.Key] = kvp.Value;
			}
		}

		var callOptions = CreateCallOptions("/management.ManagementService/CreateSchematic");
		var response = await _grpcClient.CreateSchematicAsync(request, callOptions);

		Logger.LogInformation("Successfully created schematic: {SchematicId}, PXE URL: {PxeUrl}",
			response.SchematicId, response.PxeUrl);
		return (response.SchematicId, response.PxeUrl);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<byte[]> StreamMachineLogsAsync(
		string machineId,
		bool follow = false,
		int tailLines = 100,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Streaming machine logs for {MachineId} (follow: {Follow}, tail: {TailLines})",
			machineId, follow, tailLines);

		var request = new Management.MachineLogsRequest
		{
			MachineId = machineId,
			Follow = follow,
			TailLines = tailLines
		};

		var callOptions = CreateCallOptions("/management.ManagementService/MachineLogs");

		using var call = _grpcClient.MachineLogs(request, callOptions);

		await foreach (var logData in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return logData.Bytes.ToByteArray();
		}

		Logger.LogInformation("Machine log streaming completed for {MachineId}", machineId);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<Interfaces.KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(
		bool dryRun = false,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Streaming Kubernetes manifest sync (dryRun: {DryRun})", dryRun);

		var request = new Management.KubernetesSyncManifestRequest
		{
			DryRun = dryRun
		};

		var callOptions = CreateCallOptions("/management.ManagementService/KubernetesSyncManifests");

		using var call = _grpcClient.KubernetesSyncManifests(request, callOptions);

		await foreach (var syncResult in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return new Interfaces.KubernetesSyncResult
			{
				ResponseType = (Interfaces.KubernetesSyncResult.SyncType)(int)syncResult.ResponseType,
				Path = syncResult.Path,
				Object = syncResult.Object.ToByteArray(),
				Diff = syncResult.Diff,
				Skipped = syncResult.Skipped
			};
		}

		Logger.LogInformation("Kubernetes manifest sync streaming completed");
	}

	public void Dispose() =>
		// gRPC client doesn't need explicit disposal
		GC.SuppressFinalize(this);
}
