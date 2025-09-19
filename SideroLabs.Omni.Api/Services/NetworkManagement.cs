using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of network management operations
/// </summary>
internal class NetworkManagement : OmniServiceBase, INetworkManagement
{
	/// <summary>
	/// Initializes a new instance of the NetworkManagement class
	/// </summary>
	/// <param name="options">Client options</param>
	/// <param name="channel">gRPC channel</param>
	/// <param name="authenticator">Authentication provider</param>
	public NetworkManagement(
		OmniClientOptions options,
		GrpcChannel channel,
		OmniAuthenticator? authenticator)
		: base(options, channel, authenticator)
	{
	}

	/// <inheritdoc />
	public async Task<ListNetworkConfigsResponse> ListNetworkConfigsAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation("Listing network configurations...");
		await Task.Delay(10, cancellationToken);

		return new ListNetworkConfigsResponse
		{
			NetworkConfigs = new List<NetworkConfig>
			{
				new()
				{
					Id = "network-config-1",
					Name = "default-network-config",
					Description = "Default network configuration",
					Status = new NetworkConfigStatus { Phase = "Applied", Ready = true }
				}
			}
		};
	}

	/// <inheritdoc />
	public async Task<GetNetworkConfigResponse> GetNetworkConfigAsync(string id, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Getting network configuration: {Id}", id);
		await Task.Delay(10, cancellationToken);

		return new GetNetworkConfigResponse
		{
			NetworkConfig = new NetworkConfig
			{
				Id = id,
				Name = $"network-config-{id}",
				Status = new NetworkConfigStatus { Phase = "Applied", Ready = true }
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new network configuration")]
	public async Task<CreateNetworkConfigResponse> CreateNetworkConfigAsync(string name, NetworkConfigSpec spec, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Creating network configuration: {Name}", name);
		EnsureWriteActionAllowed("Network configuration");
		await Task.Delay(10, cancellationToken);

		return new CreateNetworkConfigResponse
		{
			NetworkConfig = new NetworkConfig
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Spec = spec
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Updates a network configuration")]
	public async Task<UpdateNetworkConfigResponse> UpdateNetworkConfigAsync(string id, NetworkConfigSpec spec, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Updating network configuration: {Id}", id);
		EnsureWriteActionAllowed("Network configuration");
		await Task.Delay(10, cancellationToken);

		return new UpdateNetworkConfigResponse
		{
			NetworkConfig = new NetworkConfig
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Deletes a network configuration")]
	public async Task<DeleteNetworkConfigResponse> DeleteNetworkConfigAsync(string id, CancellationToken cancellationToken)
	{
		Logger.LogWarning("Deleting network configuration: {Id}", id);
		EnsureWriteActionAllowed("Network configuration");
		await Task.Delay(10, cancellationToken);

		return new DeleteNetworkConfigResponse { Success = true };
	}
}
