using System.Text;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Management;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Constants;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Utilities;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of Omni Management Service using real gRPC calls
/// This implements the actual ManagementService from the management.proto file
/// </summary>
internal class OmniManagementService : OmniServiceBase, IManagementService, IDisposable
{
	private readonly ManagementService.ManagementServiceClient _grpcClient;
	private readonly GrpcCallHelper _callHelper;

	/// <summary>
	/// Initializes a new instance of the OmniManagementService class
	/// </summary>
	/// <param name="options">Client options</param>
	/// <param name="channel">gRPC channel</param>
	/// <param name="authenticator">Authentication provider</param>
	public OmniManagementService(
		OmniClientOptions options,
		GrpcChannel channel,
		OmniAuthenticator? authenticator) : base(options, channel, authenticator)
	{
		_grpcClient = new ManagementService.ManagementServiceClient(channel);
		_callHelper = new GrpcCallHelper(options.Logger, CreateCallOptions);
	}

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(CancellationToken cancellationToken) =>
		GetKubeConfigAsync(false, null, null, null, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, null, null, null, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, TimeSpan? serviceAccountTtl, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, null, null, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, TimeSpan? serviceAccountTtl, string? serviceAccountUser, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, serviceAccountUser, null, cancellationToken);

	/// <inheritdoc />
	public async Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		CancellationToken cancellationToken)
	{
		// Service account creation is a write operation
		if (serviceAccount)
		{
			EnsureWriteOperationAllowed("create", "service account");
		}

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

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.KubeconfigAsync,
			GrpcMethods.Kubeconfig,
			"kubeconfig retrieval",
			cancellationToken);

		return ResponseDecoder.DecodeConfigResponse(response.Kubeconfig);
	}

	/// <inheritdoc />
	public Task<string> GetTalosConfigAsync(CancellationToken cancellationToken) =>
		GetTalosConfigAsync(false, cancellationToken);

	/// <inheritdoc />
	public async Task<string> GetTalosConfigAsync(bool admin, CancellationToken cancellationToken)
	{
		var request = new Management.TalosconfigRequest
		{
			Raw = admin  // The proto uses 'raw' instead of 'admin'
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.TalosconfigAsync,
			GrpcMethods.Talosconfig,
			"talosconfig retrieval",
			cancellationToken);

		return ResponseDecoder.DecodeConfigResponse(response.Talosconfig);
	}

	/// <inheritdoc />
	public async Task<string> GetOmniConfigAsync(CancellationToken cancellationToken)
	{
		var request = new Empty();

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.OmniconfigAsync,
			GrpcMethods.Omniconfig,
			"omniconfig retrieval",
			cancellationToken);

		return ResponseDecoder.DecodeConfigResponse(response.Omniconfig);
	}

	/// <inheritdoc />
	public Task<string> CreateServiceAccountAsync(string armoredPgpPublicKey, CancellationToken cancellationToken) =>
		CreateServiceAccountAsync(armoredPgpPublicKey, false, null, cancellationToken);

	/// <inheritdoc />
	public Task<string> CreateServiceAccountAsync(string armoredPgpPublicKey, bool useUserRole, CancellationToken cancellationToken) =>
		CreateServiceAccountAsync(armoredPgpPublicKey, useUserRole, null, cancellationToken);

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new service account")]
	public async Task<string> CreateServiceAccountAsync(
		string armoredPgpPublicKey,
		bool useUserRole,
		string? role,
		CancellationToken cancellationToken)
	{
		EnsureWriteActionAllowed("service account");

		var request = new Management.CreateServiceAccountRequest
		{
			ArmoredPgpPublicKey = armoredPgpPublicKey,
			UseUserRole = useUserRole,
			Role = role ?? "",
			Name = $"service-account-{DateTime.UtcNow:yyyyMMdd-HHmmss}" // Generate a name if not provided
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.CreateServiceAccountAsync,
			GrpcMethods.CreateServiceAccount,
			"service account creation",
			cancellationToken);

		return response.PublicKeyId;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<ServiceAccountInfo>> ListServiceAccountsAsync(CancellationToken cancellationToken)
	{
		var request = new Empty();

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.ListServiceAccountsAsync,
			GrpcMethods.ListServiceAccounts,
			"service accounts listing",
			cancellationToken);

		var serviceAccounts = response.ServiceAccounts.Select(sa => new ServiceAccountInfo
		{
			Name = sa.Name,
			Role = sa.Role,
			PgpPublicKeys = [.. sa.PgpPublicKeys.Select(key => new PgpPublicKeyInfo
			{
				Id = key.Id,
				Armored = key.Armored,
				Expiration = key.Expiration.ToDateTimeOffset()
			})]
		}).ToList();

		return serviceAccounts;
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Renews a service account with new credentials")]
	public async Task<string> RenewServiceAccountAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken)
	{
		EnsureWriteActionAllowed("service account");

		var request = new Management.RenewServiceAccountRequest
		{
			Name = name,
			ArmoredPgpPublicKey = armoredPgpPublicKey
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.RenewServiceAccountAsync,
			GrpcMethods.RenewServiceAccount,
			"service account renewal",
			cancellationToken);

		return response.PublicKeyId;
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Destroys a service account")]
	public async Task DestroyServiceAccountAsync(string name, CancellationToken cancellationToken)
	{
		EnsureWriteActionAllowed("service account");

		var request = new Management.DestroyServiceAccountRequest
		{
			Name = name
		};

		await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.DestroyServiceAccountAsync,
			GrpcMethods.DestroyServiceAccount,
			"service account destruction",
			cancellationToken);
	}

	/// <inheritdoc />
	public async Task ValidateConfigAsync(string config, CancellationToken cancellationToken)
	{
		var request = new Management.ValidateConfigRequest
		{
			Config = config
		};

		await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.ValidateConfigAsync,
			GrpcMethods.ValidateConfig,
			"configuration validation",
			cancellationToken);
	}

	/// <inheritdoc />
	public async Task<(bool Ok, string Reason)> KubernetesUpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken)
	{
		var request = new Management.KubernetesUpgradePreChecksRequest
		{
			NewVersion = newVersion
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.KubernetesUpgradePreChecksAsync,
			GrpcMethods.KubernetesUpgradePreChecks,
			"Kubernetes upgrade pre-checks",
			cancellationToken);

		return (response.Ok, response.Reason);
	}

	/// <inheritdoc />
	public Task<(string SchematicId, string PxeUrl)> CreateSchematicAsync(CancellationToken cancellationToken) =>
		CreateSchematicAsync(null, null, null, cancellationToken);

	/// <inheritdoc />
	public Task<(string SchematicId, string PxeUrl)> CreateSchematicAsync(string[]? extensions, CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, null, null, cancellationToken);

	/// <inheritdoc />
	public Task<(string SchematicId, string PxeUrl)> CreateSchematicAsync(string[]? extensions, string[]? extraKernelArgs, CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, extraKernelArgs, null, cancellationToken);

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new schematic for machine provisioning")]
	public async Task<(string SchematicId, string PxeUrl)> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		CancellationToken cancellationToken)
	{
		EnsureWriteActionAllowed("schematic");

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

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.CreateSchematicAsync,
			GrpcMethods.CreateSchematic,
			"schematic creation",
			cancellationToken);

		return (response.SchematicId, response.PxeUrl);
	}

	/// <inheritdoc />
	public IAsyncEnumerable<byte[]> StreamMachineLogsAsync(string machineId, CancellationToken cancellationToken) =>
		StreamMachineLogsAsync(machineId, false, 100, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<byte[]> StreamMachineLogsAsync(string machineId, bool follow, CancellationToken cancellationToken) =>
		StreamMachineLogsAsync(machineId, follow, 100, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<byte[]> StreamMachineLogsAsync(
		string machineId,
		bool follow,
		int tailLines,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = new Management.MachineLogsRequest
		{
			MachineId = machineId,
			Follow = follow,
			TailLines = tailLines
		};

		using var call = _callHelper.ExecuteStreamingCall(
			request,
			_grpcClient.MachineLogs,
			GrpcMethods.MachineLogs,
			"machine logs streaming");

		await foreach (var logData in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return logData.Bytes.ToByteArray();
		}

		Logger.LogDebug("Machine log streaming completed for {MachineId}", machineId);
	}

	/// <inheritdoc />
	public IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(CancellationToken cancellationToken) =>
		StreamKubernetesSyncManifestsAsync(false, cancellationToken);

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Synchronizes Kubernetes manifests")]
	public async IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(
		bool dryRun,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
	{
		// Only enforce read-only mode for actual sync operations (not dry runs)
		if (!dryRun)
		{
			EnsureWriteActionAllowed("kubernetes manifests");
		}

		var request = new Management.KubernetesSyncManifestRequest
		{
			DryRun = dryRun
		};

		using var call = _callHelper.ExecuteStreamingCall(
			request,
			_grpcClient.KubernetesSyncManifests,
			GrpcMethods.KubernetesSyncManifests,
			"Kubernetes manifest sync streaming");

		await foreach (var syncResult in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return new KubernetesSyncResult
			{
				ResponseType = (SyncType)(int)syncResult.ResponseType,
				Path = syncResult.Path,
				Object = syncResult.Object.ToByteArray(),
				Diff = syncResult.Diff,
				Skipped = syncResult.Skipped
			};
		}

		Logger.LogDebug("Kubernetes manifest sync streaming completed");
	}

	public void Dispose() =>
		// gRPC client doesn't need explicit disposal
		GC.SuppressFinalize(this);
}
