using System.Runtime.InteropServices;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Operations for Machine resources
/// </summary>
public interface IMachineOperations
{
	/// <summary>
	/// Lists all machines
	/// </summary>
	IAsyncEnumerable<Machine> ListAsync(
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific machine by ID
	/// </summary>
	Task<Machine> GetAsync(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new machine
	/// </summary>
	Task<Machine> CreateAsync(
		Machine machine,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing machine
	/// </summary>
	Task<Machine> UpdateAsync(
		Machine machine,
		[Optional] string? currentVersion,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a machine by ID
	/// </summary>
	Task DeleteAsync(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Watches for machine changes
	/// </summary>
	IAsyncEnumerable<ResourceEvent<Machine>> WatchAsync(
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] string? id,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Applies a machine (create or update)
	/// </summary>
	Task<Machine> ApplyAsync(
		Machine machine,
		[Optional] bool dryRun,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Locks a machine to prevent modifications
	/// </summary>
	Task LockAsync(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Unlocks a machine to allow modifications
	/// </summary>
	Task UnlockAsync(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken);
}
