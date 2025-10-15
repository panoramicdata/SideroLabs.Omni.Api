using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for Omni Management Service operations
/// This represents the actual gRPC ManagementService from management.proto
/// </summary>
public interface IManagementService
{
	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	/// <param name="serviceAccount">Whether to create a service account</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	/// <param name="serviceAccount">Whether to create a service account</param>
	/// <param name="serviceAccountTtl">TTL for service account</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	/// <param name="serviceAccount">Whether to create a service account</param>
	/// <param name="serviceAccountTtl">TTL for service account</param>
	/// <param name="serviceAccountUser">Service account user name</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	/// <param name="serviceAccount">Whether to create a service account</param>
	/// <param name="serviceAccountTtl">TTL for service account</param>
	/// <param name="serviceAccountUser">Service account user name</param>
	/// <param name="serviceAccountGroups">Service account groups</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	/// <param name="serviceAccount">Whether to create a service account</param>
	/// <param name="serviceAccountTtl">TTL for service account</param>
	/// <param name="serviceAccountUser">Service account user name</param>
	/// <param name="serviceAccountGroups">Service account groups</param>
	/// <param name="grantType">Grant type for the service account</param>
	/// <param name="breakGlass">Whether to use break-glass access</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		string? grantType,
		bool breakGlass,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the talosconfig for cluster access
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Talosconfig as decoded string</returns>
	Task<string> GetTalosConfigAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the talosconfig for cluster access
	/// </summary>
	/// <param name="raw">Whether to get raw talosconfig (admin mode)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Talosconfig as decoded string</returns>
	Task<string> GetTalosConfigAsync(
		bool raw,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the talosconfig for cluster access
	/// </summary>
	/// <param name="raw">Whether to get raw talosconfig (admin mode)</param>
	/// <param name="breakGlass">Whether to use break-glass access (operator mode)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Talosconfig as decoded string</returns>
	Task<string> GetTalosConfigAsync(
		bool raw,
		bool breakGlass,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the omniconfig for omnictl client
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Omniconfig as decoded string</returns>
	Task<string> GetOmniConfigAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a service account
	/// </summary>
	/// <param name="armoredPgpPublicKey">PGP public key in armored format</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Public key ID of created service account</returns>
	Task<string> CreateServiceAccountAsync(
		string armoredPgpPublicKey,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a service account
	/// </summary>
	/// <param name="armoredPgpPublicKey">PGP public key in armored format</param>
	/// <param name="useUserRole">Whether to use the creating user's role</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Public key ID of created service account</returns>
	Task<string> CreateServiceAccountAsync(
		string armoredPgpPublicKey,
		bool useUserRole,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a service account
	/// </summary>
	/// <param name="armoredPgpPublicKey">PGP public key in armored format</param>
	/// <param name="useUserRole">Whether to use the creating user's role</param>
	/// <param name="role">Role for the service account (ignored if useUserRole is true)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Public key ID of created service account</returns>
	Task<string> CreateServiceAccountAsync(
		string armoredPgpPublicKey,
		bool useUserRole,
		string? role,
		CancellationToken cancellationToken);

	/// <summary>
	/// Lists all service accounts
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>List of service account information</returns>
	Task<IReadOnlyList<ServiceAccountInfo>> ListServiceAccountsAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Renews a service account
	/// </summary>
	/// <param name="name">Service account name</param>
	/// <param name="armoredPgpPublicKey">New PGP public key</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Public key ID of renewed service account</returns>
	Task<string> RenewServiceAccountAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken);

	/// <summary>
	/// Destroys a service account
	/// </summary>
	/// <param name="name">Service account name to destroy</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task DestroyServiceAccountAsync(
		string name,
		CancellationToken cancellationToken);

	/// <summary>
	/// Validates a configuration
	/// </summary>
	/// <param name="config">Configuration string to validate</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task ValidateConfigAsync(
		string config,
		CancellationToken cancellationToken);

	/// <summary>
	/// Validates JSON data against a JSON schema
	/// </summary>
	/// <param name="data">JSON data to validate</param>
	/// <param name="schema">JSON schema to validate against</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Validation result with any errors found</returns>
	Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		CancellationToken cancellationToken);

