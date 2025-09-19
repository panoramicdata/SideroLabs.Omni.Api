using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of restore operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the RestoreOperations class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
internal class RestoreOperations(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : OmniServiceBase(options, channel, authenticator), IRestoreOperations
{

	/// <inheritdoc />
	public async Task<ListRestoreOperationsResponse> ListRestoreOperationsAsync(CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/ListRestoreOperations";

		Logger.LogInformation("Listing restore operations...");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new ListRestoreOperationsResponse
		{
			RestoreOperations = new List<RestoreOperation>
			{
				new()
				{
					Id = "restore-1",
					Name = "restore-from-backup-1",
					BackupId = "backup-1",
					Spec = new RestoreSpec
					{
						TargetClusterId = "cluster-2",
						RestoreClusterState = true,
						RestorePersistentVolumes = true
					},
					Status = new RestoreStatus
					{
						Phase = "Completed",
						Progress = 100,
						RestoredItemCount = 1520,
						FailedItemCount = 3,
						StartTime = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds(),
						EndTime = DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeSeconds()
					},
					CreatedAt = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds(),
					CompletedAt = DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeSeconds()
				}
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new restore operation")]
	public async Task<CreateRestoreOperationResponse> CreateRestoreOperationAsync(string name, RestoreSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/CreateRestoreOperation";

		Logger.LogInformation("Creating restore operation: {RestoreName}", name);

		EnsureWriteActionAllowed("Restore operation");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new CreateRestoreOperationResponse
		{
			RestoreOperation = new RestoreOperation
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Spec = spec,
				Status = new RestoreStatus
				{
					Phase = "Starting",
					Progress = 0
				},
				CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	public async Task<GetRestoreOperationResponse> GetRestoreOperationAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/GetRestoreOperation";

		Logger.LogInformation("Getting restore operation: {RestoreId}", id);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new GetRestoreOperationResponse
		{
			RestoreOperation = new RestoreOperation
			{
				Id = id,
				Name = $"restore-{id}",
				Status = new RestoreStatus
				{
					Phase = "Completed",
					Progress = 100
				},
				CreatedAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
				CompletedAt = DateTimeOffset.UtcNow.AddHours(-22).ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Stop, Description = "Cancels a restore operation")]
	public async Task<CancelRestoreOperationResponse> CancelRestoreOperationAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/CancelRestoreOperation";

		Logger.LogWarning("Canceling restore operation: {RestoreId}", id);

		EnsureWriteActionAllowed("Restore operation");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new CancelRestoreOperationResponse
		{
			Success = true
		};
	}
}
