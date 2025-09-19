using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for Kubernetes integration operations
/// </summary>
public interface IKubernetesIntegration
{
	/// <summary>
	/// Gets Kubernetes configuration (kubeconfig) for a cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetKubernetesConfigResponse> GetKubernetesConfigAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets cluster metrics for all time
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetClusterMetricsResponse> GetClusterMetricsAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets cluster metrics for a specific time range
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="startTime">Start time for metrics (Unix timestamp)</param>
	/// <param name="endTime">End time for metrics (Unix timestamp)</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetClusterMetricsResponse> GetClusterMetricsAsync(string clusterId, long startTime, long endTime, CancellationToken cancellationToken);

	/// <summary>
	/// Gets node metrics for all nodes in a cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetNodeMetricsResponse> GetNodeMetricsAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets node metrics for a specific node
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="nodeId">Specific node ID</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetNodeMetricsResponse> GetNodeMetricsAsync(string clusterId, string nodeId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets pod metrics for all namespaces in a cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetPodMetricsResponse> GetPodMetricsAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets pod metrics for a specific namespace
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="namespace">Namespace filter</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetPodMetricsResponse> GetPodMetricsAsync(string clusterId, string @namespace, CancellationToken cancellationToken);
}
