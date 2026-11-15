using System.Runtime.InteropServices;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Service for retrieving Kubernetes configuration
/// </summary>
public interface IKubeConfigService
{
	/// <summary>
	/// Gets the kubeconfig for a cluster
	/// </summary>
	Task<string> GetAsync([Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Gets the kubeconfig for a cluster with service account options
	/// </summary>
	Task<string> GetAsync(
		bool serviceAccount,
		[Optional] TimeSpan? serviceAccountTtl,
		[Optional] string? serviceAccountUser,
		[Optional] string[]? serviceAccountGroups,
		[Optional] string? grantType,
		[Optional] bool breakGlass,
		[Optional] CancellationToken cancellationToken);
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
		[Optional] bool raw,
		[Optional] bool breakGlass,
		[Optional] CancellationToken cancellationToken);
}

/// <summary>
/// Service for retrieving Omni configuration
/// </summary>
public interface IOmniConfigService
{
	/// <summary>
	/// Gets the omniconfig for omnictl client
	/// </summary>
	Task<string> GetAsync([Optional] CancellationToken cancellationToken);
}
