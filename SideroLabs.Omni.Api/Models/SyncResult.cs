namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Describes the outcome of applying or simulating an Omni template resource during sync.
/// </summary>
public class SyncResult
{
	/// <summary>
	/// Gets the action Omni attempted for the resource, such as create, update, or unchanged.
	/// </summary>
	public required string Action { get; init; }

	/// <summary>
	/// Gets the resource instance involved in the sync action.
	/// </summary>
	public required object Resource { get; init; }

	/// <summary>
	/// Gets an error message when the sync action fails; otherwise <see langword="null"/>.
	/// </summary>
	public string? Error { get; internal set; }
}
