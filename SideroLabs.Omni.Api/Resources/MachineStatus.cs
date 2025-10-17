namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Status information for a Machine resource
/// </summary>
public class MachineStatus
{
	/// <summary>
	/// Whether the machine is locked to a specific cluster
	/// </summary>
	public bool Locked { get; set; }

	/// <summary>
	/// Current state of the machine
	/// </summary>
	public string State { get; set; } = string.Empty;

	/// <summary>
	/// Kubernetes node name if the machine is part of a cluster
	/// </summary>
	public string? NodeName { get; set; }

	/// <summary>
	/// IP address of the machine
	/// </summary>
	public string? IpAddress { get; set; }

	/// <summary>
	/// Hardware information
	/// </summary>
	public MachineHardware? Hardware { get; set; }

	/// <summary>
	/// Network interfaces
	/// </summary>
	public List<NetworkInterface>? NetworkInterfaces { get; set; }

	/// <summary>
	/// Status conditions
	/// </summary>
	public List<Condition>? Conditions { get; set; }

	/// <summary>
	/// Last seen timestamp
	/// </summary>
	public DateTime? LastSeen { get; set; }
}
