namespace SideroLabs.Omni.Api.Enums;

/// <summary>
/// Types of Kubernetes sync operations
/// </summary>
public enum SyncType
{
	/// <summary>
	/// Unknown operation type
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Manifest synchronization
	/// </summary>
	Manifest = 1,

	/// <summary>
	/// Rollout operation
	/// </summary>
	Rollout = 2
}
