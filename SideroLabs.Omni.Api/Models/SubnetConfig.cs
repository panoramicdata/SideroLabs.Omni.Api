namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Subnet configuration
/// </summary>
public class SubnetConfig
{
	/// <summary>
	/// Subnet CIDR
	/// </summary>
	public string Subnet { get; set; } = string.Empty;

	/// <summary>
	/// Gateway IP
	/// </summary>
	public string? Gateway { get; set; }

	/// <summary>
	/// IP range start
	/// </summary>
	public string? RangeStart { get; set; }

	/// <summary>
	/// IP range end
	/// </summary>
	public string? RangeEnd { get; set; }
}
