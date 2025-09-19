namespace SideroLabs.Omni.Api.Models.Responses;

#region Workspace Responses

/// <summary>
/// Response containing a list of workspaces
/// </summary>
public class ListWorkspacesResponse
{
	/// <summary>
	/// List of workspaces
	/// </summary>
	public List<Workspace> Workspaces { get; set; } = new();
}

/// <summary>
/// Response containing a created workspace
/// </summary>
public class CreateWorkspaceResponse
{
	/// <summary>
	/// The created workspace
	/// </summary>
	public Workspace Workspace { get; set; } = new();
}

/// <summary>
/// Response containing a specific workspace
/// </summary>
public class GetWorkspaceResponse
{
	/// <summary>
	/// The requested workspace
	/// </summary>
	public Workspace Workspace { get; set; } = new();
}

/// <summary>
/// Response containing an updated workspace
/// </summary>
public class UpdateWorkspaceResponse
{
	/// <summary>
	/// The updated workspace
	/// </summary>
	public Workspace Workspace { get; set; } = new();
}

/// <summary>
/// Response for workspace deletion (empty)
/// </summary>
public class DeleteWorkspaceResponse
{
	// Empty response - deletion successful if no error
}

#endregion

#region Configuration Template Responses

/// <summary>
/// Response containing a list of configuration templates
/// </summary>
public class ListConfigTemplatesResponse
{
	/// <summary>
	/// List of configuration templates
	/// </summary>
	public List<ConfigTemplate> Templates { get; set; } = new();
}

/// <summary>
/// Response containing a created configuration template
/// </summary>
public class CreateConfigTemplateResponse
{
	/// <summary>
	/// The created configuration template
	/// </summary>
	public ConfigTemplate Template { get; set; } = new();
}

/// <summary>
/// Response containing a specific configuration template
/// </summary>
public class GetConfigTemplateResponse
{
	/// <summary>
	/// The requested configuration template
	/// </summary>
	public ConfigTemplate Template { get; set; } = new();
}

/// <summary>
/// Response containing an updated configuration template
/// </summary>
public class UpdateConfigTemplateResponse
{
	/// <summary>
	/// The updated configuration template
	/// </summary>
	public ConfigTemplate Template { get; set; } = new();
}

/// <summary>
/// Response for configuration template deletion
/// </summary>
public class DeleteConfigTemplateResponse
{
	/// <summary>
	/// Success indicator
	/// </summary>
	public bool Success { get; set; } = true;

	/// <summary>
	/// Optional message
	/// </summary>
	public string? Message { get; set; }
}

#endregion

#region Backup Responses

/// <summary>
/// Response containing a list of backups
/// </summary>
public class ListBackupsResponse
{
	/// <summary>
	/// List of backups
	/// </summary>
	public List<Backup> Backups { get; set; } = new();
}

/// <summary>
/// Response containing a created backup
/// </summary>
public class CreateBackupResponse
{
	/// <summary>
	/// The created backup
	/// </summary>
	public Backup Backup { get; set; } = new();
}

/// <summary>
/// Response containing a specific backup
/// </summary>
public class GetBackupResponse
{
	/// <summary>
	/// The requested backup
	/// </summary>
	public Backup Backup { get; set; } = new();
}

/// <summary>
/// Response for backup deletion
/// </summary>
public class DeleteBackupResponse
{
	/// <summary>
	/// Success indicator
	/// </summary>
	public bool Success { get; set; } = true;
}

#endregion

#region Restore Responses

/// <summary>
/// Response containing a list of restore operations
/// </summary>
public class ListRestoreOperationsResponse
{
	/// <summary>
	/// List of restore operations
	/// </summary>
	public List<RestoreOperation> RestoreOperations { get; set; } = new();
}

/// <summary>
/// Response containing a created restore operation
/// </summary>
public class CreateRestoreOperationResponse
{
	/// <summary>
	/// The created restore operation
	/// </summary>
	public RestoreOperation RestoreOperation { get; set; } = new();
}

/// <summary>
/// Response containing a specific restore operation
/// </summary>
public class GetRestoreOperationResponse
{
	/// <summary>
	/// The requested restore operation
	/// </summary>
	public RestoreOperation RestoreOperation { get; set; } = new();
}

