namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Workspace specification defining desired state
/// </summary>
public class WorkspaceSpec
{
	/// <summary>
	/// Resource quotas for the workspace
	/// </summary>
	public ResourceQuota ResourceQuota { get; set; } = new();

	/// <summary>
	/// Labels for the workspace
	/// </summary>
	public Dictionary<string, string> Labels { get; set; } = new();

	/// <summary>
	/// Annotations for the workspace
	/// </summary>
	public Dictionary<string, string> Annotations { get; set; } = new();

	/// <summary>
	/// Default cluster configuration template
	/// </summary>
	public string DefaultClusterTemplate { get; set; } = string.Empty;
}
