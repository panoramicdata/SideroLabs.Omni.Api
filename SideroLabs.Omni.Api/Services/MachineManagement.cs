using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of machine management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the MachineManagement class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
internal class MachineManagement(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : OmniServiceBase(options, channel, authenticator), IMachineManagement
{

	/// <inheritdoc />
	public async Task<ListMachinesResponse> ListMachinesAsync(string clusterId, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/ListMachines";

		Logger.LogInformation("Listing machines for cluster: {ClusterId}", clusterId);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new ListMachinesResponse
		{
			Machines =
			[
				new()
				{
					Id = "machine-1",
					Name = "control-plane-1",
					ClusterId = clusterId,
					Spec = new MachineSpec
					{
						Role = "controlplane",
						Labels = new Dictionary<string, string>
						{
							{ "node-role.kubernetes.io/control-plane", "" },
							{ "environment", "production" }
						}
					},
					Status = new MachineStatus
					{
						Phase = "Running",
						Ready = true,
						Address = "10.0.1.10"
					},
					CreatedAt = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
					UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
				}
			]
		};
	}

	/// <inheritdoc />
	public async Task<GetMachineResponse> GetMachineAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/GetMachine";

		Logger.LogInformation("Getting machine: {MachineId}", id);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new GetMachineResponse
		{
			Machine = new Machine
			{
				Id = id,
				Name = $"machine-{id}",
				Spec = new MachineSpec { Role = "worker" },
				Status = new MachineStatus
				{
					Phase = "Running",
					Ready = true,
					Address = "10.0.1.20"
				}
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Updates a machine")]
	public async Task<UpdateMachineResponse> UpdateMachineAsync(string id, MachineSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/UpdateMachine";

		Logger.LogInformation("Updating machine: {MachineId}", id);

		EnsureWriteActionAllowed("Machine");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new UpdateMachineResponse
		{
			Machine = new Machine
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Deletes a machine")]
	public async Task<DeleteMachineResponse> DeleteMachineAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/DeleteMachine";

		Logger.LogWarning("Attempting to delete machine: {MachineId}", id);

		EnsureWriteActionAllowed("Machine");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new DeleteMachineResponse();
	}
}
