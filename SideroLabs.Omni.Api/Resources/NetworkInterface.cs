namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Network interface information
/// </summary>
public class NetworkInterface
{
	/// <summary>
	/// Interface name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// MAC address
	/// </summary>
	public string? MacAddress { get; set; }

	/// <summary>
	/// IP addresses assigned to this interface
	/// </summary>
	public List<string>? IpAddresses { get; set; }

	/// <summary>
	/// Link state (up/down)
	/// </summary>
	public bool? Up { get; set; }

	/// <summary>
	/// Link speed in Mbps
	/// </summary>
	public int? Speed { get; set; }
}
