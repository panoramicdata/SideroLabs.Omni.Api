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
		GetKubeConfigAsync(false, null, null, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, null, null, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, TimeSpan? serviceAccountTtl, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, null, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, TimeSpan? serviceAccountTtl, string? serviceAccountUser, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, serviceAccountUser, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, serviceAccountUser, serviceAccountGroups, null, false, cancellationToken);

	/// <inheritdoc />
	public async Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		string? grantType,
		bool breakGlass,
		CancellationToken cancellationToken)
	{
		// Service account creation is a write operation
		if (serviceAccount)
		{
			EnsureWriteOperationAllowed("create", "service account");
		}

		// Break-glass access is a sensitive operation
		if (breakGlass)
		{
			Logger.LogWarning("Using break-glass access for kubeconfig");
		}

		var request = new KubeconfigRequest
		{
			ServiceAccount = serviceAccount,
			ServiceAccountUser = serviceAccountUser ?? "",
			GrantType = grantType ?? "",
			BreakGlass = breakGlass
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
		GetTalosConfigAsync(false, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetTalosConfigAsync(bool raw, CancellationToken cancellationToken) =>
		GetTalosConfigAsync(raw, false, cancellationToken);

	/// <inheritdoc />
	public async Task<string> GetTalosConfigAsync(bool raw, bool breakGlass, CancellationToken cancellationToken)
	{
		// Break-glass access is a sensitive operation
		if (breakGlass)
		{
			Logger.LogWarning("Using break-glass access for talosconfig");
		}

		var request = new TalosconfigRequest
		{
			Raw = raw,
			BreakGlass = breakGlass
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
		EnsureWriteOperationAllowed("create", "service account");

		var request = new CreateServiceAccountRequest
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
		EnsureWriteOperationAllowed("update", "service account");

		var request = new RenewServiceAccountRequest
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
		EnsureWriteOperationAllowed("delete", "service account");

		var request = new DestroyServiceAccountRequest
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
		var request = new ValidateConfigRequest
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
	public async Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		CancellationToken cancellationToken)
	{
		var request = new ValidateJsonSchemaRequest
		{
			Data = data,
			Schema = schema
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.ValidateJSONSchemaAsync,
			GrpcMethods.ValidateJsonSchema,
			"JSON schema validation",
			cancellationToken);

		// Convert the proto response to our model
		var result = new ValidateJsonSchemaResult
		{
			Errors = ConvertProtoErrors(response.Errors)
		};

		if (!result.IsValid)
		{
			Logger.LogWarning("JSON schema validation failed with {ErrorCount} error(s)", result.TotalErrorCount);
		}

		return result;
	}

	/// <summary>
	/// Converts proto validation errors to model errors
	/// </summary>
	private static List<ValidateJsonSchemaError> ConvertProtoErrors(
		Google.Protobuf.Collections.RepeatedField<ValidateJsonSchemaResponse.Types.Error> protoErrors)
	{
		var errors = new List<ValidateJsonSchemaError>();

		foreach (var protoError in protoErrors)
		{
			errors.Add(ConvertProtoError(protoError));
		}

		return errors;
	}

	/// <summary>
	/// Recursively converts a proto error to a model error
	/// </summary>
	private static ValidateJsonSchemaError ConvertProtoError(ValidateJsonSchemaResponse.Types.Error protoError)
	{
		var error = new ValidateJsonSchemaError
		{
			SchemaPath = protoError.SchemaPath ?? "",
			DataPath = protoError.DataPath ?? "",
			Cause = protoError.Cause ?? ""
		};

		// Recursively convert nested errors
		if (protoError.Errors != null)
		{
			foreach (var nestedProtoError in protoError.Errors)
			{
				error.Errors.Add(ConvertProtoError(nestedProtoError));
			}
		}

		return error;
	}

	/// <inheritdoc />
	public async Task<(bool Ok, string Reason)> KubernetesUpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken)
	{
		var request = new KubernetesUpgradePreChecksRequest
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
	public Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(CancellationToken cancellationToken) =>
		CreateSchematicAsync(null, null, null, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(string[]? extensions, CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, null, null, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(string[]? extensions, string[]? extraKernelArgs, CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, extraKernelArgs, null, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, extraKernelArgs, metaValues, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new schematic for machine provisioning")]
	public async Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		string? talosVersion,
		string? mediaId,
		bool secureBoot,
		SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode,
		string? joinToken,
		CancellationToken cancellationToken)
	{
		EnsureWriteOperationAllowed("create", "schematic");

		var request = new CreateSchematicRequest
		{
			TalosVersion = talosVersion ?? "",
			MediaId = mediaId ?? "",
			SecureBoot = secureBoot,
			SiderolinkGrpcTunnelMode = (CreateSchematicRequest.Types.SiderolinkGRPCTunnelMode)(int)siderolinkGrpcTunnelMode,
			JoinToken = joinToken ?? ""
		};

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

		return (response.SchematicId, response.PxeUrl, response.GrpcTunnelEnabled);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
		string cluster,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
	{
		Logger.LogInformation("Starting support bundle generation for cluster: {Cluster}", cluster);

		var request = new GetSupportBundleRequest
		{
			Cluster = cluster
		};

		using var call = _callHelper.ExecuteStreamingCall(
			request,
			_grpcClient.GetSupportBundle,
			GrpcMethods.GetSupportBundle,
			"support bundle generation");

		await foreach (var update in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			var progress = new SupportBundleProgress
			{
				Source = update.Progress?.Source ?? "",
				Error = update.Progress?.Error ?? "",
				State = update.Progress?.State ?? "",
				Total = update.Progress?.Total ?? 0,
				Value = update.Progress?.Value ?? 0,
				BundleData = update.BundleData?.Length > 0 ? update.BundleData.ToByteArray() : null
			};

			if (progress.HasError)
			{
				Logger.LogWarning("Support bundle generation error from {Source}: {Error}", 
					progress.Source, progress.Error);
			}
			else if (progress.HasBundleData)
			{
				Logger.LogInformation("Support bundle data received: {Size} bytes", progress.BundleData!.Length);
			}
			else if (!string.IsNullOrEmpty(progress.State))
			{
				Logger.LogDebug("Support bundle progress: {State} ({Value}/{Total})", 
					progress.State, progress.Value, progress.Total);
			}

			yield return progress;
		}

		Logger.LogInformation("Support bundle generation completed for cluster: {Cluster}", cluster);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
	{
		Logger.LogInformation("Reading audit logs from {StartDate} to {EndDate}", startDate, endDate);

		var request = new ReadAuditLogRequest
		{
			StartTime = startDate,
			EndTime = endDate
		};

		using var call = _callHelper.ExecuteStreamingCall(
			request,
			_grpcClient.ReadAuditLog,
			GrpcMethods.ReadAuditLog,
			"audit log reading");

		var totalBytesRead = 0L;
		var chunkCount = 0;

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			var logData = response.AuditLog.ToByteArray();
			totalBytesRead += logData.Length;
			chunkCount++;

			Logger.LogDebug("Received audit log chunk {ChunkNumber}: {Size} bytes", chunkCount, logData.Length);

			yield return logData;
		}

		Logger.LogInformation("Audit log reading completed: {TotalChunks} chunks, {TotalBytes} total bytes", 
			chunkCount, totalBytesRead);
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Performs a maintenance upgrade on a machine")]
	public async Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		CancellationToken cancellationToken)
	{
		EnsureWriteOperationAllowed("upgrade", "machine");

		Logger.LogInformation("Starting maintenance upgrade for machine {MachineId} to version {Version}", 
			machineId, version);

		var request = new MaintenanceUpgradeRequest
		{
			MachineId = machineId,
			Version = version
		};

		await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.MaintenanceUpgradeAsync,
			GrpcMethods.MaintenanceUpgrade,
			"maintenance upgrade",
			cancellationToken);

		Logger.LogInformation("Maintenance upgrade completed successfully for machine {MachineId}", machineId);
	}

	/// <inheritdoc />
	public async Task<MachineJoinConfig> GetMachineJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		CancellationToken cancellationToken)
	{
		Logger.LogInformation("Getting machine join configuration (useGrpcTunnel: {UseGrpcTunnel})", useGrpcTunnel);

		var request = new GetMachineJoinConfigRequest
		{
			UseGrpcTunnel = useGrpcTunnel,
			JoinToken = joinToken
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.GetMachineJoinConfigAsync,
			GrpcMethods.GetMachineJoinConfig,
			"machine join config retrieval",
			cancellationToken);

		var config = new MachineJoinConfig
		{
			Config = response.Config ?? "",
			KernelArgs = [.. response.KernelArgs]
		};

		Logger.LogInformation("Machine join config retrieved: {Summary}", config.GetSummary());

		return config;
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a join token for machines")]
	public async Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		CancellationToken cancellationToken)
	{
		EnsureWriteOperationAllowed("create", "join token");

		Logger.LogInformation("Creating join token: {Name}, expires: {ExpirationTime}", name, expirationTime);

		var request = new CreateJoinTokenRequest
		{
			Name = name,
			ExpirationTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(expirationTime)
		};

		var response = await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.CreateJoinTokenAsync,
			GrpcMethods.CreateJoinToken,
			"join token creation",
			cancellationToken);

		Logger.LogInformation("Join token created successfully: {TokenId}", response.Id);

		return response.Id;
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Tears down a locked cluster")]
	public async Task TearDownLockedClusterAsync(
		string clusterId,
		CancellationToken cancellationToken)
	{
		EnsureWriteOperationAllowed("tear down", "locked cluster");

		Logger.LogWarning("Tearing down locked cluster: {ClusterId} - this is a destructive operation", clusterId);

		var request = new TearDownLockedClusterRequest
		{
			ClusterId = clusterId
		};

		await _callHelper.ExecuteCallAsync(
			request,
			_grpcClient.TearDownLockedClusterAsync,
			GrpcMethods.TearDownLockedCluster,
			"locked cluster tear down",
			cancellationToken);

		Logger.LogInformation("Locked cluster {ClusterId} torn down successfully", clusterId);
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
		var request = new MachineLogsRequest
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
			EnsureWriteOperationAllowed("update", "kubernetes manifests");
		}

		var request = new KubernetesSyncManifestRequest
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
