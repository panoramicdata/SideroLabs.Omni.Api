using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Example usage of the OmniClient
/// </summary>
public class OmniClientExample
{
	/// <summary>
	/// Example of how to use the OmniClient to interact with the Omni Management API
	/// </summary>
	public static async Task ExampleUsage()
	{
		// Configure the client options
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com:8443",
			AuthToken = "your-auth-token-here",
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
			// Get service status
			var status = await client.GetStatusAsync(cancellationToken);
			Console.WriteLine($"Omni service version: {status.Version}, Ready: {status.Ready}");

			// Create a new cluster
			var clusterSpec = new ClusterSpec
			{
				KubernetesVersion = "v1.28.0",
				TalosVersion = "v1.5.0",
				Features = new List<string> { "embedded-discovery-service" }
			};

			var createResponse = await client.CreateClusterAsync("test-cluster", clusterSpec, cancellationToken);
			Console.WriteLine($"Created cluster: {createResponse.Cluster.Name} (ID: {createResponse.Cluster.Id})");

			// List all clusters
			var clusters = await client.ListClustersAsync(cancellationToken);
			Console.WriteLine($"Found {clusters.Clusters.Count} clusters");

			foreach (var cluster in clusters.Clusters)
			{
				Console.WriteLine($"Cluster: {cluster.Name} (ID: {cluster.Id})");
				Console.WriteLine($"  Kubernetes: {cluster.Spec.KubernetesVersion}");
				Console.WriteLine($"  Talos: {cluster.Spec.TalosVersion}");
				Console.WriteLine($"  Status: {cluster.Status.Phase}, Ready: {cluster.Status.Ready}");

				// List machines in this cluster
				var machines = await client.ListMachinesAsync(cluster.Id, cancellationToken);
				Console.WriteLine($"  Machines: {machines.Machines.Count}");

				foreach (var machine in machines.Machines)
				{
					Console.WriteLine($"    Machine: {machine.Name} - {machine.Status.Phase}");
					Console.WriteLine($"      Role: {machine.Spec.Role}");
					Console.WriteLine($"      Address: {machine.Status.Address}");
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
}