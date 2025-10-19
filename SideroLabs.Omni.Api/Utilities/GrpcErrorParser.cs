using System.Text.RegularExpressions;
using Grpc.Core;

namespace SideroLabs.Omni.Api.Utilities;

/// <summary>
/// Utility for parsing gRPC errors and extracting HTTP details
/// </summary>
internal static partial class GrpcErrorParser
{
	// Regex to extract HTTP status code from gRPC error details
	[GeneratedRegex(@"HTTP status code:\s*(\d+)", RegexOptions.IgnoreCase)]
	private static partial Regex HttpStatusCodeRegex();

	/// <summary>
	/// Attempts to extract HTTP status code from RpcException
	/// </summary>
	/// <param name="exception">The RpcException to parse</param>
	/// <returns>HTTP status code if found, null otherwise</returns>
	internal static int? ExtractHttpStatusCode(RpcException exception)
	{
		if (exception == null)
		{
			return null;
		}

		var detail = exception.Status.Detail;
		if (string.IsNullOrEmpty(detail))
		{
			return null;
		}

		var match = HttpStatusCodeRegex().Match(detail);
		if (match.Success && int.TryParse(match.Groups[1].Value, out var statusCode))
		{
			return statusCode;
		}

		return null;
	}

	/// <summary>
	/// Attempts to extract HTTP response body from RpcException trailers
	/// </summary>
	/// <param name="exception">The RpcException to parse</param>
	/// <returns>HTTP response body if found, null otherwise</returns>
	internal static string? ExtractHttpResponseBody(RpcException exception)
	{
		if (exception == null)
		{
			return null;
		}

		// Check trailers for response body
		var trailers = exception.Trailers;
		if (trailers != null)
		{
			// Common trailer keys that might contain response body
			var bodyKeys = new[] { "grpc-status-details-bin", "http-body", "response-body", "error-details" };
			
			foreach (var key in bodyKeys)
			{
				var entry = trailers.Get(key);
				if (entry != null)
				{
					if (entry.IsBinary)
					{
						try
						{
							return System.Text.Encoding.UTF8.GetString(entry.ValueBytes);
						}
						catch
						{
							// If not valid UTF-8, return hex representation
							return BitConverter.ToString(entry.ValueBytes).Replace("-", " ");
						}
					}
					else
					{
						return entry.Value;
					}
				}
			}
		}

		// If no body in trailers, check if the detail message contains useful information
		var detail = exception.Status.Detail;
		if (!string.IsNullOrEmpty(detail) && detail.Length > 50)
		{
			// The detail itself might be the HTTP response
			return detail;
		}

		return null;
	}

	/// <summary>
	/// Creates a detailed error message including HTTP details
	/// </summary>
	/// <param name="exception">The RpcException to format</param>
	/// <returns>Formatted error message</returns>
	internal static string FormatDetailedError(RpcException exception)
	{
		var httpStatusCode = ExtractHttpStatusCode(exception);
		var httpResponseBody = ExtractHttpResponseBody(exception);

		var message = $"gRPC Error: {exception.StatusCode} - {exception.Status.Detail}";

		if (httpStatusCode.HasValue)
		{
			message += $"\nHTTP Status Code: {httpStatusCode}";
		}

		if (!string.IsNullOrEmpty(httpResponseBody))
		{
			message += $"\nHTTP Response Body:\n{httpResponseBody}";
		}

		if (exception.Trailers != null && exception.Trailers.Count > 0)
		{
			message += "\n\nResponse Trailers:";
			foreach (var trailer in exception.Trailers)
			{
				if (trailer.IsBinary)
				{
					message += $"\n  {trailer.Key}: [binary data, {trailer.ValueBytes.Length} bytes]";
				}
				else
				{
					message += $"\n  {trailer.Key}: {trailer.Value}";
				}
			}
		}

		return message;
	}

	/// <summary>
	/// Checks if the exception represents an HTTP 4xx or 5xx error
	/// </summary>
	/// <param name="exception">The RpcException to check</param>
	/// <returns>True if this is an HTTP client or server error</returns>
	internal static bool IsHttpError(RpcException exception)
	{
		var statusCode = ExtractHttpStatusCode(exception);
		return statusCode.HasValue && (statusCode >= 400 && statusCode < 600);
	}
}
