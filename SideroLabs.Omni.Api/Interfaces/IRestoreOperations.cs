using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for restore operations
/// </summary>
public interface IRestoreOperations
{
	/// <summary>
	/// Lists restore operations
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListRestoreOperationsResponse> ListRestoreOperationsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new restore operation
	/// </summary>
	/// <param name="name">Name of the restore operation</param>
	/// <param name="spec">Restore specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CreateRestoreOperationResponse> CreateRestoreOperationAsync(string name, RestoreSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a restore operation by ID
	/// </summary>
	/// <param name="id">ID of the restore operation to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetRestoreOperationResponse> GetRestoreOperationAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Cancels a restore operation
	/// </summary>
	/// <param name="id">ID of the restore operation to cancel</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CancelRestoreOperationResponse> CancelRestoreOperationAsync(string id, CancellationToken cancellationToken);
}
