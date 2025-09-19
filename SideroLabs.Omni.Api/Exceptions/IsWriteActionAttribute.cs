using SideroLabs.Omni.Api.Enums;

namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Attribute to mark methods that perform write actions (create, update, delete operations)
/// These methods will be blocked when the client is in read-only mode
/// </summary>
/// <remarks>
/// Creates a new IsWriteActionAttribute
/// </remarks>
/// <param name="actionType">The type of write operation</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class IsWriteActionAttribute(WriteActionType actionType) : Attribute
{
	/// <summary>
	/// The type of write operation being performed
	/// </summary>
	public WriteActionType ActionType { get; } = actionType;

	/// <summary>
	/// Optional description of the write action
	/// </summary>
	public string? Description { get; set; }
}
