namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Template variable definition
/// </summary>
public class TemplateVariable
{
	/// <summary>
	/// Variable type (string, number, boolean, etc.)
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// Default value for the variable
	/// </summary>
	public object? DefaultValue { get; set; }

	/// <summary>
	/// Description of the variable
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Whether the variable is required
	/// </summary>
	public bool Required { get; set; }

	/// <summary>
	/// Validation rules for the variable
	/// </summary>
	public List<string> ValidationRules { get; set; } = new();
}
