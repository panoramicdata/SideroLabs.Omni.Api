namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Configuration template specification
/// </summary>
public class ConfigTemplateSpec
{
	/// <summary>
	/// Template version
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// Configuration parameters
	/// </summary>
	public Dictionary<string, object> Parameters { get; set; } = new();

	/// <summary>
	/// Template content (YAML, JSON, etc.)
	/// </summary>
	public string Content { get; set; } = string.Empty;

	/// <summary>
	/// Template variables with default values
	/// </summary>
	public Dictionary<string, TemplateVariable> Variables { get; set; } = new();

	/// <summary>
	/// Dependencies on other templates
	/// </summary>
	public List<string> Dependencies { get; set; } = new();
}
