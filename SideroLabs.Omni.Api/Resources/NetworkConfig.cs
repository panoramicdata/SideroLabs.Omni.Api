namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Placeholder network configuration for ClusterSpec. Replace with full model later.
/// </summary>
public class NetworkConfig
{
	/// <summary>
	/// Gets or sets the primary cluster CIDR used as the base network range.
	/// </summary>
	public string Cidr { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the Kubernetes service CIDR allocated for virtual service IPs.
	/// </summary>
	public string? ServiceCidr { get; set; }

	/// <summary>
	/// Gets or sets the Kubernetes pod CIDR allocated for workload pod addresses.
	/// </summary>
	public string? PodCidr { get; set; }
}
