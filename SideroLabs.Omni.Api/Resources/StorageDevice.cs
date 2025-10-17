namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Storage device information
/// </summary>
public class StorageDevice
{
	/// <summary>
	/// Device name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Size in bytes
	/// </summary>
	public long Size { get; set; }

	/// <summary>
	/// Device type (e.g., "SSD", "HDD", "NVMe")
	/// </summary>
	public string? Type { get; set; }

	/// <summary>
	/// Model name
	/// </summary>
	public string? Model { get; set; }
}
