using System.Runtime.CompilerServices;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Base class for resource-specific operations
/// Provides standard CRUD + Watch operations by delegating to IOmniResourceClient
/// </summary>
/// <typeparam name="TResource">The resource type</typeparam>
internal abstract class ResourceOperationsBase<TResource>(IOmniResourceClient resources, OmniClientOptions options)
	where TResource : IOmniResource, new()
{
	protected readonly IOmniResourceClient Resources = resources ?? throw new ArgumentNullException(nameof(resources));
	protected readonly OmniClientOptions Options = options ?? throw new ArgumentNullException(nameof(options));

	/// <summary>
	/// Lists all resources of this type
	/// </summary>
	public async IAsyncEnumerable<TResource> ListAsync(
		string? @namespace = "default",
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var resource in Resources.ListAsync<TResource>(@namespace, cancellationToken: cancellationToken))
		{
			yield return resource;
		}
	}

	/// <summary>
	/// Gets a specific resource by ID
	/// </summary>
	public virtual async Task<TResource> GetAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default)
	{
		return await Resources.GetAsync<TResource>(id, @namespace ?? Options.DefaultNamespace, cancellationToken);
	}

	/// <summary>
	/// Creates a new resource
	/// </summary>
	public virtual async Task<TResource> CreateAsync(
		TResource resource,
		CancellationToken cancellationToken = default)
	{
		return await Resources.CreateAsync(resource, cancellationToken);
	}

	/// <summary>
	/// Updates an existing resource
	/// </summary>
	public virtual async Task<TResource> UpdateAsync(
		TResource resource,
		string? currentVersion = null,
		CancellationToken cancellationToken = default)
	{
		return await Resources.UpdateAsync(resource, currentVersion, cancellationToken);
	}

	/// <summary>
	/// Deletes a resource by ID
	/// </summary>
	public virtual async Task DeleteAsync(
		string id,
		string? @namespace = "default",
		CancellationToken cancellationToken = default)
	{
		await Resources.DeleteAsync<TResource>(id, @namespace ?? Options.DefaultNamespace, cancellationToken);
	}

	/// <summary>
	/// Watches for resource changes
	/// </summary>
	public async IAsyncEnumerable<ResourceEvent<TResource>> WatchAsync(
		string? @namespace = "default",
		string? id = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var evt in Resources.WatchAsync<TResource>(@namespace, id: id, cancellationToken: cancellationToken))
		{
			yield return evt;
		}
	}

	/// <summary>
	/// Applies a resource (create or update)
	/// </summary>
	public virtual async Task<TResource> ApplyAsync(
		TResource resource,
		bool dryRun = false,
		CancellationToken cancellationToken = default)
	{
		return await Resources.ApplyAsync(resource, dryRun, cancellationToken);
	}
}
