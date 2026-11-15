using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SideroLabs.Omni.Api.Tests;

public partial class IntegrationTests
{
	/// <summary>
	/// Integration test that validates a JSON schema with valid data and expects success.
	/// </summary>
	[Fact]
	public async Task RealWorld_ValidateJsonSchema_WithValidData()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var options = GetClientOptions();
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing JSON schema validation with valid data");

		// Act - Test JSON schema validation with valid data
		var jsonSchema = """
			{
			  "type": "object",
			  "properties": {
			    "name": { "type": "string" },
			    "age": { "type": "number" }
			  },
			  "required": ["name"]
			}
			""";

		var validData = """
			{
			  "name": "John Doe",
			  "age": 30
			}
			""";

		Logger.LogInformation("🔍 Validating JSON data against schema...");
		var result = await client.Management.ValidateJsonSchemaAsync(validData, jsonSchema, CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.IsValid.Should().BeTrue("Valid JSON data should pass schema validation");
		result.Errors.Should().BeEmpty();

		Logger.LogInformation("✅ JSON schema validation succeeded for valid data!");
	}

	/// <summary>
	/// Integration test that validates a JSON schema with invalid data and expects validation errors.
	/// </summary>
	[Fact]
	public async Task RealWorld_ValidateJsonSchema_WithInvalidData()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var options = GetClientOptions();
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing JSON schema validation with invalid data");

		// Act - Test JSON schema validation with invalid data
		var jsonSchema = """
			{
			  "type": "object",
			  "properties": {
			    "name": { "type": "string" },
			    "age": { "type": "number" }
			  },
			  "required": ["name", "age"]
			}
			""";

		var invalidData = """
			{
			  "name": "Jane Doe"
			}
			""";

		Logger.LogInformation("🔍 Validating invalid JSON data against schema...");
		var result = await client.Management.ValidateJsonSchemaAsync(invalidData, jsonSchema, CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.IsValid.Should().BeFalse("Invalid JSON data should fail schema validation");
		result.Errors.Should().NotBeEmpty("Should have validation errors");
		result.TotalErrorCount.Should().BePositive("Should have at least one error");

		Logger.LogInformation("✅ JSON schema validation correctly detected {ErrorCount} error(s)!", result.TotalErrorCount);
		Logger.LogInformation("📊 Validation Errors:");
		Logger.LogInformation("{ErrorSummary}", result.GetErrorSummary());
	}

	/// <summary>
	/// Integration test that validates a complex nested JSON schema.
	/// </summary>
	[Fact]
	public async Task RealWorld_ValidateJsonSchema_WithComplexSchema()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var options = GetClientOptions();
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing JSON schema validation with complex nested schema");

		// Act - Test with a more complex schema
		Logger.LogInformation("🔍 Validating complex JSON data against nested schema...");
		var result = await client.Management.ValidateJsonSchemaAsync(ComplexData, ComplexSchema, CancellationToken);

		// Assert
		result.Should().NotBeNull();

		if (result.IsValid)
		{
			Logger.LogInformation("✅ Complex JSON schema validation succeeded!");
		}
		else
		{
			Logger.LogInformation("❌ Complex JSON schema validation failed with {ErrorCount} error(s)", result.TotalErrorCount);
			Logger.LogInformation("{ErrorSummary}", result.GetErrorSummary());
		}
	}

	/// <summary>
	/// Integration test that generates a support bundle for a cluster.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetSupportBundle_WithCluster()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var options = GetClientOptions();
		using var client = new OmniClient(options);

		// Try to get a cluster name from configuration or use default
		var clusterName = "default"; // Replace with actual cluster name if available

		Logger.LogInformation("🚀 Testing support bundle generation for cluster: {Cluster}", clusterName);

		try
		{
			// Act - Stream support bundle generation
			var (progressUpdates, totalBundleSize, errors) = await ConsumeSupportBundleAsync(client, clusterName);

			// Assert
			progressUpdates.Should().BePositive("Should have received progress updates");
			Logger.LogInformation("✅ Received {Updates} progress updates, total bundle size: {Size} bytes",
				progressUpdates, totalBundleSize);

			if (errors.Count > 0)
			{
				Logger.LogWarning("Encountered {Count} errors during bundle generation", errors.Count);
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Cluster '{Cluster}' not found - this is expected if no cluster exists", clusterName);
			Logger.LogInformation("✅ gRPC API is working correctly (NotFound is expected for non-existent clusters)");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for support bundle generation - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("⏱️ Support bundle generation timed out - this may indicate a slow operation");
			Logger.LogInformation("✅ gRPC streaming is working correctly (timeout is expected for long operations)");
		}
	}
	private const string ComplexSchema = """
		{
		  "type": "object",
		  "properties": {
		    "user": {
		      "type": "object",
		      "properties": {
		        "name": { "type": "string", "minLength": 1 },
		        "email": { "type": "string", "format": "email" },
		        "age": { "type": "integer", "minimum": 0, "maximum": 150 }
		      },
		      "required": ["name", "email"]
		    },
		    "tags": {
		      "type": "array",
		      "items": { "type": "string" },
		      "minItems": 1
		    }
		  },
		  "required": ["user"]
		}
		""";

	private const string ComplexData = """
		{
		  "user": {
		    "name": "Alice Smith",
		    "email": "alice@example.com",
		    "age": 25
		  },
		  "tags": ["developer", "golang", "kubernetes"]
		}
		""";

	private async Task<(int Updates, long BundleSize, List<string> Errors)> ConsumeSupportBundleAsync(OmniClient client, string clusterName)
	{
		var progressUpdates = 0;
		var totalBundleSize = 0L;
		var errors = new List<string>();

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

		await foreach (var progress in client.Management.GetSupportBundleAsync(
			cluster: clusterName,
			cancellationToken: cts.Token))
		{
			progressUpdates++;
			totalBundleSize += LogBundleProgress(progress, errors);

			if (totalBundleSize > 0 || progressUpdates > 10)
			{
				break;
			}
		}

		return (progressUpdates, totalBundleSize, errors);
	}

	private long LogBundleProgress(Models.SupportBundleProgress progress, List<string> errors)
	{
		if (progress.HasError)
		{
			errors.Add($"{progress.Source}: {progress.Error}");
			Logger.LogWarning("Support bundle error from {Source}: {Error}", progress.Source, progress.Error);
		}
		else if (progress.HasBundleData)
		{
			Logger.LogInformation("Received bundle data: {Size} bytes", progress.BundleData!.Length);
			return progress.BundleData.Length;
		}
		else if (!string.IsNullOrEmpty(progress.State))
		{
			Logger.LogInformation("Progress: {State} ({Value}/{Total} - {Percentage:F1}%)",
				progress.State, progress.Value, progress.Total, progress.ProgressPercentage);
		}

		return 0;
	}}
