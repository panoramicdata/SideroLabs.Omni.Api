using System.Runtime.InteropServices;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for COSI resource operations
/// Provides access to Omni's resource-based API
/// </summary>
public interface IOmniResourceClient
{
	/// <summary>
	/// Gets a single resource by ID
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="id">Resource ID</param>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The requested resource</returns>
	Task<TResource> GetAsync<TResource>(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Lists resources of the specified type
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="selector">Label selector query</param>
	/// <param name="idMatchRegexp">Regular expression to match resource IDs</param>
	/// <param name="offset">Pagination offset</param>
	/// <param name="limit">Pagination limit (0 = no limit)</param>
	/// <param name="sortBy">Field to sort by</param>
	/// <param name="sortDescending">Sort in descending order</param>
	/// <param name="searchFor">Search terms</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of resources</returns>
	IAsyncEnumerable<TResource> ListAsync<TResource>(
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] string? selector,
		[Optional] string? idMatchRegexp,
		[Optional] int offset,
		[Optional] int limit,
		[Optional] string? sortBy,
		[Optional] bool sortDescending,
		[Optional] string[]? searchFor,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Watches for resource changes
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="selector">Label selector query</param>
	/// <param name="id">Specific resource ID to watch (optional)</param>
	/// <param name="tailEvents">Number of recent events to replay</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Async enumerable of resource events</returns>
	IAsyncEnumerable<ResourceEvent<TResource>> WatchAsync<TResource>(
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] string? selector,
		[Optional] string? id,
		[Optional] int tailEvents,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Creates a new resource
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="resource">Resource to create</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The created resource</returns>
	Task<TResource> CreateAsync<TResource>(
		TResource resource,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource;

	/// <summary>
	/// Updates an existing resource
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="resource">Resource to update</param>
	/// <param name="currentVersion">Current resource version (for optimistic locking)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The updated resource</returns>
	Task<TResource> UpdateAsync<TResource>(
		TResource resource,
		[Optional] string? currentVersion,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource;

	/// <summary>
	/// Deletes a resource by ID
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="id">Resource ID</param>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task DeleteAsync<TResource>(
		string id,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Deletes multiple resources matching a selector
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="selector">Label selector query</param>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Number of resources deleted</returns>
	Task<int> DeleteManyAsync<TResource>(
		[Optional] string? selector,
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Deletes all resources of the specified type
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="namespace">Resource namespace (default: "default")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Number of resources deleted</returns>
	Task<int> DeleteAllAsync<TResource>(
		[Optional, DefaultParameterValue("default")] string? @namespace,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Applies a resource (create or update)
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="resource">Resource to apply</param>
	/// <param name="dryRun">Dry run mode</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The applied resource</returns>
	Task<TResource> ApplyAsync<TResource>(
		TResource resource,
		[Optional] bool dryRun,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Applies a resource from YAML
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="yaml">YAML content</param>
	/// <param name="dryRun">Dry run mode</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The applied resource</returns>
	Task<TResource> ApplyYamlAsync<TResource>(
		string yaml,
		[Optional] bool dryRun,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();

	/// <summary>
	/// Applies a resource from a file
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="filePath">Path to YAML file</param>
	/// <param name="dryRun">Dry run mode</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The applied resource</returns>
	Task<TResource> ApplyFileAsync<TResource>(
		string filePath,
		[Optional] bool dryRun,
		[Optional] CancellationToken cancellationToken)
		where TResource : IOmniResource, new();
}
