namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Service for retrieving Kubernetes configuration
/// </summary>
public interface IKubeConfigService
{
	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	Task<string> GetAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the kubeconfig for a cluster with service account options
	/// </summary>
	Task<string> GetAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl = null,
		string? serviceAccountUser = null,
		string[]? serviceAccountGroups = null,
		string? grantType = null,
		bool breakGlass = false,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for retrieving Talos configuration
/// </summary>
public interface ITalosConfigService
{
	/// <summary>
	/// Gets the talosconfig for cluster access
	/// </summary>
	Task<string> GetAsync(
		bool raw = false,
		bool breakGlass = false,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for retrieving Omni configuration
/// </summary>
public interface IOmniConfigService
{
	/// <summary>
	/// Gets the omniconfig for omnictl client
	/// </summary>
	Task<string> GetAsync(CancellationToken cancellationToken = default);
}
