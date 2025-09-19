namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Types of log sources
/// </summary>
public enum LogSourceType
{
	/// <summary>
	/// Cluster-wide logs
	/// </summary>
	Cluster,

	/// <summary>
	/// Machine/node logs
	/// </summary>
	Machine,

	/// <summary>
	/// Container logs
	/// </summary>
	Container,

	/// <summary>
	/// Pod logs
	/// </summary>
	Pod,

	/// <summary>
	/// Service logs
	/// </summary>
	Service,

	/// <summary>
	/// System logs
	/// </summary>
	System,

	/// <summary>
	/// Audit logs
	/// </summary>
	Audit,

	/// <summary>
	/// Application logs
	/// </summary>
	Application
}

/// <summary>
/// Log levels for filtering
/// </summary>
public enum LogLevel
{
	/// <summary>
	/// Debug level logs
	/// </summary>
	Debug,

	/// <summary>
	/// Info level logs
	/// </summary>
	Info,

	/// <summary>
	/// Warning level logs
	/// </summary>
	Warning,

	/// <summary>
	/// Error level logs
	/// </summary>
	Error,

	/// <summary>
	/// Fatal level logs
	/// </summary>
	Fatal
}

/// <summary>
/// Log output formats
/// </summary>
public enum LogFormat
{
	/// <summary>
	/// Plain text format
	/// </summary>
	Text,

	/// <summary>
	/// JSON format
	/// </summary>
	Json,

	/// <summary>
	/// Structured format
	/// </summary>
	Structured
}

/// <summary>
/// Represents log stream information in the Omni system
/// </summary>
public class LogStream
{
	/// <summary>
	/// Unique identifier for the log stream
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Source of the logs (cluster, machine, container, etc.)
	/// </summary>
	public LogSource Source { get; set; } = new();

	/// <summary>
	/// Log stream specification
	/// </summary>
	public LogStreamSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the log stream
	/// </summary>
	public LogStreamStatus Status { get; set; } = new();
}

/// <summary>
/// Log source information
/// </summary>
public class LogSource
{
	/// <summary>
	/// Type of log source
	/// </summary>
	public LogSourceType Type { get; set; }

	/// <summary>
	/// Cluster ID (if applicable)
	/// </summary>
	public string? ClusterId { get; set; }

	/// <summary>
	/// Machine ID (if applicable)
	/// </summary>
	public string? MachineId { get; set; }

	/// <summary>
	/// Namespace (if applicable)
	/// </summary>
	public string? Namespace { get; set; }

	/// <summary>
	/// Pod name (if applicable)
	/// </summary>
	public string? PodName { get; set; }

	/// <summary>
	/// Container name (if applicable)
	/// </summary>
	public string? ContainerName { get; set; }

	/// <summary>
	/// Service name (if applicable)
	/// </summary>
	public string? ServiceName { get; set; }
}

/// <summary>
/// Log stream specification
/// </summary>
public class LogStreamSpec
{
	/// <summary>
	/// Start time for log retrieval (Unix timestamp)
	/// </summary>
	public long? StartTime { get; set; }

	/// <summary>
	/// End time for log retrieval (Unix timestamp)
	/// </summary>
	public long? EndTime { get; set; }

	/// <summary>
	/// Maximum number of log lines to retrieve
	/// </summary>
	public int? MaxLines { get; set; }

	/// <summary>
	/// Log level filter
	/// </summary>
	public LogLevel? LogLevel { get; set; }

	/// <summary>
	/// Whether to follow logs (streaming)
	/// </summary>
	public bool Follow { get; set; }

	/// <summary>
	/// Text filters to apply
	/// </summary>
	public List<string> Filters { get; set; } = new();

	/// <summary>
	/// Format for log output
	/// </summary>
	public LogFormat Format { get; set; } = LogFormat.Text;
}

/// <summary>
/// Current status of a log stream
/// </summary>
public class LogStreamStatus
{
	/// <summary>
	/// Whether the log stream is active
	/// </summary>
	public bool Active { get; set; }

	/// <summary>
	/// Number of log lines available
	/// </summary>
	public long LineCount { get; set; }

	/// <summary>
	/// Size of logs in bytes
	/// </summary>
	public long SizeBytes { get; set; }

	/// <summary>
	/// Last log timestamp (Unix timestamp)
	/// </summary>
	public long? LastLogTime { get; set; }

	/// <summary>
	/// Error message if log retrieval failed
	/// </summary>
	public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents a single log entry
/// </summary>
public class LogEntry
{
	/// <summary>
	/// Timestamp of the log entry (Unix timestamp)
	/// </summary>
	public long Timestamp { get; set; }

	/// <summary>
	/// Log level
	/// </summary>
	public string Level { get; set; } = string.Empty;

	/// <summary>
	/// Log message
	/// </summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// Source of the log entry
	/// </summary>
	public string Source { get; set; } = string.Empty;

	/// <summary>
	/// Additional metadata
	/// </summary>
	public Dictionary<string, object> Metadata { get; set; } = new();

	/// <summary>
	/// Labels associated with the log entry
	/// </summary>
	public Dictionary<string, string> Labels { get; set; } = new();
}
