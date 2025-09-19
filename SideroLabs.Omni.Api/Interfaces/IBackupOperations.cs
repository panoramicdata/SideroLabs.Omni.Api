using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for backup operations
/// </summary>
public interface IBackupOperations
{
	/// <summary>
	/// Lists all backups
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListBackupsResponse> ListBackupsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Lists backups for a specific cluster
	/// </summary>
	/// <param name="clusterId">Cluster ID to filter backups</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListBackupsResponse> ListBackupsAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new backup
	/// </summary>
	/// <param name="name">Name of the backup</param>
	/// <param name="spec">Backup specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CreateBackupResponse> CreateBackupAsync(string name, BackupSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a backup by ID
	/// </summary>
	/// <param name="id">ID of the backup to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetBackupResponse> GetBackupAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a backup
	/// </summary>
	/// <param name="id">ID of the backup to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<DeleteBackupResponse> DeleteBackupAsync(string id, CancellationToken cancellationToken);
}
