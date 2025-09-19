using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for cluster management operations
/// </summary>
public interface IClusterManagement
{
	/// <summary>
	/// Lists all clusters
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListClustersResponse> ListClustersAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new cluster
	/// </summary>
	/// <param name="name">Name of the cluster to create</param>
	/// <param name="spec">Specification for the new cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CreateClusterResponse> CreateClusterAsync(string name, ClusterSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a cluster by ID
	/// </summary>
	/// <param name="id">ID of the cluster to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetClusterResponse> GetClusterAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a cluster
	/// </summary>
	/// <param name="id">ID of the cluster to update</param>
	/// <param name="spec">Updated specification for the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<UpdateClusterResponse> UpdateClusterAsync(string id, ClusterSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a cluster
	/// </summary>
	/// <param name="id">ID of the cluster to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<DeleteClusterResponse> DeleteClusterAsync(string id, CancellationToken cancellationToken);
}
