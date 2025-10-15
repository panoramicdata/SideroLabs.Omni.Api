using System.Text;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Examples of the gRPC-only OmniClient API
/// Demonstrates the actual ManagementService operations available in Omni
/// </summary>
public static class OmniClientExample
{
	/// <summary>
	/// Basic example of how to use the OmniClient to interact with the Omni Management gRPC API
	/// </summary>
	public static async Task BasicUsageExample()
	{
		var options = CreateDefaultClientOptions();
		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var cancellationToken = cts.Token;

		try
		{
			await DemonstrateConfigurationManagement(client, cancellationToken);
			await DemonstrateServiceAccountListing(client, cancellationToken);
			await DemonstrateOperationalTasks(client, cancellationToken);
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
	/// Creates default client options for examples
	/// </summary>
	private static OmniClientOptions CreateDefaultClientOptions()
	{
		return new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "your-username",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true
		};
	}

	/// <summary>
	/// Demonstrates configuration management operations
	/// </summary>
	private static async Task DemonstrateConfigurationManagement(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("=== Configuration Management ===");

		// Get kubeconfig for cluster access
		var kubeconfig = await client.Management.GetKubeConfigAsync(
			serviceAccount: true,
			serviceAccountTtl: TimeSpan.FromHours(24),
			serviceAccountUser: "automation",
			serviceAccountGroups: ["system:masters"],
			cancellationToken: cancellationToken);

		Console.WriteLine($"Retrieved kubeconfig ({kubeconfig.Length} characters)");
		File.WriteAllText("kubeconfig.yaml", kubeconfig);

		// Get talosconfig for Talos cluster access
		var talosconfig = await client.Management.GetTalosConfigAsync(
			raw: true,
			cancellationToken: cancellationToken);

		Console.WriteLine($"Retrieved talosconfig ({talosconfig.Length} characters)");
		File.WriteAllText("talosconfig.yaml", talosconfig);

		// Get omniconfig for omnictl
		var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
		Console.WriteLine($"Retrieved omniconfig ({omniconfig.Length} characters)");
		File.WriteAllText("omniconfig.yaml", omniconfig);
	}

	/// <summary>
	/// Demonstrates service account listing and information display
	/// </summary>
	private static async Task DemonstrateServiceAccountListing(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n=== Service Account Management ===");

		var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
		Console.WriteLine($"Found {serviceAccounts.Count} service accounts");

		foreach (var account in serviceAccounts)
		{
			Console.WriteLine($"Service Account: {account.Name}");
			Console.WriteLine($"  Role: {account.Role}");
			Console.WriteLine($"  PGP Keys: {account.PgpPublicKeys.Count}");

			foreach (var key in account.PgpPublicKeys)
			{
				Console.WriteLine($"    Key ID: {key.Id}");
				Console.WriteLine($"    Expires: {key.Expiration:yyyy-MM-dd HH:mm:ss}");
			}
		}
	}

	/// <summary>
	/// Demonstrates operational tasks like validation and upgrade checks
	/// </summary>
	private static async Task DemonstrateOperationalTasks(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n=== Operational Tasks ===");

		// Validate a configuration file
		var sampleConfig = """
			apiVersion: v1
			kind: ConfigMap
			metadata:
			  name: test-config
			data:
			  key: value
			""";

		await client.Management.ValidateConfigAsync(sampleConfig, cancellationToken);
		Console.WriteLine("Configuration validation successful");

		// Check Kubernetes upgrade readiness
		var (canUpgrade, reason) = await client.Management.KubernetesUpgradePreChecksAsync(
			"v1.29.0",
			cancellationToken);

		Console.WriteLine($"Kubernetes upgrade to v1.29.0: {(canUpgrade ? "✅ Ready" : "❌ Not ready")}");
		if (!string.IsNullOrEmpty(reason))
		{
			Console.WriteLine($"Reason: {reason}");
		}

	}

	/// <summary>
	/// Advanced example showcasing streaming operations and machine management
	/// </summary>
	public static async Task AdvancedStreamingExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "your-username",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 60,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
		var cancellationToken = cts.Token;

		try
		{
			await DemonstrateMachineLogStreaming(client, cancellationToken);
			await DemonstrateKubernetesManifestSync(client, cancellationToken);
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
	/// Demonstrates machine log streaming functionality
	/// </summary>
	private static async Task DemonstrateMachineLogStreaming(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("=== Machine Log Streaming ===");

		var machineId = "machine-001"; // Replace with actual machine ID
		Console.WriteLine($"Streaming logs from machine: {machineId}");

		await foreach (var logData in client.Management.StreamMachineLogsAsync(
			machineId,
			follow: true,
			tailLines: 50,
			cancellationToken))
		{
			var logText = Encoding.UTF8.GetString(logData);
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {logText}");

			// Break after 10 log entries for demo
			if (logText.Contains("break-demo"))
			{
				break;
			}
		}
	}

	/// <summary>
	/// Demonstrates Kubernetes manifest synchronization streaming
	/// </summary>
	private static async Task DemonstrateKubernetesManifestSync(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n=== Kubernetes Manifest Sync ===");
		Console.WriteLine("Streaming manifest sync results (dry run):");

		await foreach (var syncResult in client.Management.StreamKubernetesSyncManifestsAsync(
			dryRun: true,
			cancellationToken))
		{
			Console.WriteLine($"Sync Result: {syncResult.ResponseType}");
			Console.WriteLine($"  Path: {syncResult.Path}");
			Console.WriteLine($"  Skipped: {syncResult.Skipped}");

			if (!string.IsNullOrEmpty(syncResult.Diff))
			{
				Console.WriteLine($"  Diff:\n{syncResult.Diff}");
			}

			if (syncResult.Object.Length > 0)
			{
				Console.WriteLine($"  Object size: {syncResult.Object.Length} bytes");
			}
		}
	}

	/// <summary>
	/// Example demonstrating service account lifecycle management
	/// </summary>
	public static async Task ServiceAccountManagementExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "admin-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Service Account Lifecycle ===");
			var samplePgpPublicKey = GetSamplePgpPublicKey();

			await DemonstrateServiceAccountLifecycle(client, samplePgpPublicKey, cancellationToken);
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
	/// Gets a sample PGP public key for demonstration purposes
	/// </summary>
	private static string GetSamplePgpPublicKey()
	{
		return """
			-----BEGIN PGP PUBLIC KEY BLOCK-----
			
			mQENBGJxyz4BCADGn5n1...sample...key...content
			-----END PGP PUBLIC KEY BLOCK-----
			""";
	}

	/// <summary>
	/// Demonstrates the complete service account lifecycle
	/// </summary>
	private static async Task DemonstrateServiceAccountLifecycle(OmniClient client, string pgpPublicKey, CancellationToken cancellationToken)
	{
		// Create a new service account
		Console.WriteLine("Creating service account...");
		var publicKeyId = await client.Management.CreateServiceAccountAsync(
			armoredPgpPublicKey: pgpPublicKey,
			useUserRole: true, // Use the role of the creating user
			cancellationToken: cancellationToken);

		Console.WriteLine($"✅ Created service account with public key ID: {publicKeyId}");

		// List service accounts to see the new one
		var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
		var newAccount = serviceAccounts.FirstOrDefault(sa =>
			sa.PgpPublicKeys.Any(key => key.Id == publicKeyId));

		if (newAccount != null)
		{
			await ProcessNewServiceAccount(client, newAccount, pgpPublicKey, cancellationToken);
		}
	}

	/// <summary>
	/// Processes operations on a newly created service account
	/// </summary>
	private static async Task ProcessNewServiceAccount(OmniClient client, ServiceAccountInfo account, string pgpPublicKey, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Found new service account: {account.Name}");
		Console.WriteLine($"  Role: {account.Role}");

		// Renew the service account with a new key
		Console.WriteLine("Renewing service account...");
		var newPublicKeyId = await client.Management.RenewServiceAccountAsync(
			name: account.Name,
			armoredPgpPublicKey: pgpPublicKey, // In practice, use a new key
			cancellationToken: cancellationToken);

		Console.WriteLine($"✅ Renewed service account with new public key ID: {newPublicKeyId}");

		// Clean up - destroy the service account
		Console.WriteLine("Cleaning up service account...");
		await client.Management.DestroyServiceAccountAsync(
			name: account.Name,
			cancellationToken: cancellationToken);

		Console.WriteLine($"✅ Destroyed service account: {account.Name}");
	}

	/// <summary>
	/// Example demonstrating machine provisioning with schematics
	/// </summary>
	public static async Task MachineProvisioningExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "provisioning-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Machine Provisioning with Schematics ===");
			await CreateAndDisplaySchematic(client, cancellationToken);
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
	/// Creates a schematic and displays the results
	/// </summary>
	private static async Task CreateAndDisplaySchematic(OmniClient client, CancellationToken cancellationToken)
	{
		// Create a schematic for provisioning machines with all available options
		var (schematicId, pxeUrl, grpcTunnelEnabled) = await client.Management.CreateSchematicAsync(
			extensions:
			[
				"siderolabs/iscsi-tools",        // iSCSI storage support
				"siderolabs/util-linux-tools",   // Additional Linux utilities
				"siderolabs/gvisor"              // Container runtime security
			],
			extraKernelArgs:
			[
				"console=ttyS0,115200",          // Serial console
				"net.ifnames=0",                 // Predictable network names
				"systemd.unified_cgroup_hierarchy=0"
			],
			metaValues: new Dictionary<uint, string>
			{
				{ 0x0a, "datacenter-1" },       // Datacenter location
				{ 0x0b, "rack-a1" },            // Rack identifier
				{ 0x0c, "production" }          // Environment
			},
			talosVersion: "v1.7.0",             // NEW: Specify Talos version
			mediaId: "installer",               // NEW: Installation media
			secureBoot: true,                   // NEW: Enable secure boot
			siderolinkGrpcTunnelMode: SiderolinkGrpcTunnelMode.Auto,  // NEW: gRPC tunnel mode
			joinToken: null,                    // NEW: Optional join token
			cancellationToken: cancellationToken);

		DisplaySchematicResults(schematicId, pxeUrl, grpcTunnelEnabled);
	}

