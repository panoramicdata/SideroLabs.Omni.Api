using SideroLabs.Omni.Api.Examples.Scenarios;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Main program for running Omni API examples
/// </summary>
public static class Program
{
	private static readonly (string Key, string Title, Func<CancellationToken, Task> Run)[] Examples =
	[
		("basic", "Basic Usage", _ => OmniClientExample.BasicUsageExample()),
		("structured", "Structured Basic Usage", BasicUsageExample.DemonstrateAsync),
		("readonly", "Read-Only Mode", _ => OmniClientExample.ReadOnlyModeExample()),
		("streaming", "Advanced Streaming", _ => OmniClientExample.AdvancedStreamingExample()),
		("service-accounts", "Service Account Management", _ => OmniClientExample.ServiceAccountManagementExample()),
		("provisioning", "Machine Provisioning", _ => OmniClientExample.MachineProvisioningExample()),
		("comprehensive", "Comprehensive Management Service", _ => OmniClientExample.ComprehensiveManagementServiceExample())
	];

	/// <summary>
	/// Main entry point for the examples application
	/// </summary>
	/// <param name="args">Command line arguments</param>
	public static async Task Main(string[] args)
	{
		Console.WriteLine("=== SideroLabs Omni API Examples ===");
		Console.WriteLine();

		if (args.Length == 0)
		{
			ShowUsage();
			return;
		}

		var exampleName = args[0].ToLowerInvariant();
		using var cts = new CancellationTokenSource();

		// Handle Ctrl+C gracefully
		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			cts.Cancel();
			Console.WriteLine("\nExample cancelled by user.");
		};

		try
		{
			if (!await TryRunAsync(exampleName, cts.Token))
			{
				Console.WriteLine($"Unknown example: {exampleName}");
				ShowUsage();
				Environment.Exit(1);
			}
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("\nExample execution was cancelled.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"\nError running example: {ex.Message}");
			if (args.Contains("--verbose"))
			{
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
			}

			Environment.Exit(1);
		}

		Console.WriteLine("\nExample completed successfully.");
	}

	private static async Task<bool> TryRunAsync(string exampleName, CancellationToken cancellationToken)
	{
		if (exampleName == "all")
		{
			await RunAllExamples(cancellationToken);
			return true;
		}

		var example = Examples.FirstOrDefault(e => e.Key == exampleName);
		if (example.Run == null)
		{
			return false;
		}

		Console.WriteLine($"Running {example.Title} Example...");
		await example.Run(cancellationToken);
		return true;
	}

	/// <summary>
	/// Shows usage information
	/// </summary>
	private static void ShowUsage()
	{
		Console.WriteLine("Usage: SideroLabs.Omni.Api.Examples <example-name> [options]");
		Console.WriteLine();
		Console.WriteLine("Available examples:");
		foreach (var (key, title, _) in Examples)
		{
			Console.WriteLine($"  {key,-18} - {title}");
		}

		Console.WriteLine("  all                - Run all examples sequentially");
		Console.WriteLine();
		Console.WriteLine("Options:");
		Console.WriteLine("  --verbose          - Show detailed error information");
		Console.WriteLine();
		Console.WriteLine("Examples:");
		Console.WriteLine("  SideroLabs.Omni.Api.Examples basic");
		Console.WriteLine("  SideroLabs.Omni.Api.Examples readonly --verbose");
		Console.WriteLine("  SideroLabs.Omni.Api.Examples all");
	}

	/// <summary>
	/// Runs all examples sequentially
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	private static async Task RunAllExamples(CancellationToken cancellationToken)
	{
		for (var i = 0; i < Examples.Length; i++)
		{
			var (_, name, run) = Examples[i];

			Console.WriteLine($"\n--- Example {i + 1}/{Examples.Length}: {name} ---");

			try
			{
				await run(cancellationToken);
				Console.WriteLine($"✅ {name} completed successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ {name} failed: {ex.Message}");
			}

			if (i < Examples.Length - 1 && !await WaitForKeyAsync(cancellationToken))
			{
				break;
			}
		}
	}

	private static async Task<bool> WaitForKeyAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("\nPress any key to continue to the next example, or Ctrl+C to stop...");
		try
		{
			await Task.Run(() => Console.ReadKey(true), cancellationToken);
			return true;
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("\nStopping examples execution.");
			return false;
		}
	}
}