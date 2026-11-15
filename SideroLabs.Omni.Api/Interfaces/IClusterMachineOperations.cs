using System.Runtime.InteropServices;
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
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific cluster machine by ID
	/// </summary>
	Task<ClusterMachine> GetAsync(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new cluster machine
	/// </summary>
	Task<ClusterMachine> CreateAsync(
		ClusterMachine clusterMachine,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing cluster machine
	/// </summary>
	Task<ClusterMachine> UpdateAsync(
		ClusterMachine clusterMachine,
		[Optional] string? currentVersion,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a cluster machine by ID
	/// </summary>
	Task DeleteAsync(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Watches for cluster machine changes
	/// </summary>
	IAsyncEnumerable<ResourceEvent<ClusterMachine>> WatchAsync(
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] string? id,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Applies a cluster machine (create or update)
	/// </summary>
	Task<ClusterMachine> ApplyAsync(
		ClusterMachine clusterMachine,
		[Optional] bool dryRun,
		[Optional] CancellationToken cancellationToken);
}
