namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Resource quota definitions
/// </summary>
public class ResourceQuota
{
	/// <summary>
	/// Maximum number of clusters allowed
	/// </summary>
	public int? MaxClusters { get; set; }

	/// <summary>
	/// Maximum number of machines allowed
	/// </summary>
	public int? MaxMachines { get; set; }

	/// <summary>
	/// Maximum CPU cores allowed
	/// </summary>
	public double? MaxCpuCores { get; set; }

	/// <summary>
	/// Maximum memory in GB allowed
	/// </summary>
	public double? MaxMemoryGb { get; set; }

	/// <summary>
	/// Maximum storage in GB allowed
	/// </summary>
	public double? MaxStorageGb { get; set; }
}
