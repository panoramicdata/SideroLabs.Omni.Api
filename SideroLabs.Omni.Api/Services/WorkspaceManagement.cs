using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of workspace management operations
/// </summary>
internal class WorkspaceManagement : OmniServiceBase, IWorkspaceManagement
{
	/// <summary>
	/// Initializes a new instance of the WorkspaceManagement class
	/// </summary>
	/// <param name="options">Client options</param>
	/// <param name="channel">gRPC channel</param>
	/// <param name="authenticator">Authentication provider</param>
	public WorkspaceManagement(
		OmniClientOptions options,
		GrpcChannel channel,
		OmniAuthenticator? authenticator)
		: base(options, channel, authenticator)
	{
	}

	/// <inheritdoc />
	public async Task<ListWorkspacesResponse> ListWorkspacesAsync(CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/ListWorkspaces";

		Logger.LogInformation("Listing workspaces...");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new ListWorkspacesResponse
		{
			Workspaces = new List<Workspace>
			{
				new()
				{
					Id = "workspace-1",
					Name = "default-workspace",
					Description = "Default workspace for cluster management",
					Spec = new WorkspaceSpec
					{
						ResourceQuota = new ResourceQuota
						{
							MaxClusters = 10,
							MaxMachines = 100,
							MaxCpuCores = 500,
							MaxMemoryGb = 1000,
							MaxStorageGb = 5000
						},
						Labels = new Dictionary<string, string>
						{
							{ "environment", "production" },
							{ "team", "platform" }
						}
					},
					Status = new WorkspaceStatus
					{
						Phase = "Active",
						Ready = true,
						ClusterCount = 3,
						ResourceUsage = new ResourceUsage
						{
							Clusters = 3,
							Machines = 25,
							CpuCores = 125.5,
							MemoryGb = 256.0,
							StorageGb = 1024.0
						}
					},
					CreatedAt = DateTimeOffset.UtcNow.AddDays(-60).ToUnixTimeSeconds(),
					UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds()
				}
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new workspace")]
	public async Task<CreateWorkspaceResponse> CreateWorkspaceAsync(string name, WorkspaceSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/CreateWorkspace";

		Logger.LogInformation("Creating workspace: {WorkspaceName}", name);

		EnsureWriteActionAllowed("Workspace");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new CreateWorkspaceResponse
		{
			Workspace = new Workspace
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Spec = spec,
				Status = new WorkspaceStatus
				{
					Phase = "Initializing",
					Ready = false,
					ClusterCount = 0,
					ResourceUsage = new ResourceUsage()
				},
				CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	public async Task<GetWorkspaceResponse> GetWorkspaceAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/GetWorkspace";

		Logger.LogInformation("Getting workspace: {WorkspaceId}", id);

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new GetWorkspaceResponse
		{
			Workspace = new Workspace
			{
				Id = id,
				Name = $"workspace-{id}",
				Description = "Sample workspace",
				Status = new WorkspaceStatus
				{
					Phase = "Active",
					Ready = true,
					ClusterCount = 2
				}
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Updates a workspace")]
	public async Task<UpdateWorkspaceResponse> UpdateWorkspaceAsync(string id, WorkspaceSpec spec, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/UpdateWorkspace";

		Logger.LogInformation("Updating workspace: {WorkspaceId}", id);

		EnsureWriteActionAllowed("Workspace");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new UpdateWorkspaceResponse
		{
			Workspace = new Workspace
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Deletes a workspace")]
	public async Task<DeleteWorkspaceResponse> DeleteWorkspaceAsync(string id, CancellationToken cancellationToken)
	{
		const string method = "/omni.management.ManagementService/DeleteWorkspace";

		Logger.LogWarning("Attempting to delete workspace: {WorkspaceId}", id);

		EnsureWriteActionAllowed("Workspace");

		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		Logger.LogDebug("Call options would be created for method: {Method}", method);

		return new DeleteWorkspaceResponse();
	}
}
