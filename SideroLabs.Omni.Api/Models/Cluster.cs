namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a cluster in the Omni system
/// </summary>
public class Cluster
{
	/// <summary>
	/// Unique identifier for the cluster
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the cluster
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Cluster specification
	/// </summary>
	public ClusterSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the cluster
	/// </summary>
	public ClusterStatus Status { get; set; } = new();

	/// <summary>
	/// Timestamp when the cluster was created (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the cluster was last updated (Unix timestamp)
	/// </summary>
	public long UpdatedAt { get; set; }
}

/// <summary>
/// Cluster specification defining desired state
/// </summary>
public class ClusterSpec
{
	/// <summary>
	/// Kubernetes version for the cluster
	/// </summary>
	public string KubernetesVersion { get; set; } = string.Empty;

	/// <summary>
	/// Talos version for the cluster
	/// </summary>
	public string TalosVersion { get; set; } = string.Empty;

	/// <summary>
	/// List of enabled features for the cluster
	/// </summary>
	public List<string> Features { get; set; } = new();
}

/// <summary>
/// Current status of a cluster
/// </summary>
public class ClusterStatus
{
	/// <summary>
	/// Current phase of the cluster lifecycle
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Whether the cluster is ready for use
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// List of current conditions affecting the cluster
	/// </summary>
	public List<string> Conditions { get; set; } = new();
}