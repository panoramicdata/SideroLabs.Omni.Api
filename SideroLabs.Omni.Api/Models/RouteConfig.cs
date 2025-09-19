namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Route configuration
/// </summary>
public class RouteConfig
{
	/// <summary>
	/// Destination network
	/// </summary>
	public string Destination { get; set; } = string.Empty;

	/// <summary>
	/// Gateway IP
	/// </summary>
	public string Gateway { get; set; } = string.Empty;

	/// <summary>
	/// Route metric
	/// </summary>
	public int? Metric { get; set; }
}
