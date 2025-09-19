using SideroLabs.Omni.Api.Examples.Infrastructure;

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

		Output.WriteSection("Configuration Management");

		// Get kubeconfig for cluster access
		var kubeconfig = await client.Management.GetKubeConfigAsync(
			serviceAccount: true,
			serviceAccountTtl: TimeSpan.FromHours(24),
			serviceAccountUser: "automation",
			serviceAccountGroups: ["system:masters"],
			cancellationToken: cts.Token);

		Output.WriteLine("Retrieved kubeconfig ({0} characters)", kubeconfig.Length);
		File.WriteAllText("kubeconfig.yaml", kubeconfig);

		// Get talosconfig for Talos cluster access
		var talosconfig = await client.Management.GetTalosConfigAsync(
			admin: true,
			cancellationToken: cts.Token);

		Output.WriteLine("Retrieved talosconfig ({0} characters)", talosconfig.Length);
		File.WriteAllText("talosconfig.yaml", talosconfig);

		// Get omniconfig for omnictl
		var omniconfig = await client.Management.GetOmniConfigAsync(cts.Token);
		Output.WriteLine("Retrieved omniconfig ({0} characters)", omniconfig.Length);
		File.WriteAllText("omniconfig.yaml", omniconfig);

		Output.WriteSection("Service Account Management");

		// List existing service accounts
		var serviceAccounts = await client.Management.ListServiceAccountsAsync(cts.Token);
		Output.WriteLine("Found {0} service accounts", serviceAccounts.Count);

		foreach (var account in serviceAccounts)
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

		Output.WriteSection("Operational Tasks");

		// Validate a configuration file
		var sampleConfig = """
			apiVersion: v1
			kind: ConfigMap
			metadata:
			  name: test-config
			data:
			  key: value
			""";

		await client.Management.ValidateConfigAsync(sampleConfig, cts.Token);
		Output.WriteSuccess("Configuration validation successful");

		// Check Kubernetes upgrade readiness
		var (canUpgrade, reason) = await client.Management.KubernetesUpgradePreChecksAsync(
			"v1.29.0",
			cts.Token);

		var upgradeStatus = canUpgrade ? "Ready" : "Not ready";
		Output.WriteLine("Kubernetes upgrade to v1.29.0: {0} {1}", canUpgrade ? "✅" : "❌", upgradeStatus);
		if (!string.IsNullOrEmpty(reason))
		{
			Output.WriteLine("Reason: {0}", reason);
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
