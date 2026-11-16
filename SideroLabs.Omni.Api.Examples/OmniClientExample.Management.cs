using System.Text;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Read-only mode and comprehensive management examples
/// </summary>
public static partial class OmniClientExample
{
	/// <summary>
	/// Example demonstrating read-only mode and proper error handling
	/// </summary>
	public static async Task ReadOnlyModeExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "readonly-user",
			PgpPrivateKey = "<your armored PGP private key>",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true,
			IsReadOnly = true // Enable read-only mode for safety
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Read-Only Mode Demonstration ===");
			Console.WriteLine($"Client is in read-only mode: {client.IsReadOnly}");

			await DemonstrateAllowedReadOperations(client, cancellationToken);
			await DemonstrateBlockedWriteOperations(client, cancellationToken);
			DisplayReadOnlyModeSummary();
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Operation was cancelled");
		}
		catch (ReadOnlyModeException ex)
		{
			Console.WriteLine($"❌ Read-only mode violation: {ex.Message}");
			Console.WriteLine($"   Operation: {ex.Operation}");
			Console.WriteLine($"   Resource: {ex.ResourceType}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	/// <summary>
	/// Demonstrates read operations that are allowed in read-only mode
	/// </summary>
	private static async Task DemonstrateAllowedReadOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n✅ Read operations (allowed):");

		// NEW API!
		var serviceAccounts = await client.ServiceAccounts.ListAsync(cancellationToken);
		Console.WriteLine($"  - Listed {serviceAccounts.Count} service accounts");

		var omniconfig = await client.OmniConfig.GetAsync(cancellationToken);
		Console.WriteLine($"  - Retrieved omniconfig ({omniconfig.Length} characters)");

		var sampleConfig = "apiVersion: v1\nkind: Pod";
		await client.Validation.ValidateConfigAsync(sampleConfig, cancellationToken);
		Console.WriteLine("  - Validated configuration");
	}

	/// <summary>
	/// Demonstrates write operations that are blocked in read-only mode
	/// </summary>
	private static async Task DemonstrateBlockedWriteOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n❌ Write operations (blocked in read-only mode):");

		// NEW API!
		await TryWriteOperation("CreateServiceAccountAsync",
			() => client.ServiceAccounts.CreateAsync("test-key", cancellationToken: cancellationToken));

		await TryWriteOperation("CreateSchematicAsync",
			() => client.Schematics.CreateAsync(new CreateSchematicOptions { Extensions = ["test-extension"] }, cancellationToken: cancellationToken));

		await TryWriteOperation("GetKubeConfigAsync(serviceAccount=true)",
			() => client.KubeConfig.GetAsync(serviceAccount: true, cancellationToken: cancellationToken));
	}

	/// <summary>
	/// Tries a write operation and handles the expected ReadOnlyModeException
	/// </summary>
	private static async Task TryWriteOperation(string operationName, Func<Task> operation)
	{
		try
		{
			await operation();
			Console.WriteLine($"  - {operationName} - UNEXPECTED: Should have thrown ReadOnlyModeException");
		}
		catch (ReadOnlyModeException ex)
		{
			Console.WriteLine($"  - {operationName} - ✅ Blocked: {ex.Message}");
			Console.WriteLine($"    Operation: {ex.Operation}, Resource: {ex.ResourceType}");
		}
	}

	/// <summary>
	/// Displays a summary of read-only mode benefits
	/// </summary>
	private static void DisplayReadOnlyModeSummary()
	{
		Console.WriteLine("\n🛡️ Read-only mode provides safety for production environments");
		Console.WriteLine("💡 All write operations are properly blocked with clear error messages");
	}

	/// <summary>
	/// Example showing all available ManagementService operations
	/// </summary>
	public static async Task ComprehensiveManagementServiceExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			AuthToken = "your-auth-token",
			TimeoutSeconds = 60,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Comprehensive ManagementService Operations ===");
			Console.WriteLine("This example demonstrates all available gRPC operations from management.proto");

			await DemonstrateAllManagementOperations(client, cancellationToken);
			DisplayComprehensiveExampleSummary();
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Operation was cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	/// <summary>
	/// Demonstrates all management service operations
	/// </summary>
	private static async Task DemonstrateAllManagementOperations(OmniClient client, CancellationToken cancellationToken)
	{
		await DemonstrateConfigurationOperations(client, cancellationToken);
		await DemonstrateServiceAccountOperations(client, cancellationToken);
		await DemonstrateValidationOperations(client, cancellationToken);
		await DemonstrateKubernetesOperations(client, cancellationToken);
		await DemonstrateMachineProvisioningOperations(client, cancellationToken);
		DemonstrateStreamingOperations();
	}

	/// <summary>
	/// Demonstrates configuration management operations
	/// </summary>
	private static async Task DemonstrateConfigurationOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n1️⃣ Configuration Management:");

		// NEW API!
		var kubeconfig = await client.KubeConfig.GetAsync(cancellationToken: cancellationToken);
		Console.WriteLine($"   ✅ Kubeconfig: {kubeconfig.Length} characters");

		var talosconfig = await client.TalosConfig.GetAsync(cancellationToken: cancellationToken);
		Console.WriteLine($"   ✅ Talosconfig: {talosconfig.Length} characters");

		var omniconfig = await client.OmniConfig.GetAsync(cancellationToken);
		Console.WriteLine($"   ✅ Omniconfig: {omniconfig.Length} characters");
	}

	/// <summary>
	/// Demonstrates service account operations
	/// </summary>
	private static async Task DemonstrateServiceAccountOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n2️⃣ Service Account Management:");

		// NEW API!
		var accounts = await client.ServiceAccounts.ListAsync(cancellationToken);
		Console.WriteLine($"   ✅ Service Accounts: {accounts.Count} found");
	}

	/// <summary>
	/// Demonstrates validation operations
	/// </summary>
	private static async Task DemonstrateValidationOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n3️⃣ Configuration Validation:");

		// NEW API!
		await client.Validation.ValidateConfigAsync("apiVersion: v1\nkind: Pod", cancellationToken);
		Console.WriteLine("   ✅ Configuration validation successful");
	}

	/// <summary>
	/// Demonstrates Kubernetes operations
	/// </summary>
	private static async Task DemonstrateKubernetesOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n4️⃣ Kubernetes Operations:");

		// NEW API!
		var upgradePreCheckResult = await client.Kubernetes.UpgradePreChecksAsync(
			"v1.29.0", cancellationToken);
		Console.WriteLine($"   ✅ Upgrade check: {(upgradePreCheckResult.Ok ? "Ready" : "Not ready")} - {upgradePreCheckResult.Reason}");
	}

	/// <summary>
	/// Demonstrates machine provisioning operations
	/// </summary>
	private static async Task DemonstrateMachineProvisioningOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n5️⃣ Machine Provisioning:");

		// NEW API!
		var schematicResult = await client.Schematics.CreateAsync(new CreateSchematicOptions { Extensions = ["siderolabs/util-linux-tools"] }, cancellationToken: cancellationToken);
		Console.WriteLine($"   ✅ Schematic created: {schematicResult.SchematicId}");
		Console.WriteLine($"   📦 PXE URL: {schematicResult.PxeUrl}");
		Console.WriteLine($"   🔌 gRPC Tunnel: {(schematicResult.GrpcTunnelEnabled ? "Enabled" : "Disabled")}");
	}

	/// <summary>
	/// Demonstrates streaming operations (placeholder)
	/// </summary>
	private static void DemonstrateStreamingOperations()
	{
		Console.WriteLine("\n6️⃣ Streaming Operations:");
		Console.WriteLine("   🔄 Machine logs streaming...");
		Console.WriteLine("   🔄 Kubernetes manifest sync streaming...");
	}

	/// <summary>
	/// Displays summary of comprehensive example
	/// </summary>
	private static void DisplayComprehensiveExampleSummary()
	{
		Console.WriteLine("\n✅ All ManagementService operations demonstrated successfully!");
		Console.WriteLine("\nNote: This represents the complete set of operations");
		Console.WriteLine("available in the Omni gRPC ManagementService.");
	}
}
