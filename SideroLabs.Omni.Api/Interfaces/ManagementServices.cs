using System.Runtime.InteropServices;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Service for managing service accounts
/// </summary>
public interface IServiceAccountService
{
	/// <summary>
	/// Creates a service account
	/// </summary>
	Task<string> CreateAsync(
		string armoredPgpPublicKey,
		[Optional] bool useUserRole,
		[Optional] string? role,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Lists all service accounts
	/// </summary>
	Task<IReadOnlyList<ServiceAccountInfo>> ListAsync([Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Renews a service account
	/// </summary>
	Task<string> RenewAsync(
		string name,
		string armoredPgpPublicKey,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Destroys a service account
	/// </summary>
	Task DestroyAsync(string name, [Optional] CancellationToken cancellationToken);
}

/// <summary>
/// Service for validation operations
/// </summary>
public interface IValidationService
{
	/// <summary>
	/// Validates a configuration
	/// </summary>
	Task ValidateConfigAsync(string config, [Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Validates JSON data against a JSON schema
	/// </summary>
	Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		[Optional] CancellationToken cancellationToken);
}

/// <summary>
/// Service for Kubernetes operations
/// </summary>
public interface IKubernetesService
{
	/// <summary>
	/// Performs Kubernetes upgrade pre-checks
	/// </summary>
	Task<KubernetesUpgradePreCheckResult> UpgradePreChecksAsync(
		string newVersion,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Streams Kubernetes manifest synchronization results
	/// </summary>
	IAsyncEnumerable<KubernetesSyncResult> StreamSyncManifestsAsync(
		[Optional] bool dryRun,
		[Optional] CancellationToken cancellationToken);
}

/// <summary>
/// Service for machine schematic operations
/// </summary>
public interface ISchematicService
{
	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	/// <param name="options">Schematic options; defaults are used when omitted</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task<SchematicResult> CreateAsync(
		[Optional] CreateSchematicOptions? options,
		[Optional] CancellationToken cancellationToken);
}

/// <summary>
/// Service for machine management operations
/// </summary>
public interface IMachineService
{
	/// <summary>
	/// Streams machine logs
	/// </summary>
	IAsyncEnumerable<byte[]> StreamLogsAsync(
		string machineId,
		[Optional] bool follow,
		[Optional] int tailLines,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Performs a maintenance upgrade on a machine
	/// </summary>
	Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Gets the configuration for a machine to join the cluster
	/// </summary>
	Task<MachineJoinConfig> GetJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Creates a join token for machines to join the cluster
	/// </summary>
	Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		[Optional] CancellationToken cancellationToken);
}

/// <summary>
/// Service for support and audit operations
/// </summary>
public interface ISupportService
{
	/// <summary>
	/// Generates and streams a support bundle for troubleshooting
	/// </summary>
	IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
		string cluster,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Streams audit log entries for compliance and security analysis
	/// </summary>
	IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Tears down a locked cluster
	/// </summary>
	Task TearDownLockedClusterAsync(
		string clusterId,
		[Optional] CancellationToken cancellationToken);
}
