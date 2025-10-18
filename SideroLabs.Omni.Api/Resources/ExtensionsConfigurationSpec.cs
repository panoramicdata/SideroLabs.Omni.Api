namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Specification for an ExtensionsConfiguration resource
/// </summary>
public class ExtensionsConfigurationSpec
{
	/// <summary>
	/// List of Talos extensions to install
	/// </summary>
	public List<string>? Extensions { get; set; }

	/// <summary>
	/// Talos version for which these extensions are configured
	/// </summary>
	public string? TalosVersion { get; set; }
}
