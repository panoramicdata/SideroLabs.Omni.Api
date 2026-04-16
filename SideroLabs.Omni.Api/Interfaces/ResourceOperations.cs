using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Defines namespaced read and watch operations for Omni COSI resources.
/// </summary>
/// <typeparam name="TResource">The concrete Omni resource type.</typeparam>
public interface INamespacedResourceReadOperations<TResource>
	where TResource : IOmniResource
{
	/// <summary>
	/// Lists resources in a namespace.
	/// </summary>
	/// <param name="namespace">The COSI namespace to query. Defaults to <c>default</c>.</param>
	/// <param name="cancellationToken">A token that can cancel the asynchronous stream.</param>
	/// <returns>An asynchronous stream of matching resources.</returns>
	IAsyncEnumerable<TResource> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a resource by identifier from a namespace.
	/// </summary>
	/// <param name="id">The resource identifier.</param>
	/// <param name="namespace">The COSI namespace to query. Defaults to <c>default</c>.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>The resolved resource.</returns>
	Task<TResource> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);

	/// <summary>
	/// Watches resources for add, update, and delete events.
	/// </summary>
	/// <param name="namespace">The COSI namespace to watch. Defaults to <c>default</c>.</param>
	/// <param name="id">Optional identifier filter for a single resource.</param>
	/// <param name="cancellationToken">A token that can cancel the asynchronous stream.</param>
	/// <returns>An asynchronous stream of resource events.</returns>
	IAsyncEnumerable<ResourceEvent<TResource>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines create, update, and delete operations for Omni COSI resources.
/// </summary>
/// <typeparam name="TResource">The concrete Omni resource type.</typeparam>
public interface INamespacedResourceWriteOperations<TResource>
	where TResource : IOmniResource
{
	/// <summary>
	/// Creates a new resource.
	/// </summary>
	/// <param name="resource">The resource payload to create.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>The created resource as returned by Omni.</returns>
	Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing resource.
	/// </summary>
	/// <param name="resource">The resource payload containing desired state.</param>
	/// <param name="currentVersion">Optional optimistic-lock version from metadata.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>The updated resource.</returns>
	Task<TResource> UpdateAsync(TResource resource, string? currentVersion = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a resource by identifier.
	/// </summary>
	/// <param name="id">The resource identifier.</param>
	/// <param name="namespace">The COSI namespace containing the resource. Defaults to <c>default</c>.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines apply operations for Omni COSI resources.
/// </summary>
/// <typeparam name="TResource">The concrete Omni resource type.</typeparam>
public interface INamespacedResourceApplyOperations<TResource>
	where TResource : IOmniResource
{
	/// <summary>
	/// Applies desired resource state using Omni apply semantics.
	/// </summary>
	/// <param name="resource">The desired resource state.</param>
	/// <param name="dryRun"><see langword="true"/> to validate without persisting.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>The applied resource.</returns>
	Task<TResource> ApplyAsync(TResource resource, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines full CRUD, apply, and watch operations for a namespaced Omni COSI resource.
/// </summary>
/// <typeparam name="TResource">The concrete Omni resource type.</typeparam>
public interface INamespacedResourceOperations<TResource> :
	INamespacedResourceReadOperations<TResource>,
	INamespacedResourceWriteOperations<TResource>,
	INamespacedResourceApplyOperations<TResource>
	where TResource : IOmniResource
{
}

/// <summary>
/// Operations for <see cref="MachineSet"/> resources.
/// </summary>
public interface IMachineSetOperations : INamespacedResourceOperations<MachineSet>
{
}

/// <summary>
/// Operations for <see cref="MachineSetNode"/> resources.
/// </summary>
public interface IMachineSetNodeOperations : INamespacedResourceOperations<MachineSetNode>
{
}

/// <summary>
/// Operations for <see cref="ConfigPatch"/> resources.
/// </summary>
public interface IConfigPatchOperations : INamespacedResourceOperations<ConfigPatch>
{
}

/// <summary>
/// Operations for <see cref="ExtensionsConfiguration"/> resources.
/// </summary>
public interface IExtensionsConfigurationOperations : INamespacedResourceOperations<ExtensionsConfiguration>
{
}

/// <summary>
/// Operations for <see cref="Identity"/> resources.
/// </summary>
public interface IIdentityOperations : INamespacedResourceOperations<Identity>
{
}

/// <summary>
/// Operations for <see cref="ControlPlane"/> resources.
/// </summary>
public interface IControlPlaneOperations : INamespacedResourceOperations<ControlPlane>
{
}

/// <summary>
/// Operations for <see cref="LoadBalancerConfig"/> resources.
/// </summary>
public interface ILoadBalancerOperations : INamespacedResourceOperations<LoadBalancerConfig>
{
}

/// <summary>
/// Operations for <see cref="TalosConfig"/> resources.
/// </summary>
public interface ITalosConfigOperations : INamespacedResourceOperations<TalosConfig>
{
}

/// <summary>
/// Operations for <see cref="KubernetesNode"/> resources.
/// </summary>
public interface IKubernetesNodeOperations : INamespacedResourceOperations<KubernetesNode>
{
}

/// <summary>
/// Operations for <see cref="MachineClass"/> resources.
/// </summary>
public interface IMachineClassOperations : INamespacedResourceOperations<MachineClass>
{
}
