using System.Runtime.CompilerServices;
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

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Service for managing service accounts
/// </summary>
internal class ServiceAccountService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : ManagementServiceBase(options, channel, authenticator), IServiceAccountService
{
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new service account")]
	public async Task<string> CreateAsync(
		string armoredPgpPublicKey,
		bool useUserRole = false,
		string? role = null,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("create", "service account");

		var request = new CreateServiceAccountRequest
		{
			ArmoredPgpPublicKey = armoredPgpPublicKey,
			UseUserRole = useUserRole,
			Role = role ?? "",
			Name = $"service-account-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.CreateServiceAccountAsync,
			GrpcMethods.CreateServiceAccount,
			"service account creation",
			cancellationToken);

		return response.PublicKeyId;
	}

	public async Task<IReadOnlyList<ServiceAccountInfo>> ListAsync(CancellationToken cancellationToken = default)
	{
		var request = new Empty();

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.ListServiceAccountsAsync,
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

	[IsWriteAction(WriteActionType.Update, Description = "Renews a service account with new credentials")]
	public async Task<string> RenewAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("update", "service account");

		var request = new RenewServiceAccountRequest
		{
			Name = name,
			ArmoredPgpPublicKey = armoredPgpPublicKey
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.RenewServiceAccountAsync,
			GrpcMethods.RenewServiceAccount,
			"service account renewal",
			cancellationToken);

		return response.PublicKeyId;
	}

	[IsWriteAction(WriteActionType.Delete, Description = "Destroys a service account")]
	public async Task DestroyAsync(string name, CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("delete", "service account");

		var request = new DestroyServiceAccountRequest
		{
			Name = name
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.DestroyServiceAccountAsync,
			GrpcMethods.DestroyServiceAccount,
			"service account destruction",
			cancellationToken);
	}
}

/// <summary>
/// Service for validation operations
/// </summary>
internal class ValidationService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : ManagementServiceBase(options, channel, authenticator), IValidationService
{
	public async Task ValidateConfigAsync(string config, CancellationToken cancellationToken = default)
	{
		var request = new ValidateConfigRequest
		{
			Config = config
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.ValidateConfigAsync,
			GrpcMethods.ValidateConfig,
			"configuration validation",
			cancellationToken);
	}

	public async Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		CancellationToken cancellationToken = default)
	{
		var request = new ValidateJsonSchemaRequest
		{
			Data = data,
			Schema = schema
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.ValidateJSONSchemaAsync,
			GrpcMethods.ValidateJsonSchema,
			"JSON schema validation",
			cancellationToken);

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

	private static ValidateJsonSchemaError ConvertProtoError(ValidateJsonSchemaResponse.Types.Error protoError)
	{
		var error = new ValidateJsonSchemaError
		{
			SchemaPath = protoError.SchemaPath ?? "",
			DataPath = protoError.DataPath ?? "",
			Cause = protoError.Cause ?? ""
		};

		if (protoError.Errors != null)
		{
			foreach (var nestedProtoError in protoError.Errors)
			{
				error.Errors.Add(ConvertProtoError(nestedProtoError));
			}
		}

		return error;
	}
}

/// <summary>
/// Service for Kubernetes operations
/// </summary>
internal class KubernetesService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : ManagementServiceBase(options, channel, authenticator), IKubernetesService
{
	public async Task<KubernetesUpgradePreCheckResult> UpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken = default)
	{
		var request = new KubernetesUpgradePreChecksRequest
		{
			NewVersion = newVersion
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.KubernetesUpgradePreChecksAsync,
			GrpcMethods.KubernetesUpgradePreChecks,
			"Kubernetes upgrade pre-checks",
			cancellationToken);

		return response.Ok
			? KubernetesUpgradePreCheckResult.Success(response.Reason)
			: KubernetesUpgradePreCheckResult.Failure(response.Reason);
	}

	[IsWriteAction(WriteActionType.Update, Description = "Synchronizes Kubernetes manifests")]
	public async IAsyncEnumerable<KubernetesSyncResult> StreamSyncManifestsAsync(
		bool dryRun = false,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (!dryRun)
		{
			EnsureWriteOperationAllowed("update", "kubernetes manifests");
		}

		var request = new KubernetesSyncManifestRequest
		{
			DryRun = dryRun
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.KubernetesSyncManifests,
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
	}
}

