namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a machine in the Omni system
/// </summary>
public class Machine
{
	/// <summary>
	/// Unique identifier for the machine
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the machine
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// ID of the cluster this machine belongs to
	/// </summary>
	public string ClusterId { get; set; } = string.Empty;

	/// <summary>
	/// Machine specification
	/// </summary>
	public MachineSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the machine
	/// </summary>
	public MachineStatus Status { get; set; } = new();

	/// <summary>
	/// Timestamp when the machine was created (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the machine was last updated (Unix timestamp)
	/// </summary>
	public long UpdatedAt { get; set; }
}

/// <summary>
/// Machine specification defining desired state
/// </summary>
public class MachineSpec
{
	/// <summary>
	/// Role of the machine in the cluster (e.g., "controlplane", "worker")
	/// </summary>
	public string Role { get; set; } = string.Empty;

	/// <summary>
	/// Custom labels assigned to the machine
	/// </summary>
	public Dictionary<string, string> Labels { get; set; } = [];
}

/// <summary>
/// Current status of a machine
/// </summary>
public class MachineStatus
{
	/// <summary>
	/// Current phase of the machine lifecycle
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Whether the machine is ready for use
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// IP address or hostname of the machine
	/// </summary>
	public string Address { get; set; } = string.Empty;
}