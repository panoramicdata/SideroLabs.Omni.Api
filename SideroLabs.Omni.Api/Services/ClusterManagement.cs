using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of cluster management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the ClusterManagement class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
internal class ClusterManagement(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : OmniServiceBase(options, channel, authenticator), IClusterManagement
{

	/// <inheritdoc />
	public async Task<ListClustersResponse> ListClustersAsync(CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/ListClusters";

		Logger.LogInformation("Listing clusters...");

		// For now, return placeholder data to avoid production calls during development
		// TODO: Replace with actual gRPC call once proto-generated client is available
		await Task.Delay(10, cancellationToken);

		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new ListClustersResponse
		{
			Clusters =
			[
				new()
				{
					Id = "cluster-1",
					Name = "production-cluster",
					Spec = new ClusterSpec
					{
						KubernetesVersion = "v1.28.0",
						TalosVersion = "v1.5.0",
						Features = ["embedded-discovery-service"]
					},
					Status = new ClusterStatus
					{
						Phase = "Running",
						Ready = true,
						Conditions = ["Healthy"]
					},
					CreatedAt = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
					UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
				}
			]
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new cluster")]
	public async Task<CreateClusterResponse> CreateClusterAsync(string name, ClusterSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/CreateCluster";

		Logger.LogInformation("Creating cluster: {ClusterName}", name);

		// Check if write operations are allowed
		EnsureWriteOperationAllowed("create", "Cluster");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new CreateClusterResponse
		{
			Cluster = new Cluster
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Spec = spec,
				Status = new ClusterStatus
				{
					Phase = "Provisioning",
					Ready = false,
					Conditions = ["Creating"]
				},
				CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	public async Task<GetClusterResponse> GetClusterAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/GetCluster";

		Logger.LogInformation("Getting cluster: {ClusterId}", id);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new GetClusterResponse
		{
			Cluster = new Cluster
			{
				Id = id,
				Name = $"cluster-{id}",
				Spec = new ClusterSpec
				{
					KubernetesVersion = "v1.28.0",
					TalosVersion = "v1.5.0"
				},
				Status = new ClusterStatus
				{
					Phase = "Running",
					Ready = true
				}
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Updates an existing cluster")]
	public async Task<UpdateClusterResponse> UpdateClusterAsync(string id, ClusterSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/UpdateCluster";

		Logger.LogInformation("Updating cluster: {ClusterId}", id);

		// Check if write operations are allowed
		EnsureWriteOperationAllowed("update", "Cluster");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new UpdateClusterResponse
		{
			Cluster = new Cluster
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Deletes an existing cluster")]
	public async Task<DeleteClusterResponse> DeleteClusterAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/DeleteCluster";

		Logger.LogWarning("Attempting to delete cluster: {ClusterId}", id);

		// Check if write operations are allowed
		EnsureWriteOperationAllowed("delete", "Cluster");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new DeleteClusterResponse();
	}
}
