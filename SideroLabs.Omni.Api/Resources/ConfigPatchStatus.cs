namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Status information for a ConfigPatch resource
/// </summary>
public class ConfigPatchStatus
{
	/// <summary>
	/// Whether the patch has been applied
	/// </summary>
	public bool Applied { get; set; }

	/// <summary>
	/// Error message if patch application failed
	/// </summary>
	public string? Error { get; set; }

	/// <summary>
	/// Last time the patch was applied
	/// </summary>
	public DateTime? LastApplied { get; set; }
}
