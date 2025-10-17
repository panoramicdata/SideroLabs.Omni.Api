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
        string? @namespace = "default",
        CancellationToken cancellationToken = default)
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
        string? @namespace = "default",
        string? selector = null,
        string? idMatchRegexp = null,
        int offset = 0,
        int limit = 0,
        string? sortBy = null,
        bool sortDescending = false,
        string[]? searchFor = null,
        CancellationToken cancellationToken = default)
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
        string? @namespace = "default",
        string? selector = null,
        string? id = null,
        int tailEvents = 0,
        CancellationToken cancellationToken = default)
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
        CancellationToken cancellationToken = default)
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
        string? currentVersion = null,
        CancellationToken cancellationToken = default)
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
        string? @namespace = "default",
        CancellationToken cancellationToken = default)
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
        bool dryRun = false,
        CancellationToken cancellationToken = default)
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
        bool dryRun = false,
        CancellationToken cancellationToken = default)
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
        bool dryRun = false,
        CancellationToken cancellationToken = default)
		where TResource : IOmniResource, new();
}
