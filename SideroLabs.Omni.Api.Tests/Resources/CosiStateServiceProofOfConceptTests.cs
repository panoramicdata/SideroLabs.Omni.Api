using System.Reflection;
using Cosi.Resource;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Security;
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
	/// <summary>
	/// Integration proof-of-concept test that calls the COSI State /cosi.resource.State/List endpoint directly via raw gRPC to verify accessibility.
	/// </summary>
	[Fact]
	public async Task CosiStateService_ListClusters_Works()
	{
		var channel = ReadPrivateField<GrpcChannel>("_channel");
		Assert.NotNull(channel);

		var headers = new Grpc.Core.Metadata();
		ReadPrivateField<OmniAuthenticator>("_authenticator")?.SignRequest(headers, "/cosi.resource.State/List");
		var callOptions = new CallOptions(headers: headers, deadline: DateTime.UtcNow.AddSeconds(30));
		var request = new ListRequest { Namespace = "default", Type = "Clusters.omni.sidero.dev" };

		using var call = new State.StateClient(channel).List(request, callOptions);

		try
		{
			var clusters = new List<string>();
			await foreach (var response in call.ResponseStream.ReadAllAsync(CancellationToken))
			{
				if (response.Resource != null)
				{
					clusters.Add(response.Resource.Metadata.Id);
				}
			}

			Logger.LogInformation("Total clusters received: {Count}", clusters.Count);
		}
		catch (RpcException ex)
		{
			Logger.LogError("Failed: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
			Assert.NotEqual(StatusCode.Unimplemented, ex.StatusCode);
			throw;
		}
	}

	private T? ReadPrivateField<T>(string name) where T : class =>
		typeof(OmniClient).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(OmniClient) as T;
}
