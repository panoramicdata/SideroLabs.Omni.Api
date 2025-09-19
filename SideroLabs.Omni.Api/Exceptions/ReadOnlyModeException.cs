namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Exception thrown when attempting to execute a write action while the client is in read-only mode
/// </summary>
public class ReadOnlyModeException : InvalidOperationException
{
	/// <summary>
	/// The operation that was attempted
	/// </summary>
	public string Operation { get; }

	/// <summary>
	/// The resource type being operated on
	/// </summary>
	public string ResourceType { get; }

	/// <summary>
	/// Creates a new ReadOnlyModeException
	/// </summary>
	/// <param name="operation">The operation that was attempted</param>
	/// <param name="resourceType">The resource type being operated on</param>
	public ReadOnlyModeException(string operation, string resourceType)
		: base($"Cannot perform {operation} on {resourceType} when client is in read-only mode")
	{
		Operation = operation;
		ResourceType = resourceType;
	}

	/// <summary>
	/// Creates a new ReadOnlyModeException with a custom message
	/// </summary>
	/// <param name="operation">The operation that was attempted</param>
	/// <param name="resourceType">The resource type being operated on</param>
	/// <param name="message">Custom error message</param>
	public ReadOnlyModeException(string operation, string resourceType, string message)
		: base(message)
	{
		Operation = operation;
		ResourceType = resourceType;
	}
}