/// <summary>
/// Response for canceling a restore operation
/// </summary>
public class CancelRestoreOperationResponse
{
	/// <summary>
	/// Success indicator
	/// </summary>
	public bool Success { get; set; } = true;

	/// <summary>
	/// Optional message
	/// </summary>
	public string? Message { get; set; }
}

#endregion

#region Log Responses

/// <summary>
/// Response containing log streams
/// </summary>
public class GetLogStreamsResponse
{
	/// <summary>
	/// Available log streams
	/// </summary>
	public List<LogStream> LogStreams { get; set; } = new();
}

/// <summary>
/// Response containing log entries
/// </summary>
public class GetLogsResponse
{
	/// <summary>
	/// Log entries
	/// </summary>
	public List<LogEntry> LogEntries { get; set; } = new();

	/// <summary>
	/// Total number of log entries available
	/// </summary>
	public long TotalCount { get; set; }

	/// <summary>
	/// Whether there are more logs available
	/// </summary>
	public bool HasMore { get; set; }

	/// <summary>
	/// Continuation token for pagination
	/// </summary>
	public string? ContinuationToken { get; set; }
}

/// <summary>
/// Response for starting a log stream
/// </summary>
public class StartLogStreamResponse
{
	/// <summary>
	/// Stream ID for the log stream
	/// </summary>
	public string StreamId { get; set; } = string.Empty;

	/// <summary>
	/// WebSocket URL for streaming logs
	/// </summary>
	public string? StreamUrl { get; set; }
}

/// <summary>
/// Response for stopping a log stream
/// </summary>
public class StopLogStreamResponse
{
	/// <summary>
	/// Success indicator
	/// </summary>
	public bool Success { get; set; } = true;

	/// <summary>
	/// Stream ID that was stopped
	/// </summary>
	public string StreamId { get; set; } = string.Empty;
}

#endregion

#region Network Responses

/// <summary>
/// Response containing a list of network configurations
/// </summary>
public class ListNetworkConfigsResponse
{
	/// <summary>
	/// List of network configurations
	/// </summary>
	public List<NetworkConfig> NetworkConfigs { get; set; } = new();
}

/// <summary>
/// Response containing a created network configuration
/// </summary>
public class CreateNetworkConfigResponse
{
	/// <summary>
	/// The created network configuration
	/// </summary>
	public NetworkConfig NetworkConfig { get; set; } = new();
}

/// <summary>
/// Response containing a specific network configuration
/// </summary>
public class GetNetworkConfigResponse
{
	/// <summary>
	/// The requested network configuration
	/// </summary>
	public NetworkConfig NetworkConfig { get; set; } = new();
}

/// <summary>
/// Response containing an updated network configuration
/// </summary>
public class UpdateNetworkConfigResponse
{
	/// <summary>
	/// The updated network configuration
	/// </summary>
	public NetworkConfig NetworkConfig { get; set; } = new();
}

/// <summary>
/// Response for network configuration deletion
/// </summary>
public class DeleteNetworkConfigResponse
{
	/// <summary>
	/// Success indicator
	/// </summary>
	public bool Success { get; set; } = true;
}

#endregion

#region Kubernetes Integration Responses

/// <summary>
/// Response containing Kubernetes configuration
/// </summary>
public class GetKubernetesConfigResponse
{
	/// <summary>
	/// Kubernetes configuration (kubeconfig content)
	/// </summary>
	public string KubeConfig { get; set; } = string.Empty;

	/// <summary>
	/// Expiration time of the configuration (Unix timestamp)
	/// </summary>
	public long? ExpiresAt { get; set; }

	/// <summary>
	/// Server endpoint
	/// </summary>
	public string Server { get; set; } = string.Empty;

	/// <summary>
	/// Cluster CA certificate
	/// </summary>
	public string? ClusterCaCertificate { get; set; }
}

/// <summary>
/// Response containing cluster metrics
/// </summary>
public class GetClusterMetricsResponse
{
	/// <summary>
	/// CPU usage metrics
	/// </summary>
	public MetricData CpuUsage { get; set; } = new();

