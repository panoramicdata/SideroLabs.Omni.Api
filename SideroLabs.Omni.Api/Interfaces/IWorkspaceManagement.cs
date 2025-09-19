using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for workspace management operations
/// </summary>
public interface IWorkspaceManagement
{
	/// <summary>
	/// Lists all workspaces
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListWorkspacesResponse> ListWorkspacesAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new workspace
	/// </summary>
	/// <param name="name">Name of the workspace to create</param>
	/// <param name="spec">Specification for the new workspace</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CreateWorkspaceResponse> CreateWorkspaceAsync(string name, WorkspaceSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a workspace by ID
	/// </summary>
	/// <param name="id">ID of the workspace to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetWorkspaceResponse> GetWorkspaceAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a workspace
	/// </summary>
	/// <param name="id">ID of the workspace to update</param>
	/// <param name="spec">Updated specification for the workspace</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<UpdateWorkspaceResponse> UpdateWorkspaceAsync(string id, WorkspaceSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a workspace
	/// </summary>
	/// <param name="id">ID of the workspace to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<DeleteWorkspaceResponse> DeleteWorkspaceAsync(string id, CancellationToken cancellationToken);
}
