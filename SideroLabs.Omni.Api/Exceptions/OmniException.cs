using Grpc.Core;

namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Base exception for all Omni-specific exceptions
/// </summary>
/// <param name="operationName">The operation name</param>
/// <param name="message">The exception message</param>
/// <param name="methodPath">The gRPC method path</param>
/// <param name="innerException">The inner exception</param>
public abstract class OmniException(string operationName, string message, string? methodPath = null, Exception? innerException = null) : Exception(message, innerException)
{
	/// <summary>
	/// Gets the operation name that caused the exception
	/// </summary>
	public string OperationName { get; } = operationName ?? throw new ArgumentNullException(nameof(operationName));

	/// <summary>
	/// Gets the gRPC method path if applicable
	/// </summary>
	public string? MethodPath { get; } = methodPath;
}
