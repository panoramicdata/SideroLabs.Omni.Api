namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Current resource usage
/// </summary>
public class ResourceUsage
{
	/// <summary>
	/// Current number of clusters
	/// </summary>
	public int Clusters { get; set; }

	/// <summary>
	/// Current number of machines
	/// </summary>
	public int Machines { get; set; }

	/// <summary>
	/// Current CPU cores in use
	/// </summary>
	public double CpuCores { get; set; }

	/// <summary>
	/// Current memory in GB in use
	/// </summary>
	public double MemoryGb { get; set; }

	/// <summary>
	/// Current storage in GB in use
	/// </summary>
	public double StorageGb { get; set; }
}
