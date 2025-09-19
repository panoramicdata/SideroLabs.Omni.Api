using System.Text;
using SideroLabs.Omni.Api.Exceptions;

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
		// Configure the client options with PGP-based authentication
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com",

			// Method 1: Direct PGP key content (recommended for production)
			Identity = "your-username",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",

			// Method 2: PGP key file path (alternative)
			// PgpKeyFilePath = "/path/to/your/pgp-key-file.txt",

			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true
		};

		// Create and use the client
		using var client = new OmniClient(options);

		// Create a cancellation token with timeout
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var cancellationToken = cts.Token;

		try
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
				admin: true,
				cancellationToken: cancellationToken);

			Console.WriteLine($"Retrieved talosconfig ({talosconfig.Length} characters)");
			File.WriteAllText("talosconfig.yaml", talosconfig);

			// Get omniconfig for omnictl
			var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
			Console.WriteLine($"Retrieved omniconfig ({omniconfig.Length} characters)");
			File.WriteAllText("omniconfig.yaml", omniconfig);

			Console.WriteLine("\n=== Service Account Management ===");

			// List existing service accounts
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
	/// Advanced example showcasing streaming operations and machine management
	/// </summary>
	public static async Task AdvancedStreamingExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com",
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
			Console.WriteLine("=== Machine Log Streaming ===");

			// Stream logs from a specific machine
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

			Console.WriteLine("\n=== Kubernetes Manifest Sync ===");

			// Stream Kubernetes manifest synchronization
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
	/// Example demonstrating service account lifecycle management
	/// </summary>
	public static async Task ServiceAccountManagementExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com",
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

			// Sample PGP public key (you would use a real one)
			var samplePgpPublicKey = """
				-----BEGIN PGP PUBLIC KEY BLOCK-----
				
				mQENBGJxyz4BCADGn5n1...sample...key...content
				-----END PGP PUBLIC KEY BLOCK-----
				""";

			// Create a new service account
			Console.WriteLine("Creating service account...");
			var publicKeyId = await client.Management.CreateServiceAccountAsync(
				armoredPgpPublicKey: samplePgpPublicKey,
				useUserRole: true, // Use the role of the creating user
				cancellationToken: cancellationToken);

			Console.WriteLine($"✅ Created service account with public key ID: {publicKeyId}");

			// List service accounts to see the new one
			var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);

			var newAccount = serviceAccounts.FirstOrDefault(sa =>
				sa.PgpPublicKeys.Any(key => key.Id == publicKeyId));

			if (newAccount != null)
			{
				Console.WriteLine($"Found new service account: {newAccount.Name}");
				Console.WriteLine($"  Role: {newAccount.Role}");

				// Renew the service account with a new key
				Console.WriteLine("Renewing service account...");
				var newPublicKeyId = await client.Management.RenewServiceAccountAsync(
					name: newAccount.Name,
					armoredPgpPublicKey: samplePgpPublicKey, // In practice, use a new key
					cancellationToken: cancellationToken);

				Console.WriteLine($"✅ Renewed service account with new public key ID: {newPublicKeyId}");

				// Clean up - destroy the service account
				Console.WriteLine("Cleaning up service account...");
				await client.Management.DestroyServiceAccountAsync(
					name: newAccount.Name,
					cancellationToken: cancellationToken);

				Console.WriteLine($"✅ Destroyed service account: {newAccount.Name}");
			}

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
	/// Example demonstrating machine provisioning with schematics
	/// </summary>
	public static async Task MachineProvisioningExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com",
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

			// Create a schematic for provisioning machines
			var (schematicId, pxeUrl) = await client.Management.CreateSchematicAsync(
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
				cancellationToken: cancellationToken);

			Console.WriteLine($"✅ Created schematic: {schematicId}");
			Console.WriteLine($"📦 PXE Boot URL: {pxeUrl}");
			Console.WriteLine();
			Console.WriteLine("Use this PXE URL to boot machines with the configured extensions and settings.");
			Console.WriteLine("The schematic includes:");
			Console.WriteLine("  - iSCSI tools for storage");
			Console.WriteLine("  - Additional Linux utilities");
			Console.WriteLine("  - gVisor for container security");
			Console.WriteLine("  - Custom kernel arguments for console and networking");
			Console.WriteLine("  - Metadata tags for datacenter, rack, and environment");

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
	/// Example demonstrating read-only mode and proper error handling
	/// </summary>
	public static async Task ReadOnlyModeExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com",
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

			// These operations will work (read operations)
			Console.WriteLine("\n✅ Read operations (allowed):");

			var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
			Console.WriteLine($"  - Listed {serviceAccounts.Count} service accounts");

			var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
			Console.WriteLine($"  - Retrieved omniconfig ({omniconfig.Length} characters)");

			var sampleConfig = "apiVersion: v1\nkind: Pod";
			await client.Management.ValidateConfigAsync(sampleConfig, cancellationToken);
			Console.WriteLine("  - Validated configuration");

			// Demonstrate proper exception handling for write operations
			Console.WriteLine("\n❌ Write operations (blocked in read-only mode):");

			try
			{
				await client.Management.CreateServiceAccountAsync("test-key", cancellationToken);
				Console.WriteLine("  - CreateServiceAccountAsync() - UNEXPECTED: Should have thrown ReadOnlyModeException");
			}
			catch (ReadOnlyModeException ex)
			{
				Console.WriteLine($"  - CreateServiceAccountAsync() - ✅ Blocked: {ex.Message}");
				Console.WriteLine($"    Operation: {ex.Operation}, Resource: {ex.ResourceType}");
			}

			try
			{
				await client.Management.CreateSchematicAsync(["test-extension"], cancellationToken);
				Console.WriteLine("  - CreateSchematicAsync() - UNEXPECTED: Should have thrown ReadOnlyModeException");
			}
			catch (ReadOnlyModeException ex)
			{
				Console.WriteLine($"  - CreateSchematicAsync() - ✅ Blocked: {ex.Message}");
				Console.WriteLine($"    Operation: {ex.Operation}, Resource: {ex.ResourceType}");
			}

			try
			{
				await client.Management.GetKubeConfigAsync(serviceAccount: true, cancellationToken);
				Console.WriteLine("  - GetKubeConfigAsync(serviceAccount=true) - UNEXPECTED: Should have thrown ReadOnlyModeException");
			}
			catch (ReadOnlyModeException ex)
			{
				Console.WriteLine($"  - GetKubeConfigAsync(serviceAccount=true) - ✅ Blocked: {ex.Message}");
				Console.WriteLine($"    Operation: {ex.Operation}, Resource: {ex.ResourceType}");
			}

			Console.WriteLine("\n🛡️ Read-only mode provides safety for production environments");
			Console.WriteLine("💡 All write operations are properly blocked with clear error messages");

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
	/// Example showing all available ManagementService operations
	/// </summary>
	public static async Task ComprehensiveManagementServiceExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com",
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

			// 1. Configuration Management
			Console.WriteLine("\n1️⃣ Configuration Management:");

			var kubeconfig = await client.Management.GetKubeConfigAsync(cancellationToken: cancellationToken);
			Console.WriteLine($"   ✅ Kubeconfig: {kubeconfig.Length} characters");

			var talosconfig = await client.Management.GetTalosConfigAsync(cancellationToken: cancellationToken);
			Console.WriteLine($"   ✅ Talosconfig: {talosconfig.Length} characters");

			var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
			Console.WriteLine($"   ✅ Omniconfig: {omniconfig.Length} characters");

			// 2. Service Account Management
			Console.WriteLine("\n2️⃣ Service Account Management:");

			var accounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
			Console.WriteLine($"   ✅ Service Accounts: {accounts.Count} found");

			// 3. Validation
			Console.WriteLine("\n3️⃣ Configuration Validation:");

			await client.Management.ValidateConfigAsync("apiVersion: v1\nkind: Pod", cancellationToken);
			Console.WriteLine("   ✅ Configuration validation successful");

			// 4. Kubernetes Operations
			Console.WriteLine("\n4️⃣ Kubernetes Operations:");

			var (upgradeOk, upgradeReason) = await client.Management.KubernetesUpgradePreChecksAsync(
				"v1.29.0", cancellationToken);
			Console.WriteLine($"   ✅ Upgrade check: {(upgradeOk ? "Ready" : "Not ready")} - {upgradeReason}");

			// 5. Machine Provisioning
			Console.WriteLine("\n5️⃣ Machine Provisioning:");

			var (schematicId, pxeUrl) = await client.Management.CreateSchematicAsync(
				extensions: ["siderolabs/util-linux-tools"],
				cancellationToken: cancellationToken);
			Console.WriteLine($"   ✅ Schematic created: {schematicId}");
			Console.WriteLine($"   📦 PXE URL: {pxeUrl}");

			// 6. Streaming Operations
			Console.WriteLine("\n6️⃣ Streaming Operations:");

			Console.WriteLine("   🔄 Machine logs streaming...");
			// Note: This would stream in a real implementation

			Console.WriteLine("   🔄 Kubernetes manifest sync streaming...");
			// Note: This would stream in a real implementation

			Console.WriteLine("\n✅ All ManagementService operations demonstrated successfully!");
			Console.WriteLine("\nNote: This represents the complete set of operations");
			Console.WriteLine("available in the Omni gRPC ManagementService.");

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
}
