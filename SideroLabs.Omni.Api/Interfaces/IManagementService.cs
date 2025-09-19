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
	/// <param name="serviceAccount">Whether to create a service account</param>
	/// <param name="serviceAccountTtl">TTL for service account</param>
	/// <param name="serviceAccountUser">Service account user name</param>
	/// <param name="serviceAccountGroups">Service account groups</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kubeconfig as decoded string</returns>
	Task<string> GetKubeConfigAsync(
		bool serviceAccount = false,
		TimeSpan? serviceAccountTtl = null,
		string? serviceAccountUser = null,
		string[]? serviceAccountGroups = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the talosconfig for cluster access
	/// </summary>
	/// <param name="admin">Whether to get admin talosconfig</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Talosconfig as decoded string</returns>
	Task<string> GetTalosConfigAsync(bool admin = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the omniconfig for omnictl client
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Omniconfig as decoded string</returns>
	Task<string> GetOmniConfigAsync(CancellationToken cancellationToken = default);

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
		bool useUserRole = false,
		string? role = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists all service accounts
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>List of service account information</returns>
	Task<IReadOnlyList<ServiceAccountInfo>> ListServiceAccountsAsync(CancellationToken cancellationToken = default);

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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Destroys a service account
	/// </summary>
	/// <param name="name">Service account name to destroy</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task DestroyServiceAccountAsync(string name, CancellationToken cancellationToken = default);

	/// <summary>
	/// Validates a configuration
	/// </summary>
	/// <param name="config">Configuration string to validate</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task ValidateConfigAsync(string config, CancellationToken cancellationToken = default);

	/// <summary>
	/// Performs Kubernetes upgrade pre-checks
	/// </summary>
	/// <param name="newVersion">New Kubernetes version to check</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Pre-check results</returns>
	Task<(bool Ok, string Reason)> KubernetesUpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a schematic for machine provisioning
	/// </summary>
	/// <param name="extensions">Extensions to include</param>
	/// <param name="extraKernelArgs">Extra kernel arguments</param>
	/// <param name="metaValues">Meta values mapping</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Schematic ID and PXE URL</returns>
	Task<(string SchematicId, string PxeUrl)> CreateSchematicAsync(
		string[]? extensions = null,
		string[]? extraKernelArgs = null,
		Dictionary<uint, string>? metaValues = null,
		CancellationToken cancellationToken = default);

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
		bool follow = false,
		int tailLines = 100,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Streams Kubernetes manifest synchronization results
	/// </summary>
	/// <param name="dryRun">Whether to perform a dry run</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of sync results</returns>
	IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(
		bool dryRun = false,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Service account information
/// </summary>
public class ServiceAccountInfo
{
	public string Name { get; set; } = "";
	public IReadOnlyList<PgpPublicKeyInfo> PgpPublicKeys { get; set; } = [];
	public string Role { get; set; } = "";
}

/// <summary>
/// PGP public key information
/// </summary>
public class PgpPublicKeyInfo
{
	public string Id { get; set; } = "";
	public string Armored { get; set; } = "";
	public DateTimeOffset Expiration { get; set; }
}

/// <summary>
/// Kubernetes manifest sync result
/// </summary>
public class KubernetesSyncResult
{
	public enum SyncType
	{
		Unknown = 0,
		Manifest = 1,
		Rollout = 2
	}

	public SyncType ResponseType { get; set; }
	public string Path { get; set; } = "";
	public byte[] Object { get; set; } = [];
	public string Diff { get; set; } = "";
	public bool Skipped { get; set; }
}