	/// <summary>
	/// Memory usage metrics
	/// </summary>
	public MetricData MemoryUsage { get; set; } = new();

	/// <summary>
	/// Storage usage metrics
	/// </summary>
	public MetricData StorageUsage { get; set; } = new();

	/// <summary>
	/// Network usage metrics
	/// </summary>
	public MetricData NetworkUsage { get; set; } = new();

	/// <summary>
	/// Pod count metrics
	/// </summary>
	public MetricData PodCount { get; set; } = new();

	/// <summary>
	/// Node count metrics
	/// </summary>
	public MetricData NodeCount { get; set; } = new();
}

/// <summary>
/// Metric data with time series
/// </summary>
public class MetricData
{
	/// <summary>
	/// Metric name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Metric unit
	/// </summary>
	public string Unit { get; set; } = string.Empty;

	/// <summary>
	/// Current value
	/// </summary>
	public double CurrentValue { get; set; }

	/// <summary>
	/// Time series data points
	/// </summary>
	public List<MetricDataPoint> DataPoints { get; set; } = new();
}

/// <summary>
/// Single metric data point
/// </summary>
public class MetricDataPoint
{
	/// <summary>
	/// Timestamp of the data point (Unix timestamp)
	/// </summary>
	public long Timestamp { get; set; }

	/// <summary>
	/// Value at this timestamp
	/// </summary>
	public double Value { get; set; }

	/// <summary>
	/// Additional labels
	/// </summary>
	public Dictionary<string, string> Labels { get; set; } = new();
}

#endregion

#region Enhanced Status Response

/// <summary>
/// Enhanced response containing the status of the Omni service
/// </summary>
public class GetEnhancedStatusResponse
{
	/// <summary>
	/// Version of the Omni service
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// Whether the service is ready
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// Detailed service health information
	/// </summary>
	public ServiceHealth Health { get; set; } = new();

	/// <summary>
	/// System statistics
	/// </summary>
	public SystemStats SystemStats { get; set; } = new();

	/// <summary>
	/// License information
	/// </summary>
	public LicenseInfo? License { get; set; }
}

/// <summary>
/// Service health information
/// </summary>
public class ServiceHealth
{
	/// <summary>
	/// Overall health status
	/// </summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>
	/// Health check results
	/// </summary>
	public Dictionary<string, HealthCheckResult> HealthChecks { get; set; } = new();

	/// <summary>
	/// Uptime in seconds
	/// </summary>
	public long UptimeSeconds { get; set; }

	/// <summary>
	/// Last health check timestamp (Unix timestamp)
	/// </summary>
	public long LastHealthCheck { get; set; }
}

/// <summary>
/// Individual health check result
/// </summary>
public class HealthCheckResult
{
	/// <summary>
	/// Health check status
	/// </summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>
	/// Response time in milliseconds
	/// </summary>
	public double ResponseTimeMs { get; set; }

	/// <summary>
	/// Error message if unhealthy
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Last check timestamp (Unix timestamp)
	/// </summary>
	public long LastCheck { get; set; }
}

/// <summary>
/// System statistics
/// </summary>
public class SystemStats
{
	/// <summary>
	/// Total number of clusters
	/// </summary>
	public int TotalClusters { get; set; }

	/// <summary>
	/// Total number of machines
	/// </summary>
	public int TotalMachines { get; set; }

	/// <summary>
	/// Total number of active workspaces
	/// </summary>
	public int ActiveWorkspaces { get; set; }

	/// <summary>
	/// Total number of backups
	/// </summary>
	public int TotalBackups { get; set; }

	/// <summary>
	/// Total storage used in GB
	/// </summary>
	public double StorageUsedGb { get; set; }

	/// <summary>
	/// Resource utilization
	/// </summary>
	public ResourceUtilization ResourceUtilization { get; set; } = new();
}

/// <summary>
/// Resource utilization statistics
/// </summary>
public class ResourceUtilization
{
	/// <summary>
	/// CPU utilization percentage
	/// </summary>
	public double CpuUtilization { get; set; }

	/// <summary>
	/// Memory utilization percentage
	/// </summary>
	public double MemoryUtilization { get; set; }

