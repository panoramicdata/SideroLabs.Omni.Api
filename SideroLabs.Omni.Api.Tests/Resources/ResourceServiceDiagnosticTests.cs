using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Resources;
using System.Text.RegularExpressions;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Diagnostic tests to verify ResourceService endpoint accessibility and capture HTTP response bodies
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Diagnostic")]
public partial class ResourceServiceDiagnosticTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[GeneratedRegex(@"HTTP status code:\s*(\d+)", RegexOptions.IgnoreCase)]
	private static partial Regex HttpStatusCodeRegex();

	private static int? ExtractHttpStatusCode(Grpc.Core.RpcException ex)
	{
		var match = HttpStatusCodeRegex().Match(ex.Status.Detail);
		return match.Success && int.TryParse(match.Groups[1].Value, out var code) ? code : null;
	}

	private static string? ExtractHttpResponseBody(Grpc.Core.RpcException ex)
	{
		// Check trailers for response body
		if (ex.Trailers != null)
		{
			// Try multiple common trailer keys
			var bodyKeys = new[] { "grpc-status-details-bin", "http-body", "response-body", "error-details", "grpc-message" };
			foreach (var key in bodyKeys)
			{
				var entry = ex.Trailers.Get(key);
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
							return $"[Binary data, {entry.ValueBytes.Length} bytes - not UTF8]";
						}
					}
					return entry.Value;
				}
			}
		}
		return null;
	}

	[Fact]
	public async Task ResourceService_ListClusters_CaptureHttpResponseBody()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		try
		{
			Logger.LogInformation("🔍 Testing ResourceService endpoint with read-only List operation");
			Logger.LogInformation("📍 Endpoint: {BaseUrl}", OmniClient.BaseUrl);
			Logger.LogInformation("🔐 Identity: {Identity}", OmniClient.Identity ?? "(none)");
			Logger.LogInformation("📋 Attempting to list Cluster resources...");
			
			var clusters = new List<Cluster>();
			await foreach (var cluster in OmniClient.Resources.ListAsync<Cluster>(
				@namespace: "default",
				cancellationToken: CancellationToken))
			{
				clusters.Add(cluster);
			}

			Logger.LogInformation("✅ Successfully listed {Count} clusters via ResourceService!", clusters.Count);
			
			foreach (var cluster in clusters.Take(3))
			{
				Logger.LogInformation("  - Cluster: {Name} (Kubernetes: {Version})", 
					cluster.Metadata.Id, 
					cluster.Spec.KubernetesVersion);
			}

			Assert.True(true, "ResourceService List operation succeeded");
		}
		catch (Grpc.Core.RpcException ex)
		{
			Logger.LogError(ex, "❌ gRPC Error: {Status} - {Detail}", ex.StatusCode, ex.Status.Detail);
			
			// Extract HTTP details
			var httpStatus = ExtractHttpStatusCode(ex);
			var httpBody = ExtractHttpResponseBody(ex);
			
			Logger.LogError("═══════════════════════════════════════════════════════════");
			Logger.LogError("📊 DETAILED ERROR ANALYSIS:");
			Logger.LogError("═══════════════════════════════════════════════════════════");
			Logger.LogError("  gRPC Status Code: {StatusCode}", ex.StatusCode);
			Logger.LogError("  gRPC Status Detail: {Detail}", ex.Status.Detail);
			
			if (httpStatus.HasValue)
			{
				Logger.LogError("  HTTP Status Code: {HttpStatus}", httpStatus.Value);
			}
			
			if (!string.IsNullOrEmpty(httpBody))
			{
				Logger.LogError("═══════════════════════════════════════════════════════════");
				Logger.LogError("⭐⭐⭐ HTTP RESPONSE BODY FOUND ⭐⭐⭐");
				Logger.LogError("═══════════════════════════════════════════════════════════");
				Logger.LogError("{Body}", httpBody);
				Logger.LogError("═══════════════════════════════════════════════════════════");
			}
			else
			{
				Logger.LogWarning("  ⚠️ No HTTP response body found in exception trailers");
			}

			// Log ALL trailers for debugging
			if (ex.Trailers != null && ex.Trailers.Count > 0)
			{
				Logger.LogError("  Response Trailers ({Count}):", ex.Trailers.Count);
				foreach (var trailer in ex.Trailers)
				{
					if (trailer.IsBinary)
					{
						try
						{
							var text = System.Text.Encoding.UTF8.GetString(trailer.ValueBytes);
							Logger.LogError("    {Key} (binary): {Text}", trailer.Key, text);
						}
						catch
						{
							Logger.LogError("    {Key}: [binary, {Length} bytes - non-UTF8]", 
								trailer.Key, trailer.ValueBytes.Length);
						}
					}
					else
					{
						Logger.LogError("    {Key}: {Value}", trailer.Key, trailer.Value);
					}
				}
			}
			else
			{
				Logger.LogWarning("  ⚠️ No trailers in response");
			}
			
			Logger.LogError("═══════════════════════════════════════════════════════════");
			
			throw;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "❌ Unexpected error: {Message}", ex.Message);
			throw;
		}
	}

	[Fact]
	public async Task ManagementService_Baseline_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		try
		{
			Logger.LogInformation("🔍 Baseline test: Verify ManagementService works");

			// This should work (we know it does from previous tests)
			var omniconfig = await OmniClient.Management.GetOmniConfigAsync(CancellationToken);

			Logger.LogInformation("✅ ManagementService works: {Length} chars", omniconfig.Length);
			Assert.NotEmpty(omniconfig);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "❌ Even ManagementService failed: {Message}", ex.Message);
			throw;
		}
	}
}
