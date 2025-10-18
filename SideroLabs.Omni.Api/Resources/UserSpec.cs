namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Specification for a User resource
/// </summary>
public class UserSpec
{
	/// <summary>
	/// Gets or sets the user role (e.g., Admin, Operator, Reader)
	/// </summary>
	public string Role { get; set; } = "";
}
