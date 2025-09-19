using SideroLabs.Omni.Api.Examples.Scenarios;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Main program for running Omni API examples
/// </summary>
public static class Program
{
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
			switch (exampleName)
			{
				case "basic":
					Console.WriteLine("Running Basic Usage Example...");
					await OmniClientExample.BasicUsageExample();
					break;

				case "streaming":
					Console.WriteLine("Running Advanced Streaming Example...");
					await OmniClientExample.AdvancedStreamingExample();
					break;

				case "service-accounts":
					Console.WriteLine("Running Service Account Management Example...");
					await OmniClientExample.ServiceAccountManagementExample();
					break;

				case "provisioning":
					Console.WriteLine("Running Machine Provisioning Example...");
					await OmniClientExample.MachineProvisioningExample();
					break;

				case "readonly":
					Console.WriteLine("Running Read-Only Mode Example...");
					await OmniClientExample.ReadOnlyModeExample();
					break;

				case "comprehensive":
					Console.WriteLine("Running Comprehensive Management Service Example...");
					await OmniClientExample.ComprehensiveManagementServiceExample();
					break;

				case "structured":
					Console.WriteLine("Running Structured Basic Usage Example...");
					await BasicUsageExample.DemonstrateAsync(cts.Token);
					break;

				case "all":
					await RunAllExamples(cts.Token);
					break;

				default:
					Console.WriteLine($"Unknown example: {exampleName}");
					ShowUsage();
					Environment.Exit(1);
					break;
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

	/// <summary>
	/// Shows usage information
	/// </summary>
	private static void ShowUsage()
	{
		Console.WriteLine("Usage: SideroLabs.Omni.Api.Examples <example-name> [options]");
		Console.WriteLine();
		Console.WriteLine("Available examples:");
		Console.WriteLine("  basic              - Basic usage example with configuration management");
		Console.WriteLine("  streaming          - Advanced streaming operations and machine management");
		Console.WriteLine("  service-accounts   - Service account lifecycle management");
		Console.WriteLine("  provisioning       - Machine provisioning with schematics");
		Console.WriteLine("  readonly           - Read-only mode demonstration");
		Console.WriteLine("  comprehensive      - All available ManagementService operations");
		Console.WriteLine("  structured         - Structured basic usage example");
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
		var examples = new (string Name, Func<Task> ExampleFunc)[]
		{
			("Basic Usage", () => OmniClientExample.BasicUsageExample()),
			("Structured Basic Usage", () => BasicUsageExample.DemonstrateAsync(cancellationToken)),
			("Read-Only Mode", () => OmniClientExample.ReadOnlyModeExample()),
			("Advanced Streaming", () => OmniClientExample.AdvancedStreamingExample()),
			("Service Account Management", () => OmniClientExample.ServiceAccountManagementExample()),
			("Machine Provisioning", () => OmniClientExample.MachineProvisioningExample()),
			("Comprehensive Management Service", () => OmniClientExample.ComprehensiveManagementServiceExample())
		};

		for (var i = 0; i < examples.Length; i++)
		{
			var (name, exampleFunc) = examples[i];

			Console.WriteLine($"\n--- Example {i + 1}/{examples.Length}: {name} ---");

			try
			{
				await exampleFunc();
				Console.WriteLine($"✅ {name} completed successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ {name} failed: {ex.Message}");
			}

			// Wait a moment between examples (unless cancelled)
			if (i < examples.Length - 1)
			{
				Console.WriteLine("\nPress any key to continue to the next example, or Ctrl+C to stop...");
				var keyTask = Task.Run(() => Console.ReadKey(true), cancellationToken);
				try
				{
					await keyTask;
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine("\nStopping examples execution.");
					break;
				}
			}
		}
	}
}
