namespace SideroLabs.Omni.Api.Models.Responses;

// Machine Response Models

/// <summary>
/// Response containing a list of machines
/// </summary>
public class ListMachinesResponse
{
	/// <summary>
	/// List of machines
	/// </summary>
	public List<Machine> Machines { get; set; } = [];
}

/// <summary>
/// Response containing a specific machine
/// </summary>
public class GetMachineResponse
{
	/// <summary>
	/// The requested machine
	/// </summary>
	public Machine Machine { get; set; } = new();
}

/// <summary>
/// Response containing an updated machine
/// </summary>
public class UpdateMachineResponse
{
	/// <summary>
	/// The updated machine
	/// </summary>
	public Machine Machine { get; set; } = new();
}

/// <summary>
/// Response for machine deletion (empty)
/// </summary>
public class DeleteMachineResponse
{
	// Empty response - deletion successful if no error
}

// Status Response Models

/// <summary>
/// Response containing the status of the Omni service
/// </summary>
public class GetStatusResponse
{
	/// <summary>
	/// Version of the Omni service
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// Whether the service is ready
	/// </summary>
	public bool Ready { get; set; }
}