/// <summary>
/// Service for machine schematic operations
/// </summary>
internal class SchematicService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : ManagementServiceBase(options, channel, authenticator), ISchematicService
{
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new schematic for machine provisioning")]
	public async Task<SchematicResult> CreateAsync(
		string[]? extensions = null,
		string[]? extraKernelArgs = null,
		Dictionary<uint, string>? metaValues = null,
		string? talosVersion = null,
		string? mediaId = null,
		bool secureBoot = false,
		SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode = SiderolinkGrpcTunnelMode.Auto,
		string? joinToken = null,
		CancellationToken cancellationToken = default)
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

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.CreateSchematicAsync,
			GrpcMethods.CreateSchematic,
			"schematic creation",
			cancellationToken);

		return new SchematicResult
		{
			SchematicId = response.SchematicId,
			PxeUrl = response.PxeUrl,
			GrpcTunnelEnabled = response.GrpcTunnelEnabled
		};
	}
}

/// <summary>
/// Service for machine management operations
/// </summary>
internal class MachineService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : ManagementServiceBase(options, channel, authenticator), IMachineService
{
	public async IAsyncEnumerable<byte[]> StreamLogsAsync(
		string machineId,
		bool follow = false,
		int tailLines = 0,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = new MachineLogsRequest
		{
			MachineId = machineId,
			Follow = follow,
			TailLines = tailLines
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.MachineLogs,
			GrpcMethods.MachineLogs,
			"machine logs streaming");

		await foreach (var logData in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return logData.Bytes.ToByteArray();
		}
	}

	[IsWriteAction(WriteActionType.Update, Description = "Performs a maintenance upgrade on a machine")]
	public async Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("upgrade", "machine");

		Logger.LogInformation("Starting maintenance upgrade for machine {MachineId} to version {Version}",
			machineId, version);

		var request = new MaintenanceUpgradeRequest
		{
			MachineId = machineId,
			Version = version
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.MaintenanceUpgradeAsync,
			GrpcMethods.MaintenanceUpgrade,
			"maintenance upgrade",
			cancellationToken);
	}

	public async Task<MachineJoinConfig> GetJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		CancellationToken cancellationToken = default)
	{
		var request = new GetMachineJoinConfigRequest
		{
			UseGrpcTunnel = useGrpcTunnel,
			JoinToken = joinToken
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.GetMachineJoinConfigAsync,
			GrpcMethods.GetMachineJoinConfig,
			"machine join config retrieval",
			cancellationToken);

		return new MachineJoinConfig
		{
			Config = response.Config ?? "",
			KernelArgs = [.. response.KernelArgs]
		};
	}

	[IsWriteAction(WriteActionType.Create, Description = "Creates a join token for machines")]
	public async Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("create", "join token");

		var request = new CreateJoinTokenRequest
		{
			Name = name,
			ExpirationTime = Timestamp.FromDateTimeOffset(expirationTime)
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.CreateJoinTokenAsync,
			GrpcMethods.CreateJoinToken,
			"join token creation",
			cancellationToken);

		return response.Id;
	}
}

/// <summary>
/// Service for support and audit operations
/// </summary>
internal class SupportService(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : ManagementServiceBase(options, channel, authenticator), ISupportService
{
	public async IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
		string cluster,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Starting support bundle generation for cluster: {Cluster}", cluster);

		var request = new GetSupportBundleRequest
		{
			Cluster = cluster
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.GetSupportBundle,
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

			yield return progress;
		}
	}

	public async IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Reading audit logs from {StartDate} to {EndDate}", startDate, endDate);

		var request = new ReadAuditLogRequest
		{
			StartTime = startDate,
			EndTime = endDate
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.ReadAuditLog,
			GrpcMethods.ReadAuditLog,
			"audit log reading");

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return response.AuditLog.ToByteArray();
		}
	}

	[IsWriteAction(WriteActionType.Delete, Description = "Tears down a locked cluster")]
	public async Task TearDownLockedClusterAsync(
		string clusterId,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("tear down", "locked cluster");

		Logger.LogWarning("Tearing down locked cluster: {ClusterId} - this is a destructive operation", clusterId);

		var request = new TearDownLockedClusterRequest
		{
			ClusterId = clusterId
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.TearDownLockedClusterAsync,
			GrpcMethods.TearDownLockedCluster,
			"locked cluster tear down",
			cancellationToken);
	}
}
