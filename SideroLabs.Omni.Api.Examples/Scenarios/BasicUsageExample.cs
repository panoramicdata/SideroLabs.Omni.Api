using SideroLabs.Omni.Api.Examples.Infrastructure;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples.Scenarios;

/// <summary>
/// Example demonstrating basic OmniClient usage
/// </summary>
/// <param name="output">Output interface</param>
public class BasicUsageExample(IExampleOutput output) : ExampleBase(output)
{
	/// <inheritdoc />
	protected override async Task ExecuteExampleAsync(CancellationToken cancellationToken)
	{
		// Configure the client options
		var options = ExampleConfigurationFactory.CreateStandardOptions();

		// Create and use the client
		using var client = new OmniClient(options);
		using var cts = CreateTimeoutSource(TimeSpan.FromSeconds(30));

		await DemonstrateConfigurationManagement(client, cts.Token);
		await DemonstrateServiceAccountManagement(client, cts.Token);
		await DemonstrateOperationalTasks(client, cts.Token);
	}

	/// <summary>
	/// Demonstrates configuration management operations
	/// </summary>
	private async Task DemonstrateConfigurationManagement(OmniClient client, CancellationToken cancellationToken)
	{
		Output.WriteSection("Configuration Management");

		// Get kubeconfig for cluster access with all available options
		var kubeconfig = await client.Management.GetKubeConfigAsync(
			serviceAccount: true,
			serviceAccountTtl: TimeSpan.FromHours(24),
			serviceAccountUser: "automation",
			serviceAccountGroups: ["system:masters"],
			grantType: "token",  // New parameter
			breakGlass: false,   // New parameter
			cancellationToken: cancellationToken);

		Output.WriteLine("Retrieved kubeconfig ({0} characters)", kubeconfig.Length);
		File.WriteAllText("kubeconfig.yaml", kubeconfig);

		// Get talosconfig for Talos cluster access with break-glass option
		var talosconfig = await client.Management.GetTalosConfigAsync(
			raw: true,
			breakGlass: false,  // New parameter
			cancellationToken: cancellationToken);

		Output.WriteLine("Retrieved talosconfig ({0} characters)", talosconfig.Length);
		File.WriteAllText("talosconfig.yaml", talosconfig);

		// Get omniconfig for omnictl
		var omniconfig = await client.Management.GetOmniConfigAsync(cancellationToken);
		Output.WriteLine("Retrieved omniconfig ({0} characters)", omniconfig.Length);
		File.WriteAllText("omniconfig.yaml", omniconfig);
	}

	/// <summary>
	/// Demonstrates service account management operations
	/// </summary>
	private async Task DemonstrateServiceAccountManagement(OmniClient client, CancellationToken cancellationToken)
	{
		Output.WriteSection("Service Account Management");

		// List existing service accounts
		var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);
		Output.WriteLine("Found {0} service accounts", serviceAccounts.Count);

		foreach (var account in serviceAccounts)
		{
			DisplayServiceAccountInfo(account);
		}
	}

	/// <summary>
	/// Displays information about a service account
	/// </summary>
	private void DisplayServiceAccountInfo(ServiceAccountInfo account)
	{
		Output.WriteLine("Service Account: {0}", account.Name);
		Output.WriteLine("  Role: {0}", account.Role);
		Output.WriteLine("  PGP Keys: {0}", account.PgpPublicKeys.Count);

		foreach (var key in account.PgpPublicKeys)
		{
			Output.WriteLine("    Key ID: {0}", key.Id);
			Output.WriteLine("    Expires: {0:yyyy-MM-dd HH:mm:ss}", key.Expiration);
		}
	}

	/// <summary>
	/// Demonstrates operational tasks
	/// </summary>
	private async Task DemonstrateOperationalTasks(OmniClient client, CancellationToken cancellationToken)
	{
		Output.WriteSection("Operational Tasks");

		await ValidateConfiguration(client, cancellationToken);
		await ValidateJsonSchema(client, cancellationToken);
		await CheckKubernetesUpgrade(client, cancellationToken);
	}

	/// <summary>
	/// Validates a sample configuration
	/// </summary>
	private async Task ValidateConfiguration(OmniClient client, CancellationToken cancellationToken)
	{
		var sampleConfig = """
			apiVersion: v1
			kind: ConfigMap
			metadata:
			  name: test-config
			data:
			  key: value
			""";

		await client.Management.ValidateConfigAsync(sampleConfig, cancellationToken);
		Output.WriteSuccess("Configuration validation successful");
	}

	/// <summary>
	/// Validates JSON data against a schema
	/// </summary>
	private async Task ValidateJsonSchema(OmniClient client, CancellationToken cancellationToken)
	{
		var jsonSchema = """
			{
			  "type": "object",
			  "properties": {
			    "cluster": { "type": "string" },
			    "version": { "type": "string" },
			    "replicas": { "type": "number", "minimum": 1 }
			  },
			  "required": ["cluster", "version"]
			}
			""";

		var jsonData = """
			{
			  "cluster": "production",
			  "version": "1.29.0",
			  "replicas": 3
			}
			""";

		var result = await client.Management.ValidateJsonSchemaAsync(jsonData, jsonSchema, cancellationToken);

		if (result.IsValid)
		{
			Output.WriteSuccess("JSON schema validation successful");
		}
		else
		{
			Output.WriteError($"JSON schema validation failed with {result.TotalErrorCount} error(s)");
			Output.WriteLine(result.GetErrorSummary());
		}
	}

	/// <summary>
	/// Checks Kubernetes upgrade readiness
	/// </summary>
	private async Task CheckKubernetesUpgrade(OmniClient client, CancellationToken cancellationToken)
	{
		var upgradePreCheckResult = await client.Management.KubernetesUpgradePreChecksAsync(
			"v1.29.0",
			cancellationToken);

		var upgradeStatus = upgradePreCheckResult.Ok ? "Ready" : "Not ready";
		Output.WriteLine("Kubernetes upgrade to v1.29.0: {0} {1}", upgradePreCheckResult.Ok ? "✅" : "❌", upgradeStatus);
		if (!string.IsNullOrEmpty(upgradePreCheckResult.Reason))
		{
			Output.WriteLine("Reason: {0}", upgradePreCheckResult.Reason);
		}
	}

	/// <summary>
	/// Demonstrates the basic usage example
	/// </summary>
	/// <returns>Task representing the example execution</returns>
	public static Task DemonstrateAsync() => DemonstrateAsync(CancellationToken.None);

	/// <summary>
	/// Demonstrates the basic usage example
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Task representing the example execution</returns>
	public static async Task DemonstrateAsync(CancellationToken cancellationToken)
	{
		var output = new ConsoleExampleOutput();
		var example = new BasicUsageExample(output);
		await example.RunAsync(cancellationToken);
	}
}
