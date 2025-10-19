using Cosi.Resource;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// PROOF OF CONCEPT: Test the COSI v1alpha1 State service
/// This test verifies that we can call /cosi.resource.State/* instead of /omni.resources.ResourceService/*
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "COSI")]
[Trait("Category", "ProofOfConcept")]
public class CosiStateServiceProofOfConceptTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task CosiStateService_ListClusters_Works()
	{
		// Arrange
		Logger.LogInformation("?? PROOF OF CONCEPT: Testing COSI State service");
		Logger.LogInformation("Service: /cosi.resource.State/List");
		Logger.LogInformation("Resource Type: Clusters.omni.sidero.dev");

		// Get the gRPC channel from our OmniClient
		var channelField = typeof(OmniClient).GetField("_channel", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var channel = channelField?.GetValue(OmniClient) as Grpc.Net.Client.GrpcChannel;
		
		Assert.NotNull(channel);

		var stateClient = new State.StateClient(channel);

		var request = new ListRequest
		{
			Namespace = "default",
			Type = "Clusters.omni.sidero.dev"
		};

		// Create call options with authentication
		var headers = new Grpc.Core.Metadata();
		
		// Get the authenticator
		var authField = typeof(OmniClient).GetField("_authenticator",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var authenticator = authField?.GetValue(OmniClient) as SideroLabs.Omni.Api.Security.OmniAuthenticator;

		authenticator?.SignRequest(headers, "/cosi.resource.State/List");

		var callOptions = new Grpc.Core.CallOptions(headers: headers, deadline: DateTime.UtcNow.AddSeconds(30));

		// Act
		Logger.LogInformation("?? Calling /cosi.resource.State/List...");
		
		using var call = stateClient.List(request, callOptions);
		var clusters = new List<string>();

		try
		{
			await foreach (var response in call.ResponseStream.ReadAllAsync(CancellationToken))
			{
				if (response.Resource != null)
				{
					clusters.Add(response.Resource.Metadata.Id);
					Logger.LogInformation("  ? Received cluster: {ClusterId}", response.Resource.Metadata.Id);
					Logger.LogInformation("     Version: {Version}", response.Resource.Metadata.Version);
					Logger.LogInformation("     Namespace: {Namespace}", response.Resource.Metadata.Namespace);
				}
			}

			// Assert
			Logger.LogInformation("?? SUCCESS! COSI State service works!");
			Logger.LogInformation("?? Total clusters received: {Count}", clusters.Count);
			
			// This test proves the concept - even if there are no clusters, getting a successful response
			// (not HTTP 405) proves the endpoint works!
			Assert.True(true, "COSI State service is accessible!");
		}
		catch (Grpc.Core.RpcException ex)
		{
			Logger.LogError("? FAILED: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
			Logger.LogError("Details: {Detail}", ex.Status.Detail);
			
			// If we get Unimplemented, the service is blocked
			Assert.NotEqual(Grpc.Core.StatusCode.Unimplemented, ex.StatusCode);
			
			// Re-throw to see full details
			throw;
		}
	}
}
