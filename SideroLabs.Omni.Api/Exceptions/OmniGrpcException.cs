using Grpc.Core;

namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Exception thrown when a gRPC operation fails
/// </summary>
/// <param name="operationName">The operation name</param>
/// <param name="grpcStatusCode">The gRPC status code</param>
/// <param name="grpcStatusDetail">The gRPC status details</param>
/// <param name="methodPath">The gRPC method path</param>
/// <param name="innerException">The inner exception</param>
public class OmniGrpcException(
	string operationName,
	StatusCode grpcStatusCode,
	string? grpcStatusDetail = null,
	string? methodPath = null,
	Exception? innerException = null) : OmniException(operationName, FormatMessage(operationName, grpcStatusCode, grpcStatusDetail), methodPath, innerException)
{
	/// <summary>
	/// Gets the gRPC status code
	/// </summary>
	public StatusCode GrpcStatusCode { get; } = grpcStatusCode;

	/// <summary>
	/// Gets the gRPC status details
	/// </summary>
	public string? GrpcStatusDetail { get; } = grpcStatusDetail;

	private static string FormatMessage(string operationName, StatusCode statusCode, string? detail)
	{
		var message = $"gRPC operation '{operationName}' failed with status {statusCode}";
		return string.IsNullOrEmpty(detail) ? message : $"{message}: {detail}";
	}
}
