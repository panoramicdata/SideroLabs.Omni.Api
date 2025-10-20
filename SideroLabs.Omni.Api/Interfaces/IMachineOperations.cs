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
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a specific machine by ID
	/// </summary>
	Task<Machine> GetAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new machine
	/// </summary>
	Task<Machine> CreateAsync(
		Machine machine,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing machine
	/// </summary>
	Task<Machine> UpdateAsync(
		Machine machine,
		string? currentVersion = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a machine by ID
	/// </summary>
	Task DeleteAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Watches for machine changes
	/// </summary>
	IAsyncEnumerable<ResourceEvent<Machine>> WatchAsync(
		string? @namespace = "default",
		string? id = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Applies a machine (create or update)
	/// </summary>
	Task<Machine> ApplyAsync(
		Machine machine,
		bool dryRun = false,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Locks a machine to prevent modifications
	/// </summary>
	Task LockAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Unlocks a machine to allow modifications
	/// </summary>
	Task UnlockAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default);
}
