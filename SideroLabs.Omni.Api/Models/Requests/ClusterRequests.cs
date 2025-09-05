namespace SideroLabs.Omni.Api.Models.Requests;

// Cluster Request Models

/// <summary>
/// Request to list all clusters
/// </summary>
public class ListClustersRequest
{
	// Empty request - no parameters needed
}

/// <summary>
/// Request to create a new cluster
/// </summary>
public class CreateClusterRequest
{
	/// <summary>
	/// Name of the cluster to create
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Specification for the new cluster
	/// </summary>
	public ClusterSpec Spec { get; set; } = new();
}

/// <summary>
/// Request to get a specific cluster
/// </summary>
public class GetClusterRequest
{
	/// <summary>
	/// ID of the cluster to retrieve
	/// </summary>
	public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Request to update a cluster
/// </summary>
public class UpdateClusterRequest
{
	/// <summary>
	/// ID of the cluster to update
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Updated specification for the cluster
	/// </summary>
	public ClusterSpec Spec { get; set; } = new();
}

/// <summary>
/// Request to delete a cluster
/// </summary>
public class DeleteClusterRequest
{
	/// <summary>
	/// ID of the cluster to delete
	/// </summary>
	public string Id { get; set; } = string.Empty;
}