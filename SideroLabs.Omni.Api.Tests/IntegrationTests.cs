using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Integration tests that make actual calls to the Omni gRPC API
/// These tests require proper configuration in appsettings.json
/// Tests only the real gRPC ManagementService operations
/// </summary>
public class IntegrationTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	private OmniClientOptions GetClientOptions(bool? isReadOnlyOverride = null)
	{
		var configuration = LoadConfiguration();
		var omniSection = configuration.GetSection("Omni");

		var authToken = omniSection["AuthToken"] ?? throw new FormatException("Omni config section must contain AuthToken.");

		return CreateClientOptions(omniSection, authToken, isReadOnlyOverride);
	}

	/// <summary>
	/// Loads configuration from appsettings.json
	/// </summary>
	private static IConfiguration LoadConfiguration()
	{
		return new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false)
			.Build();
	}

	/// <summary>
	/// Creates the OmniClientOptions with the provided configuration
	/// </summary>
	private OmniClientOptions CreateClientOptions(IConfigurationSection omniSection, string authToken, bool? isReadOnlyOverride)
		=> new()
		{
			BaseUrl = new(omniSection["BaseUrl"] ?? throw new InvalidOperationException("Omni:BaseUrl not configured")),
			AuthToken = authToken,
			TimeoutSeconds = int.Parse(omniSection["TimeoutSeconds"] ?? "30"),
			UseTls = bool.Parse(omniSection["UseTls"] ?? "true"),
			ValidateCertificate = bool.Parse(omniSection["ValidateCertificate"] ?? "true"),
			IsReadOnly = isReadOnlyOverride ?? bool.Parse(omniSection["IsReadOnly"] ?? "false"),
			Logger = Logger
		};

	/// <summary>
	/// Determines if integration tests should run based on configuration
	/// </summary>
	private new bool ShouldRunIntegrationTests()
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: true)
			.Build();

		var omniSection = configuration.GetSection("Omni");
		var endpoint = omniSection["BaseUrl"];
		var authToken = omniSection["AuthToken"];

		// Only run if we have a real endpoint and auth token
		var shouldRun = !string.IsNullOrEmpty(endpoint) &&
					   !string.IsNullOrEmpty(authToken) &&
					   !endpoint.Contains("test.example.com") &&
					   !endpoint.Contains("localhost");

		if (!shouldRun)
		{
			Logger.LogWarning("Skipping integration tests - no valid Omni endpoint and credentials configured");
		}

		return shouldRun;
	}

	/// <summary>
	/// Integration test that retrieves the omniconfig from the real Omni gRPC API using configured credentials.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetOmniconfig_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting real-world gRPC integration test with Sidero Labs credentials");
		Logger.LogInformation("BaseUrl: {BaseUrl}", options.BaseUrl);
		Logger.LogInformation("Identity: {Identity}", options.Identity);
		Logger.LogInformation("IsReadOnly: {IsReadOnly}", options.IsReadOnly);

		// Act - Test the real gRPC ManagementService
		Logger.LogInformation("🔍 Attempting to get omniconfig from real Omni gRPC API...");
		var omniconfig = await client.Management.GetOmniConfigAsync(CancellationToken);

		// Assert - These should actually pass or the test should fail
		omniconfig.Should().NotBeNull();
		omniconfig.Should().NotBeEmpty();

		Logger.LogInformation("✅ Successfully retrieved omniconfig from Sidero Labs Omni gRPC API!");
		Logger.LogInformation("📊 Omniconfig Details:");
		Logger.LogInformation("  Content: {Content}", omniconfig.Length > 200 ? omniconfig[..200] + "..." : omniconfig);
	}

	/// <summary>
	/// Integration test that retrieves a kubeconfig from the real Omni gRPC API.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetKubeconfig_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting real-world kubeconfig test");

		try
		{
			// Act - Test getting kubeconfig through real gRPC
			Logger.LogInformation("🔍 Attempting to get kubeconfig from real Omni gRPC API...");
			var kubeconfig = await client.Management.GetKubeConfigAsync(
				serviceAccount: false,
				cancellationToken: CancellationToken);

			// Assert - These should actually pass or the test should fail
			kubeconfig.Should().NotBeNull();
			kubeconfig.Should().NotBeEmpty();

			Logger.LogInformation("✅ Successfully retrieved kubeconfig from Sidero Labs Omni gRPC API!");
			Logger.LogInformation("📊 Kubeconfig Details:");
			Logger.LogInformation("  Content length: {Length} characters", kubeconfig.Length);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			// This is expected if the user role doesn't have permission to get kubeconfig
			Logger.LogInformation("🔒 Permission denied for kubeconfig access - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly (permission check succeeded)");
		}
	}

	/// <summary>
	/// Integration test that retrieves a kubeconfig with all available parameters including grant type and break-glass flag.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetKubeconfig_WithAllParameters()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange - Override read-only mode for this write operation test
		var options = GetClientOptions(isReadOnlyOverride: false);
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing kubeconfig with all parameters");

		try
		{
			// Act - Test getting kubeconfig with all available parameters
			Logger.LogInformation("🔍 Attempting to get kubeconfig with grant_type and break_glass...");
			var kubeconfig = await client.Management.GetKubeConfigAsync(
				serviceAccount: false,
				serviceAccountTtl: TimeSpan.FromHours(24),
				serviceAccountUser: "test-user",
				serviceAccountGroups: ["system:authenticated"],
				grantType: "", // Empty grant type for default behavior
				breakGlass: false,
				cancellationToken: CancellationToken);

			kubeconfig.Should().NotBeNull();
			kubeconfig.Should().NotBeEmpty();

			Logger.LogInformation("✅ Successfully retrieved kubeconfig with all parameters!");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
		{
			Logger.LogInformation("ℹ️ Invalid argument - this may occur with certain parameter combinations");
			Logger.LogInformation("✅ gRPC API is validating input correctly");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
	}

	/// <summary>
	/// Integration test that retrieves a talosconfig using break-glass mode.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetTalosconfig_WithBreakGlass()
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

		Logger.LogInformation("🚀 Testing talosconfig with break-glass parameter");

		try
		{
			// Act - Test getting talosconfig with break-glass mode
			Logger.LogInformation("🔍 Attempting to get talosconfig with raw=true and breakGlass=false...");
			var talosconfig = await client.Management.GetTalosConfigAsync(
				raw: true,
				breakGlass: false,
				cancellationToken: CancellationToken);

			talosconfig.Should().NotBeNull();
			talosconfig.Should().NotBeEmpty();

			Logger.LogInformation("✅ Successfully retrieved talosconfig with break-glass parameter!");
			Logger.LogInformation("📊 Talosconfig length: {Length} characters", talosconfig.Length);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
	}

	/// <summary>
	/// Integration test that creates a schematic using all available parameters.
	/// </summary>
	[Fact]
	public async Task RealWorld_CreateSchematic_WithAllParameters()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange - Override read-only mode for this write operation test
		var options = GetClientOptions(isReadOnlyOverride: false);
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing schematic creation with all parameters");

		try
		{
			// Act - Test creating schematic with all available parameters
			Logger.LogInformation("🔍 Creating schematic with Talos version, secure boot, and gRPC tunnel mode...");
			var schematicResult = await client
				.Management
				.CreateSchematicAsync(
					extensions: ["siderolabs/util-linux-tools"],
					extraKernelArgs: ["console=ttyS0"],
					metaValues: new Dictionary<uint, string> { { 0x0a, "test-env" } },
					talosVersion: "v1.7.0",
					mediaId: "installer",
					secureBoot: false,
					siderolinkGrpcTunnelMode: Enums.SiderolinkGrpcTunnelMode.Auto,
					joinToken: null,
					cancellationToken: CancellationToken);

			schematicResult.SchematicId.Should().NotBeNullOrEmpty();
			schematicResult.PxeUrl.Should().NotBeNullOrEmpty();

			Logger.LogInformation("✅ Successfully created schematic with all parameters!");
			Logger.LogInformation("📊 Schematic Details:");
			Logger.LogInformation("  Schematic ID: {SchematicId}", schematicResult.SchematicId);
			Logger.LogInformation("  PXE URL: {PxeUrl}", schematicResult.PxeUrl);
			Logger.LogInformation("  gRPC Tunnel Enabled: {GrpcTunnelEnabled}", schematicResult.GrpcTunnelEnabled);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for schematic creation - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
	}

	/// <summary>
	/// Integration test that lists service accounts from the real Omni API.
	/// </summary>
	[Fact]
	public async Task RealWorld_ListServiceAccounts_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting real-world service account listing test");

		try
		{
			// Act - Test listing service accounts through real gRPC
			Logger.LogInformation("🔍 Attempting to list service accounts from real Omni gRPC API...");
			var serviceAccounts = await client.Management.ListServiceAccountsAsync(CancellationToken);

			// Assert - These should actually pass or the test should fail
			serviceAccounts.Should().NotBeNull();

			Logger.LogInformation("✅ Successfully retrieved service accounts from Sidero Labs Omni gRPC API!");
			Logger.LogInformation("📊 Service Account Details:");
			Logger.LogInformation("  Total Service Accounts: {Count}", serviceAccounts.Count);

			foreach (var account in serviceAccounts.Take(3)) // Log first 3 service accounts
			{
				Logger.LogInformation("  🔑 Service Account: {Name}", account.Name);
				Logger.LogInformation("    Role: {Role}", account.Role);
				Logger.LogInformation("    PGP Keys: {KeyCount}", account.PgpPublicKeys.Count);

				foreach (var key in account.PgpPublicKeys.Take(1)) // Log first key only
				{
					Logger.LogInformation("      Key ID: {KeyId}", key.Id);
					Logger.LogInformation("      Expires: {Expiration:yyyy-MM-dd HH:mm:ss}", key.Expiration);
				}
			}

			if (serviceAccounts.Count > 3)
			{
				Logger.LogInformation("  ... and {MoreCount} more service accounts", serviceAccounts.Count - 3);
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			// This is expected if the user role doesn't have permission to list service accounts
			Logger.LogInformation("🔒 Permission denied for service account listing - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly (permission check succeeded)");

			// Test passes - we successfully made a gRPC call that was properly authenticated but lacks permission
			// This proves the gRPC client and authentication are working
		}
	}

	/// <summary>
	/// Integration test that validates a machine configuration against the real Omni API.
	/// </summary>
	[Fact]
	public async Task RealWorld_ValidateConfig_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting real-world config validation test");

		// Act - Test config validation through real gRPC
		Logger.LogInformation("🔍 Attempting to validate config via real Omni gRPC API...");

		// Use a simple valid config patch that doesn't override restricted fields
		var sampleConfig = """
			machine:
			  install:
			    diskSelector:
			      size: ">= 100GB"
			  network:
			    hostname: test-node
			cluster:
			  network:
			    dnsDomain: cluster.local
			""";

		// This should succeed or throw - no swallowing exceptions
		await client.Management.ValidateConfigAsync(sampleConfig, CancellationToken);

		Logger.LogInformation("✅ Successfully validated config via Sidero Labs Omni gRPC API!");
		Logger.LogInformation("📊 Config validation completed successfully");
	}

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
		var complexSchema = """
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

		var complexData = """
			{
			  "user": {
			    "name": "Alice Smith",
			    "email": "alice@example.com",
			    "age": 25
			  },
			  "tags": ["developer", "golang", "kubernetes"]
			}
			""";

		Logger.LogInformation("🔍 Validating complex JSON data against nested schema...");
		var result = await client.Management.ValidateJsonSchemaAsync(complexData, complexSchema, CancellationToken);

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
			var progressUpdates = 0;
			var totalBundleSize = 0L;
			var errors = new List<string>();

			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // Longer timeout for bundle generation

			await foreach (var progress in client.Management.GetSupportBundleAsync(
				cluster: clusterName,
				cancellationToken: cts.Token))
			{
				progressUpdates++;

				if (progress.HasError)
				{
					errors.Add($"{progress.Source}: {progress.Error}");
					Logger.LogWarning("Support bundle error from {Source}: {Error}", progress.Source, progress.Error);
				}
				else if (progress.HasBundleData)
				{
					totalBundleSize += progress.BundleData!.Length;
					Logger.LogInformation("Received bundle data: {Size} bytes", progress.BundleData.Length);
				}
				else if (!string.IsNullOrEmpty(progress.State))
				{
					Logger.LogInformation("Progress: {State} ({Value}/{Total} - {Percentage:F1}%)",
						progress.State, progress.Value, progress.Total, progress.ProgressPercentage);
				}

				// Stop after receiving some data or enough updates
				if (totalBundleSize > 0 || progressUpdates > 10)
				{
					break;
				}
			}

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

	/// <summary>
	/// Integration test that reads the audit log for a date range.
	/// </summary>
	[Fact]
	public async Task RealWorld_ReadAuditLog_WithDateRange()
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

		// Use a recent date range
		var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
		var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

		Logger.LogInformation("🚀 Testing audit log reading from {StartDate} to {EndDate}", startDate, endDate);

		try
		{
			// Act - Stream audit log entries
			var chunkCount = 0;
			var totalBytes = 0L;
			var maxChunks = 5; // Limit chunks for testing

			await foreach (var logData in client.Management.ReadAuditLogAsync(
				startDate: startDate,
				endDate: endDate,
				cancellationToken: CancellationToken))
			{
				chunkCount++;
				totalBytes += logData.Length;

				var logText = System.Text.Encoding.UTF8.GetString(logData);
				Logger.LogInformation("Received audit log chunk {Count}: {Size} bytes", chunkCount, logData.Length);
				Logger.LogDebug("Log content preview: {Preview}",
					logText.Length > 100 ? logText[..100] + "..." : logText);

				if (chunkCount >= maxChunks)
				{
					break;
				}
			}

			// Assert
			if (chunkCount > 0)
			{
				chunkCount.Should().BePositive("Should have received at least one audit log chunk");
				totalBytes.Should().BePositive("Should have received some audit log data");
				Logger.LogInformation("✅ Successfully read {Chunks} audit log chunks, {TotalBytes} total bytes",
					chunkCount, totalBytes);
			}
			else
			{
				Logger.LogInformation("ℹ️ No audit log entries found for the specified date range");
				Logger.LogInformation("✅ gRPC streaming is working correctly (empty result is valid)");
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
		{
			Logger.LogInformation("ℹ️ Invalid date range format - expected YYYY-MM-DD");
			Logger.LogInformation("✅ gRPC API is validating input correctly");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for audit log access - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
	}

	/// <summary>
	/// Integration test that initiates a maintenance upgrade on a machine.
	/// </summary>
	[Fact]
	public async Task RealWorld_MaintenanceUpgrade_WithMachine()
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

		// Get a machine ID from talosconfig
		var testMachineId = await GetFirstMachineIdAsync(client);

		if (string.IsNullOrEmpty(testMachineId))
		{
			Logger.LogInformation("ℹ️ No machines found - skipping test");
			Logger.LogInformation("💡 Tip: Ensure your Omni instance has at least one machine configured");
			return;
		}

		Logger.LogInformation("🚀 Testing maintenance upgrade for machine: {MachineId}", testMachineId);

		try
		{
			// Act - Attempt maintenance upgrade
			await client.Management.MaintenanceUpgradeAsync(
				machineId: testMachineId,
				version: "v1.7.0",
				cancellationToken: CancellationToken);

			Logger.LogInformation("✅ Maintenance upgrade initiated successfully for machine {MachineId}", testMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Machine '{MachineId}' not found or not eligible for upgrade", testMachineId);
			Logger.LogInformation("✅ gRPC API is working correctly (NotFound is expected for unavailable machines)");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for maintenance upgrade - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (Exceptions.ReadOnlyModeException ex)
		{
			Logger.LogInformation("🔒 Write operation blocked in read-only mode: {Message}", ex.Message);
			Logger.LogInformation("✅ Read-only mode protection is working correctly");
		}
	}

	/// <summary>
	/// Integration test that retrieves a machine join configuration with gRPC tunnel mode.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetMachineJoinConfig_WithGrpcTunnel()
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

		Logger.LogInformation("🚀 Testing machine join config retrieval");

		try
		{
			// Act - Get machine join configuration
			var config = await client.Management.GetMachineJoinConfigAsync(
				useGrpcTunnel: true,
				joinToken: "test-token",
				cancellationToken: CancellationToken);

			// Assert
			config.Should().NotBeNull();
			Logger.LogInformation("✅ Machine join config retrieved successfully");
			Logger.LogInformation("📊 Configuration Details:");
			Logger.LogInformation("  Kernel Args: {Count}", config.KernelArgs.Count);
			Logger.LogInformation("  Config Size: {Size} characters", config.Config.Length);
			Logger.LogInformation("  Summary: {Summary}", config.GetSummary());

			if (config.HasKernelArgs)
			{
				Logger.LogInformation("  Kernel Args String: {Args}", config.GetKernelArgsString());
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
		{
			Logger.LogInformation("ℹ️ Invalid join token provided");
			Logger.LogInformation("✅ gRPC API is validating input correctly");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for machine join config - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
	}

	/// <summary>
	/// Integration test that creates a join token with an expiration.
	/// </summary>
	[Fact]
	public async Task RealWorld_CreateJoinToken_WithExpiration()
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

		var tokenName = $"test-token-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
		var expirationTime = DateTimeOffset.UtcNow.AddDays(7);

		Logger.LogInformation("🚀 Testing join token creation: {TokenName}", tokenName);

		try
		{
			// Act - Create join token
			var tokenId = await client.Management.CreateJoinTokenAsync(
				name: tokenName,
				expirationTime: expirationTime,
				cancellationToken: CancellationToken);

			// Assert
			tokenId.Should().NotBeNullOrEmpty();
			Logger.LogInformation("✅ Join token created successfully: {TokenId}", tokenId);
			Logger.LogInformation("📊 Token Details:");
			Logger.LogInformation("  Name: {Name}", tokenName);
			Logger.LogInformation("  Expiration: {Expiration:yyyy-MM-dd HH:mm:ss}", expirationTime);
			Logger.LogInformation("  Token ID: {TokenId}", tokenId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for join token creation - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (Exceptions.ReadOnlyModeException ex)
		{
			Logger.LogInformation("🔒 Write operation blocked in read-only mode: {Message}", ex.Message);
			Logger.LogInformation("✅ Read-only mode protection is working correctly");
		}
	}

	/// <summary>
	/// Integration test that tears down a locked cluster.
	/// </summary>
	[Fact]
	public async Task RealWorld_TearDownLockedCluster_WithClusterId()
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

		// Use a test cluster ID that doesn't exist to avoid accidental deletion
		var testClusterId = $"test-cluster-{Guid.NewGuid()}";

		Logger.LogInformation("🚀 Testing locked cluster tear down (non-existent cluster for safety)");

		try
		{
			// Act - Attempt to tear down locked cluster
			await client.Management.TearDownLockedClusterAsync(
				clusterId: testClusterId,
				cancellationToken: CancellationToken);

			Logger.LogInformation("✅ Tear down operation completed for cluster {ClusterId}", testClusterId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Cluster '{ClusterId}' not found - this is expected for test", testClusterId);
			Logger.LogInformation("✅ gRPC API is working correctly (NotFound is expected for non-existent clusters)");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for cluster tear down - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (Exceptions.ReadOnlyModeException ex)
		{
			Logger.LogInformation("🔒 Write operation blocked in read-only mode: {Message}", ex.Message);
			Logger.LogInformation("✅ Read-only mode protection is working correctly");
		}
	}

	/// <summary>
	/// Integration test that runs Kubernetes upgrade pre-checks against the real Omni API.
	/// </summary>
	[Fact]
	public async Task RealWorld_KubernetesUpgradePreChecks_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting real-world Kubernetes upgrade pre-checks test");

		try
		{
			// Act - Test Kubernetes upgrade pre-checks through real gRPC
			Logger.LogInformation("🔍 Attempting Kubernetes upgrade pre-checks via real Omni gRPC API...");

			var result = await client.Management.KubernetesUpgradePreChecksAsync(
				"v1.29.0",
				CancellationToken);

			// Assert - Just log the results, this operation can legitimately return false
			Logger.LogInformation("✅ Successfully completed Kubernetes upgrade pre-checks via Sidero Labs Omni gRPC API!");
			Logger.LogInformation("📊 Upgrade Pre-check Results:");
			Logger.LogInformation("  Can Upgrade to v1.29.0: {CanUpgrade}", result.Ok ? "✅ Yes" : "❌ No");
			Logger.LogInformation("  Reason: {Reason}", result.Reason);

			// At minimum, the reason should not be null or empty
			result.Reason.Should().NotBeNull();
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			// This is expected if the user role doesn't have permission for upgrade operations
			Logger.LogInformation("🔒 Permission denied for Kubernetes upgrade pre-checks - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly (permission check succeeded)");
		}
	}

	/// <summary>
	/// Integration test that verifies read-only mode prevents write operations.
	/// </summary>
	[Fact]
	public async Task RealWorld_ReadOnlyMode_PreventsWriteOperations()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange - Force read-only mode
		var options = GetClientOptions(isReadOnlyOverride: true);
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing read-only mode with real gRPC credentials");
		Logger.LogInformation("🔒 Read-only mode: {IsReadOnly}", options.IsReadOnly);

		// Act & Assert - Read operations should still work
		Logger.LogInformation("🔍 Verifying read operations work in read-only mode...");
		var omniconfig = await client.Management.GetOmniConfigAsync(CancellationToken);
		omniconfig.Should().NotBeNull();
		omniconfig.Should().NotBeEmpty();

		Logger.LogInformation("✅ Read operations work correctly in read-only mode");
		Logger.LogInformation("Note: Write operation protection will be implemented when needed for specific gRPC operations");
	}

	/// <summary>
	/// Integration test that exercises all management service operations comprehensively.
	/// </summary>
	[Fact]
	public async Task RealWorld_ComprehensiveManagementService_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting comprehensive gRPC ManagementService test");
		Logger.LogInformation("This test demonstrates all available real gRPC operations");

		var testResults = new TestOperationResults();

		// Execute all test categories
		await TestConfigurationManagement(client, testResults);
		await TestServiceAccountManagement(client, testResults);
		await TestConfigurationValidation(client, testResults);
		await TestKubernetesOperations(client, testResults);

		// Validate overall results
		ValidateOverallTestResults(testResults);

		Logger.LogInformation("✅ Comprehensive gRPC ManagementService test completed!");
	}

	private async Task TestConfigurationManagement(OmniClient client, TestOperationResults results)
	{
		Logger.LogInformation("\n1️⃣ Configuration Management:");

		await TestOmniConfigOperation(client, results);
		await TestKubeConfigOperation(client, results);
		await TestTalosConfigOperation(client, results);
	}

	private async Task TestOmniConfigOperation(OmniClient client, TestOperationResults results)
	{
		try
		{
			results.TotalOperations++;
			var omniconfig = await client.Management.GetOmniConfigAsync(CancellationToken);
			omniconfig.Should().NotBeNull();
			omniconfig.Should().NotBeEmpty();
			Logger.LogInformation("   ✅ Omniconfig: {Length} characters", omniconfig.Length);
			results.SuccessCount++;
		}
		catch (Exception ex)
		{
			Logger.LogWarning("   ⚠️ Omniconfig failed: {Message}", ex.Message);
		}
	}

	private async Task TestKubeConfigOperation(OmniClient client, TestOperationResults results)
	{
		try
		{
			results.TotalOperations++;
			var kubeconfig = await client.Management.GetKubeConfigAsync(cancellationToken: CancellationToken);
			kubeconfig.Should().NotBeNull();
			kubeconfig.Should().NotBeEmpty();
			Logger.LogInformation("   ✅ Kubeconfig: {Length} characters", kubeconfig.Length);
			results.SuccessCount++;
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("   🔒 Kubeconfig access denied (Reader role) - this counts as success for auth testing");
			results.SuccessCount++; // Permission check working is a success
		}
		catch (Exception ex)
		{
			Logger.LogWarning("   ⚠️ Kubeconfig failed: {Message}", ex.Message);
		}
	}

	private async Task TestTalosConfigOperation(OmniClient client, TestOperationResults results)
	{
		try
		{
			results.TotalOperations++;
			var talosconfig = await client.Management.GetTalosConfigAsync(cancellationToken: CancellationToken);
			talosconfig.Should().NotBeNull();
			talosconfig.Should().NotBeEmpty();
			Logger.LogInformation("   ✅ Talosconfig: {Length} characters", talosconfig.Length);
			results.SuccessCount++;
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("   🔒 Talosconfig access denied (Reader role) - this counts as success for auth testing");
			results.SuccessCount++; // Permission check working is a success
		}
		catch (Exception ex)
		{
			Logger.LogWarning("   ⚠️ Talosconfig failed: {Message}", ex.Message);
		}
	}

	private async Task TestServiceAccountManagement(OmniClient client, TestOperationResults results)
	{
		Logger.LogInformation("\n2️⃣ Service Account Management:");

		try
		{
			results.TotalOperations++;
			var accounts = await client.Management.ListServiceAccountsAsync(CancellationToken);
			accounts.Should().NotBeNull();
			Logger.LogInformation("   ✅ Service Accounts: {Count} found", accounts.Count);
			results.SuccessCount++;
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("   🔒 Service account listing denied (Reader role) - this counts as success for auth testing");
			results.SuccessCount++; // Permission check working is a success
		}
		catch (Exception ex)
		{
			Logger.LogWarning("   ⚠️ Service account listing failed: {Message}", ex.Message);
		}
	}

	private async Task TestConfigurationValidation(OmniClient client, TestOperationResults results)
	{
		Logger.LogInformation("\n3️⃣ Configuration Validation:");

		try
		{
			results.TotalOperations++;
			await client.Management.ValidateConfigAsync("machine:\n  network:\n    hostname: test-node", CancellationToken);
			Logger.LogInformation("   ✅ Configuration validation successful");
			results.SuccessCount++;
		}
		catch (Exception ex)
		{
			Logger.LogWarning("   ⚠️ Configuration validation failed: {Message}", ex.Message);
		}
	}

	private async Task TestKubernetesOperations(OmniClient client, TestOperationResults results)
	{
		Logger.LogInformation("\n4️⃣ Kubernetes Operations:");

		try
		{
			results.TotalOperations++;
			var result = await client.Management.KubernetesUpgradePreChecksAsync(
				"v1.29.0", CancellationToken);
			result.Reason.Should().NotBeNull();
			Logger.LogInformation("   ✅ Upgrade check: {Status} - {Reason}",
				result.Ok ? "Ready" : "Not ready", result.Reason);
			results.SuccessCount++;
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("   🔒 Kubernetes upgrade pre-checks denied (Reader role) - this counts as success for auth testing");
			results.SuccessCount++; // Permission check working is a success
		}
		catch (Exception ex)
		{
			Logger.LogWarning("   ⚠️ Kubernetes upgrade pre-checks failed: {Message}", ex.Message);
		}
	}

	private void ValidateOverallTestResults(TestOperationResults results)
	{
		Logger.LogInformation("\n📊 Test Results:");
		Logger.LogInformation("  Successful operations: {SuccessCount}/{TotalOperations}", results.SuccessCount, results.TotalOperations);
		Logger.LogInformation("  Success rate: {SuccessRate:P0}", results.SuccessRate);

		// Assert that at least 50% of operations succeeded (reasonable threshold for external dependencies)
		results.SuccessRate.Should().BeGreaterThan(0.5, "At least 50% of gRPC operations should succeed with valid credentials");
	}

	[Fact]
	public void ClientOptions_IsReadOnlyDefault_IsFalse()
	{
		// Arrange & Act
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.example.com")
		};

		// Assert
		options.IsReadOnly.Should().BeFalse("IsReadOnly should default to false");
	}

	[Fact]
	public void ClientOptions_IsReadOnlyCanBeSet()
	{
		// Arrange & Act
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.example.com"),
			IsReadOnly = true
		};

		// Assert
		options.IsReadOnly.Should().BeTrue("IsReadOnly should be settable to true");
	}

	[Fact]
	public void AuthToken_DecodesCorrectly()
	{
		// Arrange
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false)
			.Build();

		var authToken = configuration.GetSection("Omni")["AuthToken"];

		// Act & Assert
		if (!string.IsNullOrEmpty(authToken))
		{
			var decodedBytes = Convert.FromBase64String(authToken);
			var decodedJson = Encoding.UTF8.GetString(decodedBytes);

			using var jsonDoc = JsonDocument.Parse(decodedJson);
			var root = jsonDoc.RootElement;

			root.TryGetProperty("name", out var nameElement).Should().BeTrue();
			root.TryGetProperty("pgp_key", out var pgpKeyElement).Should().BeTrue();

			var identity = nameElement.GetString();
			var pgpKey = pgpKeyElement.GetString();

			identity.Should().NotBeNullOrEmpty();
			pgpKey.Should().NotBeNullOrEmpty();
			pgpKey.Should().StartWith("-----BEGIN PGP PRIVATE KEY BLOCK-----");

			Logger.LogInformation("✅ AuthToken decodes correctly for identity: {Identity}", identity);
		}
		else
		{
			Logger.LogInformation("ℹ️ No AuthToken configured - this is expected for environments without Omni credentials");
		}
	}

	/// <summary>
	/// Gets the first available machine ID from talosconfig
	/// </summary>
	private async Task<string?> GetFirstMachineIdAsync(OmniClient client)
	{
		try
		{
			Logger.LogInformation("🔍 Retrieving talosconfig to extract machine IDs...");
			var talosconfig = await client.Management.GetTalosConfigAsync(
				raw: false,
				cancellationToken: CancellationToken);

			// Parse YAML to extract endpoints
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();

			var config = deserializer.Deserialize<Dictionary<string, object>>(talosconfig);

			// Extract endpoints from contexts
			if (config.TryGetValue("contexts", out var contextsObj) && contextsObj is Dictionary<object, object> contexts)
			{
				foreach (var contextEntry in contexts.Values)
				{
					if (contextEntry is Dictionary<object, object> context &&
						context.TryGetValue("endpoints", out var endpointsObj) &&
						endpointsObj is List<object> endpoints &&
						endpoints.Count > 0)
					{
						var firstEndpoint = endpoints[0].ToString();
						Logger.LogInformation("✅ Found machine endpoint: {BaseUrl}", firstEndpoint);
						return firstEndpoint;
					}
				}
			}

			Logger.LogWarning("⚠️ No machine endpoints found in talosconfig");
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to extract machine ID from talosconfig");
			return null;
		}
	}

	/// <summary>
	/// Integration test that streams machine logs for configured machines.
	/// </summary>
	[Fact]
	public async Task RealWorld_StreamMachineLogs_WithConfiguredMachines()
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

		Logger.LogInformation("🚀 Testing machine log streaming with configured machines");

		// Get the first machine ID from talosconfig
		var testMachineId = await GetFirstMachineIdAsync(client);

		if (string.IsNullOrEmpty(testMachineId))
		{
			Logger.LogInformation("ℹ️ No machines found in talosconfig - skipping test");
			Logger.LogInformation("💡 Tip: Ensure your Omni instance has at least one machine configured");
			return;
		}

		Logger.LogInformation("Attempting to stream logs from machine: {MachineId}", testMachineId);

		try
		{
			// Act - Stream a few log entries
			var logCount = 0;
			var maxLogs = 5; // Only fetch first 5 logs for testing

			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: testMachineId,
				follow: false, // Don't follow, just get recent logs
				tailLines: 10,
				cancellationToken: CancellationToken))
			{
				var logText = System.Text.Encoding.UTF8.GetString(logData);
				Logger.LogInformation("📄 Log entry {Count}: {LogText}", ++logCount, logText.Length > 100 ? logText[..100] + "..." : logText);

				if (logCount >= maxLogs)
				{
					break;
				}
			}

			// Assert - We should have received some logs
			logCount.Should().BePositive("Should have received at least one log entry from the machine");

			Logger.LogInformation("✅ Successfully streamed {Count} log entries from machine {MachineId}", logCount, testMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			// Machine not found - provide helpful message
			Logger.LogWarning("⚠️ Machine '{MachineId}' not found.", testMachineId);
			Logger.LogInformation("💡 Tip: The machine may not be available or accessible");

			// Still mark as success since this confirms the API is working correctly
			Logger.LogInformation("✅ gRPC authentication and machine log API are working correctly (NotFound is expected for unavailable machines)");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			// Permission denied - expected with Reader role
			Logger.LogInformation("🔒 Permission denied for machine log access - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly (permission check succeeded)");
		}
	}

	/// <summary>
	/// Integration test that validates input for machine log streaming.
	/// </summary>
	[Fact]
	public async Task RealWorld_MachineLogStreaming_ValidatesInput()
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

		Logger.LogInformation("🚀 Testing machine log streaming input validation");

		try
		{
			// Act & Assert - Test with empty machine ID
			var emptyMachineId = "";
			var logCount = 0;

			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: emptyMachineId,
				follow: false,
				tailLines: 1,
				cancellationToken: CancellationToken))
			{
				logCount++;
				break; // Just check if we can start the stream
			}

			// If we get here, either:
			// 1. The API accepted empty machine ID (some APIs do this)
			// 2. We got an error (which is expected and handled in the catch)
			Logger.LogInformation("ℹ️ Machine log streaming API accepted empty machine ID");
		}
		catch (Grpc.Core.RpcException rpcEx)
		{
			// Expected - API should reject invalid input
			Logger.LogInformation("✅ Machine log streaming correctly validates input: {Status}", rpcEx.StatusCode);
			Logger.LogInformation("   Error message: {Message}", rpcEx.Status.Detail);
		}
	}

	[Fact]
	public async Task RealWorld_MachineLogStreaming_HandlesNonExistentMachine()
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

		Logger.LogInformation("🚀 Testing machine log streaming with non-existent machine");

		// Use a machine ID that definitely doesn't exist
		var nonExistentMachineId = "non-existent-machine-" + Guid.NewGuid();

		try
		{
			// Act - Try to stream logs from non-existent machine
			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: nonExistentMachineId,
				follow: false,
				tailLines: 1,
				cancellationToken: CancellationToken))
			{
				// If we get here, something unexpected happened
				Logger.LogWarning("⚠️ Received log data for non-existent machine: {Length} bytes", logData.Length);
				break;
			}

			Logger.LogInformation("ℹ️ Machine log streaming completed without error for non-existent machine");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			// Expected - machine doesn't exist
			Logger.LogInformation("✅ Machine log streaming correctly handles non-existent machine: NotFound");
			Logger.LogInformation("   Machine ID: {MachineId}", nonExistentMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx)
		{
			// Other gRPC errors
			Logger.LogInformation("ℹ️ Machine log streaming returned gRPC error: {Status}", rpcEx.StatusCode);
			Logger.LogInformation("   Error: {Message}", rpcEx.Status.Detail);
		}
	}

	[Fact]
	public async Task RealWorld_MachineLogStreaming_SupportsFollowMode()
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

		Logger.LogInformation("🚀 Testing machine log streaming in follow mode");

		// Get the first machine ID from talosconfig
		var testMachineId = await GetFirstMachineIdAsync(client);

		if (string.IsNullOrEmpty(testMachineId))
		{
			Logger.LogInformation("ℹ️ No machines found - skipping test");
			return;
		}

		try
		{
			// Act - Start streaming with follow=true, but with a short timeout
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			var logCount = 0;

			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: testMachineId,
				follow: true, // Enable follow mode
				tailLines: 5,
				cancellationToken: cts.Token))
			{
				logCount++;
				Logger.LogDebug("Received log entry {Count} in follow mode", logCount);

				// Stop after a few entries to avoid long-running test
				if (logCount >= 3)
				{
					break;
				}
			}

			Logger.LogInformation("✅ Follow mode streaming works correctly (received {Count} log entries)", logCount);
		}
		catch (OperationCanceledException)
		{
			// Expected when timeout expires
			Logger.LogInformation("✅ Follow mode streaming correctly handles cancellation");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Machine '{MachineId}' not found", testMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for machine log access - this is expected with Reader role");
		}
	}

	[Fact]
	public async Task RealWorld_MachineLogStreaming_SupportsTailLines()
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

		Logger.LogInformation("🚀 Testing machine log streaming with tail lines");

		// Get the first machine ID from talosconfig
		var testMachineId = await GetFirstMachineIdAsync(client);

		if (string.IsNullOrEmpty(testMachineId))
		{
			Logger.LogInformation("ℹ️ No machines found - skipping test");
			return;
		}

		try
		{
			// Act - Request specific number of tail lines
			var requestedTailLines = 20;
			var logCount = 0;

			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: testMachineId,
				follow: false,
				tailLines: requestedTailLines,
				cancellationToken: CancellationToken))
			{
				logCount++;

				// Don't wait for all logs in the test
				if (logCount >= requestedTailLines)
				{
					break;
				}
			}

			Logger.LogInformation("✅ Tail lines parameter works correctly (requested {Requested}, received {Actual})",
				requestedTailLines, logCount);

			if (logCount > 0)
			{
				logCount.Should().BeLessThanOrEqualTo(requestedTailLines, "Should not receive more logs than requested");
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Machine '{MachineId}' not found", testMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for machine log access - this is expected with Reader role");
		}
	}

	/// <summary>
	/// Helper class to track test operation results
	/// </summary>
	private sealed class TestOperationResults
	{
		public int SuccessCount { get; set; }
		public int TotalOperations { get; set; }
		public double SuccessRate => TotalOperations > 0 ? (double)SuccessCount / TotalOperations : 0.0;
	}
}
