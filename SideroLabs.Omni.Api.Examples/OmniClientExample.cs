using System.Text;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Examples of the gRPC-only OmniClient API
/// Demonstrates the actual ManagementService operations available in Omni
/// </summary>
public static partial class OmniClientExample
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
			AuthToken = "your-auth-token",
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

		// Get kubeconfig for cluster access - NEW API!
		var kubeconfig = await client
				.KubeConfig
				.GetAsync(
					true,
					TimeSpan.FromHours(24),
					"automation",
					["system:masters"],
					cancellationToken: cancellationToken);

		Console.WriteLine($"Retrieved kubeconfig ({kubeconfig.Length} characters)");
		File.WriteAllText("kubeconfig.yaml", kubeconfig);

		// Get talosconfig for Talos cluster access - NEW API!
		var talosconfig = await client
			.TalosConfig
			.GetAsync(
				true,
				cancellationToken: cancellationToken);

		Console.WriteLine($"Retrieved talosconfig ({talosconfig.Length} characters)");
		File.WriteAllText("talosconfig.yaml", talosconfig);

		// Get omniconfig for omnictl - NEW API!
		var omniconfig = await client.OmniConfig.GetAsync(cancellationToken);
		Console.WriteLine($"Retrieved omniconfig ({omniconfig.Length} characters)");
		File.WriteAllText("omniconfig.yaml", omniconfig);
	}

	/// <summary>
	/// Demonstrates service account listing and information display
	/// </summary>
	private static async Task DemonstrateServiceAccountListing(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("\n=== Service Account Management ===");

		// NEW API!
		var serviceAccounts = await client.ServiceAccounts.ListAsync(cancellationToken);
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

		// Validate a configuration file - NEW API!
		var sampleConfig = """
			apiVersion: v1
			kind: ConfigMap
			metadata:
			  name: test-config
			data:
			  key: value
			""";

		await client
			.Validation
			.ValidateConfigAsync(sampleConfig, cancellationToken);
		Console.WriteLine("Configuration validation successful");

		// Check Kubernetes upgrade readiness - NEW API!
		var upgradePreCheckResult = await client
			.Kubernetes
			.UpgradePreChecksAsync(
				"v1.29.0",
				cancellationToken);

		Console.WriteLine($"Kubernetes upgrade to v1.29.0: {(upgradePreCheckResult.Ok ? "✅ Ready" : "❌ Not ready")}");
		if (!string.IsNullOrEmpty(upgradePreCheckResult.Reason))
		{
			Console.WriteLine($"Reason: {upgradePreCheckResult.Reason}");
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
			PgpPrivateKey = "<your armored PGP private key>",
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

		// NEW API!
		await foreach (var logData in client.MachineManagement.StreamLogsAsync(
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

		// NEW API!
		await foreach (var syncResult in client.Kubernetes.StreamSyncManifestsAsync(
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

}