	/// <summary>
	/// Storage utilization percentage
	/// </summary>
	public double StorageUtilization { get; set; }

	/// <summary>
	/// Network utilization percentage
	/// </summary>
	public double NetworkUtilization { get; set; }
}

/// <summary>
/// License information
/// </summary>
public class LicenseInfo
{
	/// <summary>
	/// License type
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// License expiration date (Unix timestamp)
	/// </summary>
	public long? ExpiresAt { get; set; }

	/// <summary>
	/// Maximum number of clusters allowed
	/// </summary>
	public int? MaxClusters { get; set; }

	/// <summary>
	/// Maximum number of machines allowed
	/// </summary>
	public int? MaxMachines { get; set; }

	/// <summary>
	/// Licensed features
	/// </summary>
	public List<string> Features { get; set; } = new();

	/// <summary>
	/// Whether the license is valid
	/// </summary>
	public bool IsValid { get; set; }
}

/// <summary>
/// Response containing node metrics
/// </summary>
public class GetNodeMetricsResponse
{
	/// <summary>
	/// Cluster ID
	/// </summary>
	public string ClusterId { get; set; } = string.Empty;

	/// <summary>
	/// Node metrics data
	/// </summary>
	public List<NodeMetrics> NodeMetrics { get; set; } = new();
}

/// <summary>
/// Response containing pod metrics
/// </summary>
public class GetPodMetricsResponse
{
	/// <summary>
	/// Cluster ID
	/// </summary>
	public string ClusterId { get; set; } = string.Empty;

	/// <summary>
	/// Pod metrics data
	/// </summary>
	public List<PodMetrics> PodMetrics { get; set; } = new();
}

/// <summary>
/// Response containing health check information
/// </summary>
public class GetHealthCheckResponse
{
	/// <summary>
	/// Overall health status
	/// </summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>
	/// Individual component health checks
	/// </summary>
	public Dictionary<string, HealthCheckResult> HealthChecks { get; set; } = new();

	/// <summary>
	/// Timestamp of the health check
	/// </summary>
	public long Timestamp { get; set; }
}

/// <summary>
/// Node metrics information
/// </summary>
public class NodeMetrics
{
	/// <summary>
	/// Node ID
	/// </summary>
	public string NodeId { get; set; } = string.Empty;

	/// <summary>
	/// Node name
	/// </summary>
	public string NodeName { get; set; } = string.Empty;

	/// <summary>
	/// CPU usage metrics
	/// </summary>
	public MetricData CpuUsage { get; set; } = new();

	/// <summary>
	/// Memory usage metrics
	/// </summary>
	public MetricData MemoryUsage { get; set; } = new();

	/// <summary>
	/// Disk usage metrics
	/// </summary>
	public MetricData DiskUsage { get; set; } = new();

	/// <summary>
	/// Network usage metrics
	/// </summary>
	public MetricData NetworkUsage { get; set; } = new();
}

/// <summary>
/// Pod metrics information
/// </summary>
public class PodMetrics
{
	/// <summary>
	/// Pod name
	/// </summary>
	public string PodName { get; set; } = string.Empty;

	/// <summary>
	/// Namespace
	/// </summary>
	public string Namespace { get; set; } = string.Empty;

	/// <summary>
	/// Node name where pod is running
	/// </summary>
	public string NodeName { get; set; } = string.Empty;

	/// <summary>
	/// CPU usage metrics
	/// </summary>
	public MetricData CpuUsage { get; set; } = new();

	/// <summary>
	/// Memory usage metrics
	/// </summary>
	public MetricData MemoryUsage { get; set; } = new();

	/// <summary>
	/// Container metrics
	/// </summary>
	public List<ContainerMetrics> ContainerMetrics { get; set; } = new();
}

/// <summary>
/// Container metrics information
/// </summary>
public class ContainerMetrics
{
	/// <summary>
	/// Container name
	/// </summary>
	public string ContainerName { get; set; } = string.Empty;

	/// <summary>
	/// CPU usage metrics
	/// </summary>
	public MetricData CpuUsage { get; set; } = new();

	/// <summary>
	/// Memory usage metrics
	/// </summary>
	public MetricData MemoryUsage { get; set; } = new();
}

#endregion