	/// <summary>
	/// Performs Kubernetes upgrade pre-checks
	/// </summary>
	/// <param name="newVersion">New Kubernetes version to check</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Pre-check results</returns>
	Task<(bool Ok, string Reason)> KubernetesUpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Schematic ID, PXE URL, and gRPC tunnel enabled status</returns>
	Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	/// <param name="extensions">Extensions to include</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Schematic ID, PXE URL, and gRPC tunnel enabled status</returns>
	Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		string[]? extensions,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	/// <param name="extensions">Extensions to include</param>
	/// <param name="extraKernelArgs">Extra kernel arguments</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Schematic ID, PXE URL, and gRPC tunnel enabled status</returns>
	Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	/// <param name="extensions">Extensions to include</param>
	/// <param name="extraKernelArgs">Extra kernel arguments</param>
	/// <param name="metaValues">Meta values mapping</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Schematic ID, PXE URL, and gRPC tunnel enabled status</returns>
	Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a schematic for machine provisioning with all available options
	/// </summary>
	/// <param name="extensions">Extensions to include</param>
	/// <param name="extraKernelArgs">Extra kernel arguments</param>
	/// <param name="metaValues">Meta values mapping</param>
	/// <param name="talosVersion">Talos version to use</param>
	/// <param name="mediaId">Media ID for the installation media</param>
	/// <param name="secureBoot">Whether to enable secure boot</param>
	/// <param name="siderolinkGrpcTunnelMode">Siderolink gRPC tunnel mode</param>
	/// <param name="joinToken">Join token for the machine</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Schematic ID, PXE URL, and gRPC tunnel enabled status</returns>
	Task<(string SchematicId, string PxeUrl, bool GrpcTunnelEnabled)> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		string? talosVersion,
		string? mediaId,
		bool secureBoot,
		SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode,
		string? joinToken,
		CancellationToken cancellationToken);

	/// <summary>
	/// Generates and streams a support bundle for troubleshooting
	/// </summary>
	/// <param name="cluster">Cluster name to generate support bundle for</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of support bundle progress updates and data</returns>
	IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
		string cluster,
		CancellationToken cancellationToken);

	/// <summary>
	/// Streams audit log entries for compliance and security analysis
	/// </summary>
	/// <param name="startDate">Start date in YYYY-MM-DD format (inclusive)</param>
	/// <param name="endDate">End date in YYYY-MM-DD format (inclusive)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of audit log data</returns>
	IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		CancellationToken cancellationToken);

	/// <summary>
	/// Performs a maintenance upgrade on a machine
	/// </summary>
	/// <param name="machineId">Machine ID to upgrade</param>
	/// <param name="version">Version to upgrade to</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the configuration for a machine to join the cluster
	/// </summary>
	/// <param name="useGrpcTunnel">Whether to use gRPC tunnel</param>
	/// <param name="joinToken">Join token for authentication</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Machine join configuration with kernel args and config</returns>
	Task<MachineJoinConfig> GetMachineJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a join token for machines to join the cluster
	/// </summary>
	/// <param name="name">Name for the join token</param>
	/// <param name="expirationTime">When the token should expire</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The created token ID</returns>
	Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		CancellationToken cancellationToken);

	/// <summary>
	/// Tears down a locked cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster to tear down</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task TearDownLockedClusterAsync(
		string clusterId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Streams machine logs
	/// </summary>
	/// <param name="machineId">Machine ID to get logs from</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of log data</returns>
	IAsyncEnumerable<byte[]> StreamMachineLogsAsync(
		string machineId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Streams machine logs
	/// </summary>
	/// <param name="machineId">Machine ID to get logs from</param>
	/// <param name="follow">Whether to follow the logs</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of log data</returns>
	IAsyncEnumerable<byte[]> StreamMachineLogsAsync(
		string machineId,
		bool follow,
		CancellationToken cancellationToken);

	/// <summary>
	/// Streams machine logs
	/// </summary>
	/// <param name="machineId">Machine ID to get logs from</param>
	/// <param name="follow">Whether to follow the logs</param>
	/// <param name="tailLines">Number of lines to tail</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of log data</returns>
	IAsyncEnumerable<byte[]> StreamMachineLogsAsync(
		string machineId,
		bool follow,
		int tailLines,
		CancellationToken cancellationToken);

	/// <summary>
	/// Streams Kubernetes manifest synchronization results
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of sync results</returns>
	IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Streams Kubernetes manifest synchronization results
	/// </summary>
	/// <param name="dryRun">Whether to perform a dry run</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of sync results</returns>
	IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(
		bool dryRun,
		CancellationToken cancellationToken);
}
