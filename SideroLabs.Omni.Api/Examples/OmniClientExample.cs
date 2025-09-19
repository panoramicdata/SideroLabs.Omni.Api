using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Comprehensive examples of the service-based OmniClient API
/// </summary>
public class OmniClientExample
{
	/// <summary>
	/// Basic example of how to use the OmniClient to interact with the Omni Management API
	/// </summary>
	public static async Task BasicUsageExample()
	{
		// Configure the client options with PGP-based authentication
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com:8443",

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
			// Get service status
			var status = await client.Status.GetStatusAsync(cancellationToken);
			Console.WriteLine($"Omni service version: {status.Version}, Ready: {status.Ready}");

			// Create a new cluster
			var clusterSpec = new ClusterSpec
			{
				KubernetesVersion = "v1.28.0",
				TalosVersion = "v1.5.0",
				Features = ["embedded-discovery-service"]
			};

			var createResponse = await client.Clusters.CreateClusterAsync("test-cluster", clusterSpec, cancellationToken);
			Console.WriteLine($"Created cluster: {createResponse.Cluster.Name} (ID: {createResponse.Cluster.Id})");

			// List all clusters
			var clusters = await client.Clusters.ListClustersAsync(cancellationToken);
			Console.WriteLine($"Found {clusters.Clusters.Count} clusters");

			foreach (var cluster in clusters.Clusters)
			{
				Console.WriteLine($"Cluster: {cluster.Name} (ID: {cluster.Id})");
				Console.WriteLine($"  Kubernetes: {cluster.Spec.KubernetesVersion}");
				Console.WriteLine($"  Talos: {cluster.Spec.TalosVersion}");
				Console.WriteLine($"  Status: {cluster.Status.Phase}, Ready: {cluster.Status.Ready}");

				// List machines in this cluster
				var machines = await client.Machines.ListMachinesAsync(cluster.Id, cancellationToken);
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

	/// <summary>
	/// Advanced example showcasing multiple service areas
	/// </summary>
	public static async Task AdvancedUsageExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com:8443",
			Identity = "your-username",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 60,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Service Status ===");
			var enhancedStatus = await client.Status.GetEnhancedStatusAsync(cancellationToken);
			Console.WriteLine($"Version: {enhancedStatus.Version}");
			Console.WriteLine($"Health: {enhancedStatus.Health.Status}");
			Console.WriteLine($"Total Clusters: {enhancedStatus.SystemStats.TotalClusters}");

			Console.WriteLine("\n=== Workspace Management ===");
			var workspaces = await client.Workspaces.ListWorkspacesAsync(cancellationToken);
			foreach (var workspace in workspaces.Workspaces)
			{
				Console.WriteLine($"Workspace: {workspace.Name} ({workspace.Status.Phase})");
				Console.WriteLine($"  Clusters: {workspace.Status.ClusterCount}");
				Console.WriteLine($"  CPU Usage: {workspace.Status.ResourceUsage.CpuCores} cores");
			}

			Console.WriteLine("\n=== Backup Operations ===");
			var allBackups = await client.Backups.ListBackupsAsync(cancellationToken);
			Console.WriteLine($"Total backups: {allBackups.Backups.Count}");

			// List backups for a specific cluster
			if (allBackups.Backups.Any())
			{
				var clusterId = allBackups.Backups.First().Spec.ClusterId;
				var clusterBackups = await client.Backups.ListBackupsAsync(clusterId, cancellationToken);
				Console.WriteLine($"Backups for cluster {clusterId}: {clusterBackups.Backups.Count}");
			}

			Console.WriteLine("\n=== Configuration Templates ===");
			var allTemplates = await client.ConfigurationTemplates.ListConfigTemplatesAsync(cancellationToken);
			Console.WriteLine($"Total templates: {allTemplates.Templates.Count}");

			// List templates by type
			var clusterTemplates = await client.ConfigurationTemplates.ListConfigTemplatesAsync(ConfigTemplateType.Cluster, cancellationToken);
			Console.WriteLine($"Cluster templates: {clusterTemplates.Templates.Count}");

			Console.WriteLine("\n=== Network Configuration ===");
			var networkConfigs = await client.Networks.ListNetworkConfigsAsync(cancellationToken);
			foreach (var config in networkConfigs.NetworkConfigs)
			{
				Console.WriteLine($"Network Config: {config.Name} ({config.Status.Phase})");
			}

			Console.WriteLine("\n=== Kubernetes Integration ===");
			var clusters = await client.Clusters.ListClustersAsync(cancellationToken);
			foreach (var cluster in clusters.Clusters.Take(1)) // Just first cluster for demo
			{
				// Get cluster metrics for all time
				var allTimeMetrics = await client.Kubernetes.GetClusterMetricsAsync(cluster.Id, cancellationToken);
				Console.WriteLine($"Cluster {cluster.Name} CPU: {allTimeMetrics.CpuUsage.CurrentValue}%");

				// Get cluster metrics for specific time range
				var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				var hourAgo = now - 3600;
				var rangeMetrics = await client.Kubernetes.GetClusterMetricsAsync(cluster.Id, hourAgo, now, cancellationToken);
				Console.WriteLine($"  Last hour average CPU: {rangeMetrics.CpuUsage.DataPoints.Average(dp => dp.Value):F1}%");

				// Get node metrics for all nodes
				var allNodeMetrics = await client.Kubernetes.GetNodeMetricsAsync(cluster.Id, cancellationToken);
				Console.WriteLine($"  Total nodes: {allNodeMetrics.NodeMetrics.Count}");

				// Get pod metrics for all namespaces
				var allPodMetrics = await client.Kubernetes.GetPodMetricsAsync(cluster.Id, cancellationToken);
				Console.WriteLine($"  Total pods: {allPodMetrics.PodMetrics.Count}");

				// Get pod metrics for specific namespace
				var defaultPodMetrics = await client.Kubernetes.GetPodMetricsAsync(cluster.Id, "default", cancellationToken);
				Console.WriteLine($"  Pods in default namespace: {defaultPodMetrics.PodMetrics.Count}");

				// Get kubeconfig
				var kubeConfig = await client.Kubernetes.GetKubernetesConfigAsync(cluster.Id, cancellationToken);
				Console.WriteLine($"  Kubeconfig server: {kubeConfig.Server}");
			}

			Console.WriteLine("\n=== Log Management ===");
			foreach (var cluster in clusters.Clusters.Take(1)) // Just first cluster for demo
			{
				var logStreams = await client.Logs.GetLogStreamsAsync(cluster.Id, cancellationToken);
				Console.WriteLine($"Available log streams for {cluster.Name}: {logStreams.LogStreams.Count}");

				foreach (var stream in logStreams.LogStreams.Take(1)) // Just first stream for demo
				{
					var logs = await client.Logs.GetLogsAsync(stream.Source, stream.Spec, cancellationToken);
					Console.WriteLine($"  Stream {stream.Id}: {logs.LogEntries.Count} recent entries");
				}
			}

			Console.WriteLine("\n=== Health Checks ===");
			var allHealthChecks = await client.Status.GetHealthCheckAsync(cancellationToken);
			Console.WriteLine($"Overall health: {allHealthChecks.Status}");

			// Check specific component
			var dbHealthCheck = await client.Status.GetHealthCheckAsync("database", cancellationToken);
			Console.WriteLine($"Database health: {dbHealthCheck.Status}");

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
	/// Example demonstrating error handling and safety features
	/// </summary>
	public static async Task SafetyAndErrorHandlingExample()
	{
		var options = new OmniClientOptions
		{
			Endpoint = "https://your-omni-instance.example.com:8443",
			Identity = "david-bond", // This will trigger safety mechanisms
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
			Console.WriteLine("=== Testing Safety Mechanisms ===");

			// This will work (read operation)
			var status = await client.Status.GetStatusAsync(cancellationToken);
			Console.WriteLine($"✅ Read operation successful: {status.Version}");

			// These will fail due to safety mechanisms with production credentials
			try
			{
				var clusterSpec = new ClusterSpec
				{
					KubernetesVersion = "v1.28.0",
					TalosVersion = "v1.5.0"
				};
				await client.Clusters.CreateClusterAsync("dangerous-cluster", clusterSpec, cancellationToken);
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine($"🛡️  Safety mechanism triggered: {ex.Message}");
			}

			try
			{
				await client.Clusters.DeleteClusterAsync("some-cluster-id", cancellationToken);
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine($"🛡️  Safety mechanism triggered: {ex.Message}");
			}

			Console.WriteLine("Safety mechanisms are working correctly!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error: {ex.Message}");
		}
	}
}
