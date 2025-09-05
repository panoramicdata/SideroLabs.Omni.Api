namespace SideroLabs.Omni.Api.Models.Requests;

// Machine Request Models

/// <summary>
/// Request to list machines in a cluster
/// </summary>
public class ListMachinesRequest
{
	/// <summary>
	/// ID of the cluster to list machines for
	/// </summary>
	public string ClusterId { get; set; } = string.Empty;
}

/// <summary>
/// Request to get a specific machine
/// </summary>
public class GetMachineRequest
{
	/// <summary>
	/// ID of the machine to retrieve
	/// </summary>
	public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Request to update a machine
/// </summary>
public class UpdateMachineRequest
{
	/// <summary>
	/// ID of the machine to update
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Updated specification for the machine
	/// </summary>
	public MachineSpec Spec { get; set; } = new();
}

/// <summary>
/// Request to delete a machine
/// </summary>
public class DeleteMachineRequest
{
	/// <summary>
	/// ID of the machine to delete
	/// </summary>
	public string Id { get; set; } = string.Empty;
}

// Status Request Models

/// <summary>
/// Request to get the status of the Omni service
/// </summary>
public class GetStatusRequest
{
	// Empty request - no parameters needed
}