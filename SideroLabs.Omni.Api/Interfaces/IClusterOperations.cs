using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

public interface IClusterOperations
{
	/// <summary>
	/// Lists all clusters
	/// </summary>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of clusters</returns>
	IAsyncEnumerable<Cluster> ListAsync(
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a specific cluster by ID
	/// </summary>
	/// <param name="clusterId">Cluster ID</param>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The cluster resource</returns>
	Task<Cluster> GetAsync(
		string clusterId,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets cluster status information. Returns an implementation-specific status object.
	/// </summary>
	Task<object> GetStatusAsync(
		string clusterName,
		TimeSpan? waitTimeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a cluster resource.
	/// </summary>
	Task<IOmniResource> CreateAsync(
		IOmniResource cluster,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete a cluster by name
	/// </summary>
	Task DeleteAsync(
		string clusterName,
		bool force = false,
		CancellationToken cancellationToken = default);

	Task LockMachineAsync(
		string machineId,
		string clusterName,
		CancellationToken cancellationToken = default);

	Task UnlockMachineAsync(
		string machineId,
		string clusterName,
		CancellationToken cancellationToken = default);
}
