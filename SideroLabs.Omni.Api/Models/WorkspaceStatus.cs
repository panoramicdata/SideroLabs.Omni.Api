namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Current status of a workspace
/// </summary>
public class WorkspaceStatus
{
	/// <summary>
	/// Current phase of the workspace lifecycle
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Whether the workspace is ready for use
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// Number of clusters in the workspace
	/// </summary>
	public int ClusterCount { get; set; }

	/// <summary>
	/// Current resource usage
	/// </summary>
	public ResourceUsage ResourceUsage { get; set; } = new();
}
