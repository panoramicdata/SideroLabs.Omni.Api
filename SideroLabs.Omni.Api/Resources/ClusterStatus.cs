namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Status information for a Cluster resource
/// </summary>
public class ClusterStatus
{
	/// <summary>
	/// Whether the cluster is ready
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// Current phase of the cluster
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Kubernetes API server endpoint
	/// </summary>
	public string? ApiServerEndpoint { get; set; }

	/// <summary>
	/// Number of available control plane nodes
	/// </summary>
	public int ControlPlaneCount { get; set; }

	/// <summary>
	/// Number of available worker nodes
	/// </summary>
	public int WorkerCount { get; set; }

	/// <summary>
	/// Status conditions for the cluster
	/// </summary>
	public List<Condition>? Conditions { get; set; }

	/// <summary>
	/// Last time the status was updated
	/// </summary>
	public DateTime? LastUpdateTime { get; set; }

	/// <summary>
	/// Current cluster health status
	/// </summary>
	public string? Health { get; set; }
}
