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
		bool useUserRole = false,
		string? role = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists all service accounts
	/// </summary>
	Task<IReadOnlyList<ServiceAccountInfo>> ListAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Renews a service account
	/// </summary>
	Task<string> RenewAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Destroys a service account
	/// </summary>
	Task DestroyAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for validation operations
/// </summary>
public interface IValidationService
{
	/// <summary>
	/// Validates a configuration
	/// </summary>
	Task ValidateConfigAsync(string config, CancellationToken cancellationToken = default);

	/// <summary>
	/// Validates JSON data against a JSON schema
	/// </summary>
	Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		CancellationToken cancellationToken = default);
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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Streams Kubernetes manifest synchronization results
	/// </summary>
	IAsyncEnumerable<KubernetesSyncResult> StreamSyncManifestsAsync(
		bool dryRun = false,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for machine schematic operations
/// </summary>
public interface ISchematicService
{
	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	Task<SchematicResult> CreateAsync(
		string[]? extensions = null,
		string[]? extraKernelArgs = null,
		Dictionary<uint, string>? metaValues = null,
		string? talosVersion = null,
		string? mediaId = null,
		bool secureBoot = false,
		Enums.SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode = Enums.SiderolinkGrpcTunnelMode.Auto,
		string? joinToken = null,
		CancellationToken cancellationToken = default);
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
		bool follow = false,
		int tailLines = 0,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Performs a maintenance upgrade on a machine
	/// </summary>
	Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the configuration for a machine to join the cluster
	/// </summary>
	Task<MachineJoinConfig> GetJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a join token for machines to join the cluster
	/// </summary>
	Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		CancellationToken cancellationToken = default);
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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Streams audit log entries for compliance and security analysis
	/// </summary>
	IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Tears down a locked cluster
	/// </summary>
	Task TearDownLockedClusterAsync(
		string clusterId,
		CancellationToken cancellationToken = default);
}
