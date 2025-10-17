namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Hardware information for a machine
/// </summary>
public class MachineHardware
{
	/// <summary>
	/// CPU information
	/// </summary>
	public string? Processor { get; set; }

	/// <summary>
	/// Number of CPU cores
	/// </summary>
	public int? Cores { get; set; }

	/// <summary>
	/// Total memory in bytes
	/// </summary>
	public long? Memory { get; set; }

	/// <summary>
	/// Storage devices
	/// </summary>
	public List<StorageDevice>? Storage { get; set; }

	/// <summary>
	/// System manufacturer
	/// </summary>
	public string? Manufacturer { get; set; }

	/// <summary>
	/// System model
	/// </summary>
	public string? Model { get; set; }

	/// <summary>
	/// System serial number
	/// </summary>
	public string? SerialNumber { get; set; }
}
