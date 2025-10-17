using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Helper class for executing gRPC calls with consistent logging and error handling
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="createCallOptions">Function to create call options</param>
internal class GrpcCallHelper(ILogger logger, Func<string, CallOptions> createCallOptions)
{
	private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	private readonly Func<string, CallOptions> _createCallOptions = createCallOptions ?? throw new ArgumentNullException(nameof(createCallOptions));

	/// <summary>
	/// Executes a unary gRPC call with consistent logging
	/// </summary>
	/// <typeparam name="TRequest">The request type</typeparam>
	/// <typeparam name="TResponse">The response type</typeparam>
	/// <param name="request">The request object</param>
	/// <param name="grpcCall">The gRPC call function</param>
	/// <param name="methodPath">The gRPC method path</param>
	/// <param name="operationName">The operation name for logging</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The response object</returns>
	internal async Task<TResponse> ExecuteCallAsync<TRequest, TResponse>(
		TRequest request,
		Func<TRequest, CallOptions, AsyncUnaryCall<TResponse>> grpcCall,
		string methodPath,
		string operationName,
		CancellationToken cancellationToken)
	{
		_logger.LogDebug("Executing {OperationName}", operationName);

		var callOptions = _createCallOptions(methodPath);
		var asyncCall = grpcCall(request, callOptions);
		var response = await asyncCall.ResponseAsync.WaitAsync(cancellationToken);

		_logger.LogDebug("Completed {OperationName}", operationName);
		return response;
	}

	/// <summary>
	/// Executes a streaming gRPC call with consistent logging
	/// </summary>
	/// <typeparam name="TRequest">The request type</typeparam>
	/// <typeparam name="TResponse">The response type</typeparam>
	/// <param name="request">The request object</param>
	/// <param name="grpcCall">The gRPC call function</param>
	/// <param name="methodPath">The gRPC method path</param>
	/// <param name="operationName">The operation name for logging</param>
	/// <returns>The streaming call</returns>
	internal AsyncServerStreamingCall<TResponse> ExecuteStreamingCall<TRequest, TResponse>(
		TRequest request,
		Func<TRequest, CallOptions, AsyncServerStreamingCall<TResponse>> grpcCall,
		string methodPath,
		string operationName)
	{
		_logger.LogDebug("Starting streaming {OperationName}", operationName);

		var callOptions = _createCallOptions(methodPath);
		return grpcCall(request, callOptions);
	}
}
