namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Specification for a cluster machine resource
/// </summary>
public class ClusterMachineSpec
{
	/// <summary>
	/// ID of the cluster this machine belongs to
	/// </summary>
	public string ClusterId { get; set; } = string.Empty;

	/// <summary>
	/// Role of the machine in the cluster (e.g., "controlplane", "worker")
	/// </summary>
	public string Role { get; set; } = string.Empty;

	/// <summary>
	/// Machine configuration patches
	/// </summary>
	public List<string>? Patches { get; set; }

	/// <summary>
	/// Additional metadata or configuration
	/// </summary>
	public Dictionary<string, string>? Config { get; set; }
}
