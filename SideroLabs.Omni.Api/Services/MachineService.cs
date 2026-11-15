using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Management;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Constants;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Service for machine management operations
/// </summary>
internal class MachineService(ManagementConnection connection) : ManagementServiceBase(connection), IMachineService
{
	public async IAsyncEnumerable<byte[]> StreamLogsAsync(
		string machineId,
		bool follow = false,
		int tailLines = 0,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = new MachineLogsRequest
		{
			MachineId = machineId,
			Follow = follow,
			TailLines = tailLines
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.MachineLogs,
			GrpcMethods.MachineLogs,
			"machine logs streaming");

		await foreach (var logData in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return logData.Bytes.ToByteArray();
		}

		Logger.LogDebug("Machine log streaming completed for {MachineId}", machineId);
	}

	[IsWriteAction(WriteActionType.Update, Description = "Performs a maintenance upgrade on a machine")]
	public async Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("upgrade", "machine");

		Logger.LogInformation("Starting maintenance upgrade for machine {MachineId} to version {Version}",
			machineId, version);

		var request = new MaintenanceUpgradeRequest
		{
			MachineId = machineId,
			Version = version
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.MaintenanceUpgradeAsync,
			GrpcMethods.MaintenanceUpgrade,
			"maintenance upgrade",
			cancellationToken);

		Logger.LogInformation("Maintenance upgrade completed successfully for machine {MachineId}", machineId);
	}

	public async Task<MachineJoinConfig> GetJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Getting machine join configuration (useGrpcTunnel: {UseGrpcTunnel})", useGrpcTunnel);

		var request = new GetMachineJoinConfigRequest
		{
			UseGrpcTunnel = useGrpcTunnel,
			JoinToken = joinToken
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.GetMachineJoinConfigAsync,
			GrpcMethods.GetMachineJoinConfig,
			"machine join config retrieval",
			cancellationToken);

		var config = new MachineJoinConfig
		{
			Config = response.Config ?? "",
			KernelArgs = [.. response.KernelArgs]
		};

		Logger.LogInformation("Machine join config retrieved: {Summary}", config.GetSummary());

		return config;
	}

	[IsWriteAction(WriteActionType.Create, Description = "Creates a join token for machines")]
	public async Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("create", "join token");

		Logger.LogInformation("Creating join token: {Name}, expires: {ExpirationTime}", name, expirationTime);

		var request = new CreateJoinTokenRequest
		{
			Name = name,
			ExpirationTime = Timestamp.FromDateTimeOffset(expirationTime)
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.CreateJoinTokenAsync,
			GrpcMethods.CreateJoinToken,
			"join token creation",
			cancellationToken);

		Logger.LogInformation("Join token created successfully: {TokenId}", response.Id);

		return response.Id;
	}
}
