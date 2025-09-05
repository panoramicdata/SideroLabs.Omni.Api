namespace SideroLabs.Omni.Api.Models.Responses;

// Cluster Response Models

/// <summary>
/// Response containing a list of clusters
/// </summary>
public class ListClustersResponse
{
	/// <summary>
	/// List of clusters
	/// </summary>
	public List<Cluster> Clusters { get; set; } = new();
}

/// <summary>
/// Response containing a created cluster
/// </summary>
public class CreateClusterResponse
{
	/// <summary>
	/// The created cluster
	/// </summary>
	public Cluster Cluster { get; set; } = new();
}

/// <summary>
/// Response containing a specific cluster
/// </summary>
public class GetClusterResponse
{
	/// <summary>
	/// The requested cluster
	/// </summary>
	public Cluster Cluster { get; set; } = new();
}

/// <summary>
/// Response containing an updated cluster
/// </summary>
public class UpdateClusterResponse
{
	/// <summary>
	/// The updated cluster
	/// </summary>
	public Cluster Cluster { get; set; } = new();
}

/// <summary>
/// Response for cluster deletion (empty)
/// </summary>
public class DeleteClusterResponse
{
	// Empty response - deletion successful if no error
}