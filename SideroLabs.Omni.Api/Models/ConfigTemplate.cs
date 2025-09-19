namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a configuration template in the Omni system
/// </summary>
public class ConfigTemplate
{
	/// <summary>
	/// Unique identifier for the configuration template
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the configuration template
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Description of the configuration template
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Type of configuration (cluster, machine, network, etc.)
	/// </summary>
	public ConfigTemplateType Type { get; set; }

	/// <summary>
	/// Template specification
	/// </summary>
	public ConfigTemplateSpec Spec { get; set; } = new();

	/// <summary>
	/// Template metadata
	/// </summary>
	public Dictionary<string, string> Labels { get; set; } = new();

	/// <summary>
	/// Timestamp when the template was created (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the template was last updated (Unix timestamp)
	/// </summary>
	public long UpdatedAt { get; set; }
}
