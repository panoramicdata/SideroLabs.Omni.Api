using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Provides high-level lifecycle operations for Omni cluster resources.
/// </summary>
public interface IClusterOperations
{
	/// <summary>
	/// Lists cluster resources from the COSI State API.
	/// </summary>
	/// <param name="namespace">The COSI namespace to query. Defaults to <c>default</c>.</param>
	/// <param name="cancellationToken">A token that can cancel the asynchronous stream.</param>
	/// <returns>An asynchronous stream of cluster resources.</returns>
	IAsyncEnumerable<Cluster> ListAsync(
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a single cluster resource by resource identifier.
	/// </summary>
	/// <param name="clusterId">The cluster resource identifier.</param>
	/// <param name="namespace">The COSI namespace to query. Defaults to <c>default</c>.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>The resolved cluster resource.</returns>
	Task<Cluster> GetAsync(
		string clusterId,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Resolves current cluster status, optionally waiting for state convergence.
	/// </summary>
	/// <param name="clusterName">The cluster name to inspect.</param>
	/// <param name="waitTimeout">An optional duration to wait for updated status before returning.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>An implementation-specific status payload.</returns>
	Task<object> GetStatusAsync(
		string clusterName,
		TimeSpan? waitTimeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new cluster resource in Omni.
	/// </summary>
	/// <param name="cluster">The cluster resource payload to create.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>The created resource as returned by Omni.</returns>
	Task<IOmniResource> CreateAsync(
		IOmniResource cluster,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a cluster by name.
	/// </summary>
	/// <param name="clusterName">The cluster name to delete.</param>
	/// <param name="force">Whether to force deletion when supported by Omni.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	Task DeleteAsync(
		string clusterName,
		bool force = false,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Locks a machine inside a cluster to prevent disruptive automation actions.
	/// </summary>
	/// <param name="machineId">The machine identifier to lock.</param>
	/// <param name="clusterName">The owning cluster name.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	Task LockMachineAsync(
		string machineId,
		string clusterName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Unlocks a previously locked machine in a cluster.
	/// </summary>
	/// <param name="machineId">The machine identifier to unlock.</param>
	/// <param name="clusterName">The owning cluster name.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	Task UnlockMachineAsync(
		string machineId,
		string clusterName,
		CancellationToken cancellationToken = default);
}
