using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Operations for ClusterMachine resources
/// </summary>
public interface IClusterMachineOperations
{
	/// <summary>
	/// Lists all cluster machines
	/// </summary>
	IAsyncEnumerable<ClusterMachine> ListAsync(
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a specific cluster machine by ID
	/// </summary>
	Task<ClusterMachine> GetAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new cluster machine
	/// </summary>
	Task<ClusterMachine> CreateAsync(
		ClusterMachine clusterMachine,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing cluster machine
	/// </summary>
	Task<ClusterMachine> UpdateAsync(
		ClusterMachine clusterMachine,
		string? currentVersion = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a cluster machine by ID
	/// </summary>
	Task DeleteAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Watches for cluster machine changes
	/// </summary>
	IAsyncEnumerable<ResourceEvent<ClusterMachine>> WatchAsync(
		string? @namespace = "default",
		string? id = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Applies a cluster machine (create or update)
	/// </summary>
	Task<ClusterMachine> ApplyAsync(
		ClusterMachine clusterMachine,
		bool dryRun = false,
		CancellationToken cancellationToken = default);
}
