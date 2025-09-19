namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Attribute to mark methods that perform write actions (create, update, delete operations)
/// These methods will be blocked when the client is in read-only mode
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class IsWriteActionAttribute : Attribute
{
	/// <summary>
	/// The type of write operation being performed
	/// </summary>
	public WriteActionType ActionType { get; }

	/// <summary>
	/// Optional description of the write action
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Creates a new IsWriteActionAttribute
	/// </summary>
	/// <param name="actionType">The type of write operation</param>
	public IsWriteActionAttribute(WriteActionType actionType)
	{
		ActionType = actionType;
	}
}

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
