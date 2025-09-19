using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for network configuration management
/// </summary>
public interface INetworkManagement
{
	/// <summary>
	/// Lists network configurations
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListNetworkConfigsResponse> ListNetworkConfigsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets a network configuration by ID
	/// </summary>
	/// <param name="id">ID of the network configuration to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetNetworkConfigResponse> GetNetworkConfigAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new network configuration
	/// </summary>
	/// <param name="name">Name of the network configuration</param>
	/// <param name="spec">Network configuration specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CreateNetworkConfigResponse> CreateNetworkConfigAsync(string name, NetworkConfigSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a network configuration
	/// </summary>
	/// <param name="id">ID of the network configuration to update</param>
	/// <param name="spec">Updated network configuration specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<UpdateNetworkConfigResponse> UpdateNetworkConfigAsync(string id, NetworkConfigSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a network configuration
	/// </summary>
	/// <param name="id">ID of the network configuration to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<DeleteNetworkConfigResponse> DeleteNetworkConfigAsync(string id, CancellationToken cancellationToken);
}
