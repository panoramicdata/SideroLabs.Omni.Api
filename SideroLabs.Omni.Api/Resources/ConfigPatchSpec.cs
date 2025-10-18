namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Specification for a ConfigPatch resource
/// </summary>
public class ConfigPatchSpec
{
	/// <summary>
	/// YAML configuration patch data
	/// </summary>
	public string Data { get; set; } = string.Empty;
}
