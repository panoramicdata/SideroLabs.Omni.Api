using Grpc.Core;

namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Exception thrown when a gRPC operation fails
/// </summary>
/// <param name="operationName">The operation name</param>
/// <param name="grpcStatusCode">The gRPC status code</param>
/// <param name="grpcStatusDetail">The gRPC status details</param>
/// <param name="methodPath">The gRPC method path</param>
/// <param name="httpStatusCode">The HTTP status code (if applicable)</param>
/// <param name="httpResponseBody">The HTTP response body (if applicable)</param>
/// <param name="innerException">The inner exception</param>
public class OmniGrpcException(
	string operationName,
	StatusCode grpcStatusCode,
	string? grpcStatusDetail = null,
	string? methodPath = null,
	int? httpStatusCode = null,
	string? httpResponseBody = null,
	Exception? innerException = null) : OmniException(
		operationName,
		FormatMessage(operationName, grpcStatusCode, grpcStatusDetail, httpStatusCode, httpResponseBody),
		methodPath,
		innerException)
{
	/// <summary>
	/// Gets the gRPC status code
	/// </summary>
	public StatusCode GrpcStatusCode { get; } = grpcStatusCode;

	/// <summary>
	/// Gets the gRPC status details
	/// </summary>
	public string? GrpcStatusDetail { get; } = grpcStatusDetail;

	/// <summary>
	/// Gets the HTTP status code if the error originated from an HTTP response
	/// </summary>
	public int? HttpStatusCode { get; } = httpStatusCode;

	/// <summary>
	/// Gets the HTTP response body if the error originated from an HTTP response
	/// </summary>
	public string? HttpResponseBody { get; } = httpResponseBody;

	/// <summary>
	/// Gets whether this exception includes HTTP error details
	/// </summary>
	public bool HasHttpErrorDetails => HttpStatusCode.HasValue || !string.IsNullOrEmpty(HttpResponseBody);

	private static string FormatMessage(
		string operationName,
		StatusCode statusCode,
		string? detail,
		int? httpStatusCode,
		string? httpResponseBody)
	{
		var message = $"gRPC operation '{operationName}' failed with status {statusCode}";

		if (!string.IsNullOrEmpty(detail))
		{
			message = $"{message}: {detail}";
		}

		if (httpStatusCode.HasValue)
		{
			message = $"{message} (HTTP {httpStatusCode})";
		}

		if (!string.IsNullOrEmpty(httpResponseBody))
		{
			// Truncate very long response bodies
			var bodyPreview = httpResponseBody.Length > 500
				? httpResponseBody[..500] + "... (truncated)"
				: httpResponseBody;
			message = $"{message}\nHTTP Response Body: {bodyPreview}";
		}

		return message;
	}
}
