namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a backup in the Omni system
/// </summary>
public class Backup
{
	/// <summary>
	/// Unique identifier for the backup
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the backup
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Type of backup
	/// </summary>
	public BackupType Type { get; set; }

	/// <summary>
	/// Backup specification
	/// </summary>
	public BackupSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the backup
	/// </summary>
	public BackupStatus Status { get; set; } = new();

	/// <summary>
	/// Timestamp when the backup was created (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the backup was completed (Unix timestamp)
	/// </summary>
	public long? CompletedAt { get; set; }
}

/// <summary>
/// Current status of a backup
/// </summary>
public class BackupStatus
{
	/// <summary>
	/// Current phase of the backup
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Progress percentage (0-100)
	/// </summary>
	public int Progress { get; set; }

	/// <summary>
	/// Size of the backup in bytes
	/// </summary>
	public long? SizeBytes { get; set; }

	/// <summary>
	/// Number of items backed up
	/// </summary>
	public int ItemCount { get; set; }

	/// <summary>
	/// Error message if backup failed
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Backup start time (Unix timestamp)
	/// </summary>
	public long? StartTime { get; set; }

	/// <summary>
	/// Backup end time (Unix timestamp)
	/// </summary>
	public long? EndTime { get; set; }
}

/// <summary>
/// Represents a restore operation in the Omni system
/// </summary>
public class RestoreOperation
{
	/// <summary>
	/// Unique identifier for the restore operation
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the restore operation
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Source backup ID
	/// </summary>
	public string BackupId { get; set; } = string.Empty;

	/// <summary>
	/// Restore specification
	/// </summary>
	public RestoreSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the restore operation
	/// </summary>
	public RestoreStatus Status { get; set; } = new();

	/// <summary>
	/// Timestamp when the restore was started (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the restore was completed (Unix timestamp)
	/// </summary>
	public long? CompletedAt { get; set; }
}

/// <summary>
/// Restore operation specification
/// </summary>
public class RestoreSpec
{
	/// <summary>
	/// Target cluster ID for the restore
	/// </summary>
	public string TargetClusterId { get; set; } = string.Empty;

	/// <summary>
	/// Resources to include in the restore
	/// </summary>
	public List<string> IncludedResources { get; set; } = new();

	/// <summary>
	/// Resources to exclude from the restore
	/// </summary>
	public List<string> ExcludedResources { get; set; } = new();

	/// <summary>
	/// Namespace mappings (source -> target)
	/// </summary>
	public Dictionary<string, string> NamespaceMappings { get; set; } = new();

	/// <summary>
	/// Whether to restore cluster state
	/// </summary>
	public bool RestoreClusterState { get; set; } = true;

	/// <summary>
	/// Whether to restore PVs and PVCs
	/// </summary>
	public bool RestorePersistentVolumes { get; set; } = true;
}

/// <summary>
/// Current status of a restore operation
/// </summary>
public class RestoreStatus
{
	/// <summary>
	/// Current phase of the restore
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Progress percentage (0-100)
	/// </summary>
	public int Progress { get; set; }

	/// <summary>
	/// Number of items restored
	/// </summary>
	public int RestoredItemCount { get; set; }

	/// <summary>
	/// Number of items failed to restore
	/// </summary>
	public int FailedItemCount { get; set; }

	/// <summary>
	/// Error message if restore failed
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Restore start time (Unix timestamp)
	/// </summary>
	public long? StartTime { get; set; }

	/// <summary>
	/// Restore end time (Unix timestamp)
	/// </summary>
	public long? EndTime { get; set; }

	/// <summary>
	/// List of warnings during restore
	/// </summary>
	public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Types of backups
/// </summary>
public enum BackupType
{
	/// <summary>
	/// Full cluster backup including all resources
	/// </summary>
	Full,

	/// <summary>
	/// Configuration backup only
	/// </summary>
	Configuration,

	/// <summary>
	/// Application data backup
	/// </summary>
	ApplicationData,

	/// <summary>
	/// etcd backup
	/// </summary>
	Etcd,

	/// <summary>
	/// Machine image backup
	/// </summary>
	MachineImage
}

/// <summary>
/// Backup specification
/// </summary>
public class BackupSpec
{
	/// <summary>
	/// Source cluster ID for the backup
	/// </summary>
	public string ClusterId { get; set; } = string.Empty;

	/// <summary>
	/// Backup schedule (cron expression)
	/// </summary>
	public string? Schedule { get; set; }

	/// <summary>
	/// Retention policy for backups
	/// </summary>
	public BackupRetentionPolicy RetentionPolicy { get; set; } = new();

	/// <summary>
	/// Storage location for the backup
	/// </summary>
	public BackupStorageLocation StorageLocation { get; set; } = new();

	/// <summary>
	/// Resources to include in the backup
	/// </summary>
	public List<string> IncludedResources { get; set; } = new();

	/// <summary>
	/// Resources to exclude from the backup
	/// </summary>
	public List<string> ExcludedResources { get; set; } = new();

	/// <summary>
	/// Namespaces to include (empty = all namespaces)
	/// </summary>
	public List<string> IncludedNamespaces { get; set; } = new();

	/// <summary>
	/// Namespaces to exclude
	/// </summary>
	public List<string> ExcludedNamespaces { get; set; } = new();
}

/// <summary>
/// Backup retention policy
/// </summary>
public class BackupRetentionPolicy
{
	/// <summary>
	/// Number of days to retain backups
	/// </summary>
	public int RetentionDays { get; set; } = 30;

	/// <summary>
	/// Maximum number of backups to keep
	/// </summary>
	public int? MaxBackupCount { get; set; }

	/// <summary>
	/// Delete backups after retention period
	/// </summary>
	public bool AutoDelete { get; set; } = true;
}

/// <summary>
/// Backup storage location
/// </summary>
public class BackupStorageLocation
{
	/// <summary>
	/// Storage type (s3, gcs, azure, local, etc.)
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// Storage bucket or container name
	/// </summary>
	public string Bucket { get; set; } = string.Empty;

	/// <summary>
	/// Storage path prefix
	/// </summary>
	public string Prefix { get; set; } = string.Empty;

	/// <summary>
	/// Storage region
	/// </summary>
	public string? Region { get; set; }

	/// <summary>
	/// Storage configuration parameters
	/// </summary>
	public Dictionary<string, string> Config { get; set; } = new();
}
