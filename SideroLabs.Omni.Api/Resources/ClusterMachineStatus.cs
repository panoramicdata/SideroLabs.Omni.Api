namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Status information for a cluster machine
/// </summary>
public class ClusterMachineStatus
{
	/// <summary>
	/// Current phase of the cluster machine
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Whether the machine is ready
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// Configuration generation version
	/// </summary>
	public int ConfigGeneration { get; set; }

	/// <summary>
	/// Last error message if any
	/// </summary>
	public string? Error { get; set; }

	/// <summary>
	/// Status conditions
	/// </summary>
	public List<Condition>? Conditions { get; set; }
}
