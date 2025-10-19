using System.Diagnostics;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Diagnostic test to check what format COSI State returns
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Diagnostic")]
public class SpecFormatDiagnosticTest(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task CheckSpecFormat_LogsWhatServerReturns()
	{
		// Arrange
		Console.WriteLine("========================================");
		Console.WriteLine("?? DIAGNOSTIC: Checking COSI State spec format");
		Console.WriteLine("========================================");

		// Act - Get the first cluster
		var found = false;
		await foreach (var cluster in OmniClient.Resources.ListAsync<Cluster>(
			cancellationToken: CancellationToken))
		{
			Console.WriteLine($"?? Cluster ID: {cluster.Metadata.Id}");
			Console.WriteLine($"   Namespace: {cluster.Metadata.Namespace}");
			Console.WriteLine($"   Has Spec: {cluster.Spec != null}");
			
			if (cluster.Spec != null)
			{
				Console.WriteLine($"   ? SPEC DATA FOUND!");
				Console.WriteLine($"   K8s Version: '{cluster.Spec.KubernetesVersion}'");
				Console.WriteLine($"   Talos Version: '{cluster.Spec.TalosVersion}'");
			}
			else
			{
				Console.WriteLine($"   ? No spec data (still null)");
			}
			
			found = true;
			break;
		}

		Console.WriteLine("========================================");
		
		// Assert
		Assert.True(found, "Should have found at least one cluster");
	}
}