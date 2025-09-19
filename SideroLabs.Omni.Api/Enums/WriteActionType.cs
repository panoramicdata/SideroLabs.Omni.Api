namespace SideroLabs.Omni.Api.Enums;

/// <summary>
/// Types of write actions that can be performed
/// </summary>
public enum WriteActionType
{
	/// <summary>
	/// Create a new resource
	/// </summary>
	Create,

	/// <summary>
	/// Update an existing resource
	/// </summary>
	Update,

	/// <summary>
	/// Delete an existing resource
	/// </summary>
	Delete,

	/// <summary>
	/// Start or trigger an operation
	/// </summary>
	Start,

	/// <summary>
	/// Stop or cancel an operation
	/// </summary>
	Stop
}
