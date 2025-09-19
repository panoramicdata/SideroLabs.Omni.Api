namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a workspace in the Omni system
/// </summary>
public class Workspace
{
	/// <summary>
	/// Unique identifier for the workspace
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the workspace
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Description of the workspace
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Workspace specification
	/// </summary>
	public WorkspaceSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the workspace
	/// </summary>
	public WorkspaceStatus Status { get; set; } = new();

	/// <summary>
	/// Timestamp when the workspace was created (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the workspace was last updated (Unix timestamp)
	/// </summary>
	public long UpdatedAt { get; set; }
}
