using System.Reflection;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Base class for all Omni service implementations
/// </summary>
/// <remarks>
/// Initializes a new instance of the OmniServiceBase class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="logger">Logger instance</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
public abstract class OmniServiceBase(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator)
{
	protected readonly OmniClientOptions Options = options ?? throw new ArgumentNullException(nameof(options));
	protected readonly ILogger Logger = options.Logger;
	protected readonly GrpcChannel Channel = channel ?? throw new ArgumentNullException(nameof(channel));
	protected readonly OmniAuthenticator? Authenticator = authenticator;

	/// <summary>
	/// Creates call options with authentication and timeout
	/// </summary>
	/// <param name="method">The gRPC method name</param>
	/// <returns>Configured call options</returns>
	protected CallOptions CreateCallOptions(string method)
	{
		var headers = new Metadata();

		// Add PGP-based authentication if available
		if (Authenticator != null)
		{
			Authenticator.SignRequest(headers, method);
		}
		else
		{
			Logger.LogWarning("No authenticator available for method: {Method}", method);
		}

		var deadline = DateTime.UtcNow.AddSeconds(Options.TimeoutSeconds);
		return new CallOptions(headers: headers, deadline: deadline);
	}

	/// <summary>
	/// Checks if a write operation is allowed in the current client configuration
	/// </summary>
	/// <param name="operation">The operation being attempted</param>
	/// <param name="resourceType">The type of resource being operated on</param>
	/// <exception cref="ReadOnlyModeException">Thrown when attempting a write operation in read-only mode</exception>
	protected void EnsureWriteOperationAllowed(string operation, string resourceType)
	{
		if (Options.IsReadOnly)
		{
			Logger.LogWarning("Blocking {Operation} {ResourceType} operation due to read-only mode", operation, resourceType);
			throw new ReadOnlyModeException(operation, resourceType);
		}
	}

	/// <summary>
	/// Checks if the calling method has a write action attribute and enforces read-only mode if necessary
	/// </summary>
	/// <param name="resourceType">The type of resource being operated on</param>
	/// <exception cref="ReadOnlyModeException">Thrown when attempting a write operation in read-only mode</exception>
	protected void EnsureWriteActionAllowed(string resourceType)
	{
		if (!Options.IsReadOnly)
		{
			return;
		}

		// Get the calling method
		var stackTrace = new System.Diagnostics.StackTrace();
		var callingMethod = stackTrace.GetFrame(1)?.GetMethod();

		if (callingMethod != null)
		{
			var writeActionAttribute = callingMethod.GetCustomAttribute<IsWriteActionAttribute>();
			if (writeActionAttribute != null)
			{
				var operation = writeActionAttribute.ActionType.ToString().ToLowerInvariant();
				Logger.LogWarning("Blocking {Operation} {ResourceType} operation due to read-only mode", operation, resourceType);
				throw new ReadOnlyModeException(operation, resourceType);
			}
		}
	}
}
