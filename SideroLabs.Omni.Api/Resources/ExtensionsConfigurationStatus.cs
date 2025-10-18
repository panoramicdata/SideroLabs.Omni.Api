namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Status information for an ExtensionsConfiguration resource
/// </summary>
public class ExtensionsConfigurationStatus
{
	/// <summary>
	/// Whether the extensions are properly configured
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// Number of extensions successfully installed
	/// </summary>
	public int InstalledCount { get; set; }

	/// <summary>
	/// Number of extensions that failed to install
	/// </summary>
	public int FailedCount { get; set; }

	/// <summary>
	/// List of status conditions
	/// </summary>
	public List<Condition>? Conditions { get; set; }

	/// <summary>
	/// Last time the configuration was updated
	/// </summary>
	public DateTime? LastUpdateTime { get; set; }
}
