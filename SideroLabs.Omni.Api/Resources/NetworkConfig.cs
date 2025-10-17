namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Placeholder network configuration for ClusterSpec. Replace with full model later.
/// </summary>
public class NetworkConfig
{
	public string Cidr { get; set; } = string.Empty;
	public string? ServiceCidr { get; set; }
	public string? PodCidr { get; set; }
}
