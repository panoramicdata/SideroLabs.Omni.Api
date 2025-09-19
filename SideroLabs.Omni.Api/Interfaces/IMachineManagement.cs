using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for machine management operations
/// </summary>
public interface IMachineManagement
{
	/// <summary>
	/// Lists machines in a cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster to list machines for</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListMachinesResponse> ListMachinesAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a machine by ID
	/// </summary>
	/// <param name="id">ID of the machine to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetMachineResponse> GetMachineAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a machine
	/// </summary>
	/// <param name="id">ID of the machine to update</param>
	/// <param name="spec">Updated specification for the machine</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<UpdateMachineResponse> UpdateMachineAsync(string id, MachineSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a machine
	/// </summary>
	/// <param name="id">ID of the machine to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<DeleteMachineResponse> DeleteMachineAsync(string id, CancellationToken cancellationToken);
}
