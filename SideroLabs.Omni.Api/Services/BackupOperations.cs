using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of backup operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the BackupOperations class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
internal class BackupOperations(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : OmniServiceBase(options, channel, authenticator), IBackupOperations
{

	/// <inheritdoc />
	public async Task<ListBackupsResponse> ListBackupsAsync(CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/ListBackups";

		Logger.LogInformation("Listing all backups...");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new ListBackupsResponse
		{
			Backups = new List<Backup>
			{
				new()
				{
					Id = "backup-1",
					Name = "nightly-backup-cluster-1",
					Type = BackupType.Full,
					Spec = new BackupSpec
					{
						ClusterId = "cluster-1",
						Schedule = "0 2 * * *", // Daily at 2 AM
						RetentionPolicy = new BackupRetentionPolicy
						{
							RetentionDays = 30,
							MaxBackupCount = 10,
							AutoDelete = true
						},
						StorageLocation = new BackupStorageLocation
						{
							Type = "s3",
							Bucket = "omni-backups",
							Prefix = "cluster-backups/"
						}
					},
					Status = new BackupStatus
					{
						Phase = "Completed",
						Progress = 100,
						SizeBytes = 1024 * 1024 * 512, // 512 MB
						ItemCount = 1523,
						StartTime = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(),
						EndTime = DateTimeOffset.UtcNow.AddMinutes(-90).ToUnixTimeSeconds()
					},
					CreatedAt = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(),
					CompletedAt = DateTimeOffset.UtcNow.AddMinutes(-90).ToUnixTimeSeconds()
				}
			}
		};
	}

	/// <inheritdoc />
	public async Task<ListBackupsResponse> ListBackupsAsync(string clusterId, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/ListBackups";

		Logger.LogInformation("Listing backups for cluster: {ClusterId}", clusterId);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		// Filter backups for the specific cluster
		var allBackups = await ListBackupsAsync(cancellationToken);
		var filteredBackups = allBackups.Backups.Where(b => b.Spec.ClusterId == clusterId).ToList();

		return new ListBackupsResponse
		{
			Backups = filteredBackups
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new backup")]
	public async Task<CreateBackupResponse> CreateBackupAsync(string name, BackupSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/CreateBackup";

		Logger.LogInformation("Creating backup: {BackupName}", name);

		EnsureWriteActionAllowed("Backup");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new CreateBackupResponse
		{
			Backup = new Backup
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Type = BackupType.Full,
				Spec = spec,
				Status = new BackupStatus
				{
					Phase = "Starting",
					Progress = 0
				},
				CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	public async Task<GetBackupResponse> GetBackupAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/GetBackup";

		Logger.LogInformation("Getting backup: {BackupId}", id);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new GetBackupResponse
		{
			Backup = new Backup
			{
				Id = id,
				Name = $"backup-{id}",
				Type = BackupType.Full,
				Status = new BackupStatus
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
	[IsWriteAction(WriteActionType.Delete, Description = "Deletes a backup")]
	public async Task<DeleteBackupResponse> DeleteBackupAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/DeleteBackup";

		Logger.LogWarning("Deleting backup: {BackupId}", id);

		EnsureWriteActionAllowed("Backup");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new DeleteBackupResponse
		{
			Success = true
		};
	}
}