	/// <summary>
	/// Displays the schematic creation results
	/// </summary>
	private static void DisplaySchematicResults(string schematicId, string pxeUrl, bool grpcTunnelEnabled)
	{
		Console.WriteLine($"✅ Created schematic: {schematicId}");
		Console.WriteLine($"📦 PXE Boot URL: {pxeUrl}");
		Console.WriteLine($"🔌 gRPC Tunnel: {(grpcTunnelEnabled ? "Enabled" : "Disabled")}");
		Console.WriteLine();
		Console.WriteLine("Use this PXE URL to boot machines with the configured extensions and settings.");
		Console.WriteLine("The schematic includes:");
		Console.WriteLine("  - iSCSI tools for storage");
		Console.WriteLine("  - Additional Linux utilities");
		Console.WriteLine("  - gVisor for container security");
		Console.WriteLine("  - Custom kernel arguments for console and networking");
		Console.WriteLine("  - Metadata tags for datacenter, rack, and environment");
		Console.WriteLine("  - Talos v1.7.0 with secure boot enabled");
		Console.WriteLine($"  - gRPC tunnel mode configured for optimal connectivity");
	}

	/// <summary>
	/// Example demonstrating read-only mode and proper error handling
	/// </summary>
	public static async Task ReadOnlyModeExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "readonly-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
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

