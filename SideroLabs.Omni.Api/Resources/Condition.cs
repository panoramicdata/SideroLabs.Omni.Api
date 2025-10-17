namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents a condition in a resource status
/// </summary>
public class Condition
{
	/// <summary>
	/// Type of the condition (e.g., "Ready", "Available")
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// Status of the condition ("True", "False", "Unknown")
	/// </summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>
	/// Last time the condition transitioned
	/// </summary>
	public DateTime? LastTransitionTime { get; set; }

	/// <summary>
	/// Reason for the condition's last transition
	/// </summary>
	public string? Reason { get; set; }

	/// <summary>
	/// Human-readable message indicating details about the transition
	/// </summary>
	public string? Message { get; set; }

	/// <summary>
	/// Gets whether the condition is true
	/// </summary>
	public bool IsTrue => Status == "True";

	/// <summary>
	/// Gets whether the condition is false
	/// </summary>
	public bool IsFalse => Status == "False";

	/// <summary>
	/// Gets whether the condition is unknown
	/// </summary>
	public bool IsUnknown => Status == "Unknown";
}