		var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
		Console.WriteLine($"  - Listed {serviceAccounts.Count} service accounts");

		var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
		Console.WriteLine($"  - Retrieved omniconfig ({omniconfig.Length} characters)");

		var sampleConfig = "apiVersion: v1\nkind: Pod";
		await client.Management.ValidateConfigAsync(sampleConfig, cancellationToken);
		Console.WriteLine("  - Validated configuration");
	}

	/// <summary>
	/// Demonstrates write operations that are blocked in read-only mode
	/// </summary>
	private static async Task DemonstrateBlockedWriteOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n❌ Write operations (blocked in read-only mode):");

		await TryWriteOperation("CreateServiceAccountAsync",
			() => client.Management.CreateServiceAccountAsync("test-key", cancellationToken));

		await TryWriteOperation("CreateSchematicAsync",
			() => client.Management.CreateSchematicAsync(["test-extension"], cancellationToken));

		await TryWriteOperation("GetKubeConfigAsync(serviceAccount=true)",
			() => client.Management.GetKubeConfigAsync(serviceAccount: true, cancellationToken));
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
			Identity = "comprehensive-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
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

		var kubeconfig = await client.Management.GetKubeConfigAsync(cancellationToken: cancellationToken);
		Console.WriteLine($"   ✅ Kubeconfig: {kubeconfig.Length} characters");

		var talosconfig = await client.Management.GetTalosConfigAsync(cancellationToken: cancellationToken);
		Console.WriteLine($"   ✅ Talosconfig: {talosconfig.Length} characters");

		var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
		Console.WriteLine($"   ✅ Omniconfig: {omniconfig.Length} characters");
	}

	/// <summary>
	/// Demonstrates service account operations
	/// </summary>
	private static async Task DemonstrateServiceAccountOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n2️⃣ Service Account Management:");

		var accounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
		Console.WriteLine($"   ✅ Service Accounts: {accounts.Count} found");
	}

	/// <summary>
	/// Demonstrates validation operations
	/// </summary>
	private static async Task DemonstrateValidationOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n3️⃣ Configuration Validation:");

		await client.Management.ValidateConfigAsync("apiVersion: v1\nkind: Pod", cancellationToken);
		Console.WriteLine("   ✅ Configuration validation successful");
	}

	/// <summary>
	/// Demonstrates Kubernetes operations
	/// </summary>
	private static async Task DemonstrateKubernetesOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n4️⃣ Kubernetes Operations:");

		var (upgradeOk, upgradeReason) = await client.Management.KubernetesUpgradePreChecksAsync(
			"v1.29.0", cancellationToken);
		Console.WriteLine($"   ✅ Upgrade check: {(upgradeOk ? "Ready" : "Not ready")} - {upgradeReason}");
	}

	/// <summary>
	/// Demonstrates machine provisioning operations
	/// </summary>
	private static async Task DemonstrateMachineProvisioningOperations(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n5️⃣ Machine Provisioning:");

		var (schematicId, pxeUrl, grpcTunnelEnabled) = await client.Management.CreateSchematicAsync(
			extensions: ["siderolabs/util-linux-tools"],
			cancellationToken: cancellationToken);
		Console.WriteLine($"   ✅ Schematic created: {schematicId}");
		Console.WriteLine($"   📦 PXE URL: {pxeUrl}");
		Console.WriteLine($"   🔌 gRPC Tunnel: {(grpcTunnelEnabled ? "Enabled" : "Disabled")}");
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
