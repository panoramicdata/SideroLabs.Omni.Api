using System.Runtime.CompilerServices;
using System.Text.Json;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Omni.Resources;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Internal implementation of COSI resource operations for OmniClient
/// </summary>
/// <remarks>
/// Creates a new ResourceClientService
/// </remarks>
internal class ResourceClientService(
	ResourceService.ResourceServiceClient grpcClient,
	ILogger logger,
	bool isReadOnly,
	OmniClientOptions options) : IOmniResourceClient
{
	/// <inheritdoc />
	public async Task<TResource> GetAsync<TResource>(
		string id,
		string? @namespace,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		logger.LogDebug("Getting resource {Type}/{Namespace}/{Id}", resourceType, @namespace, id);

		var request = new GetRequest
		{
			Namespace = @namespace ?? "default",
			Type = resourceType,
			Id = id
		};

		var response = await grpcClient.GetAsync(request, cancellationToken: cancellationToken);

		var resource = JsonSerializer.Deserialize<TResource>(response.Body, OmniClient.JsonSerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize resource {resourceType}/{id}");
		logger.LogInformation("Retrieved resource {Type}/{Namespace}/{Id}", resourceType, @namespace, id);
		return resource;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<TResource> ListAsync<TResource>(
		string? @namespace,
		string? selector,
		string? idMatchRegexp,
		int offset,
		int limit,
		string? sortBy,
		bool sortDescending,
		string[]? searchFor,
		[EnumeratorCancellation] CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		logger.LogDebug("Listing resources {Type}/{Namespace}", resourceType, @namespace);

		var request = new ListRequest
		{
			Namespace = @namespace ?? "default",
			Type = resourceType,
			Offset = offset,
			Limit = limit,
			SortByField = sortBy ?? "",
			SortDescending = sortDescending
		};

		if (searchFor != null)
		{
			request.SearchFor.AddRange(searchFor);
		}

		var response = await grpcClient.ListAsync(request, cancellationToken: cancellationToken);

		logger.LogInformation("Listed {Count} resources of type {Type}", response.Items.Count, resourceType);

		foreach (var item in response.Items)
		{
			var resource = JsonSerializer.Deserialize<TResource>(item, OmniClient.JsonSerializerOptions);

			if (resource != null)
			{
				yield return resource;
			}
		}
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ResourceEvent<TResource>> WatchAsync<TResource>(
		string? @namespace,
		string? selector,
		string? id,
		int tailEvents,
		[EnumeratorCancellation] CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		logger.LogDebug("Watching resources {Type}/{Namespace}", resourceType, @namespace);

		var request = new WatchRequest
		{
			Namespace = @namespace ?? options.DefaultNamespace,
			Type = resourceType,
			Id = id ?? "",
			TailEvents = tailEvents
		};

		using var call = grpcClient.Watch(request, cancellationToken: cancellationToken);

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			if (response.Event == null) continue;

			var resource = JsonSerializer.Deserialize<TResource>(response.Event.Resource, OmniClient.JsonSerializerOptions);

			if (resource == null) continue;

			TResource? oldResource = default;
			if (!string.IsNullOrEmpty(response.Event.Old))
			{
				oldResource = JsonSerializer.Deserialize<TResource>(response.Event.Old, OmniClient.JsonSerializerOptions);
			}

			yield return new ResourceEvent<TResource>
			{
				Type = MapEventType(response.Event.EventType),
				Resource = resource,
				OldResource = oldResource,
				Total = response.Total
			};
		}
	}

	/// <inheritdoc />
	public async Task<TResource> CreateAsync<TResource>(
		TResource resource,
		CancellationToken cancellationToken)
		where TResource : IOmniResource
	{
		CheckReadOnly(nameof(CreateAsync));

		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		logger.LogDebug("Creating resource {Type}/{Namespace}/{Id}",
			resourceType, resource.Metadata.Namespace, resource.Metadata.Id);

		var json = JsonSerializer.Serialize(resource, OmniClient.JsonSerializerOptions);

		var request = new CreateRequest
		{
			Resource = new Resource
			{
				Metadata = resource.Metadata.ToProto(),
				Spec = json
			}
		};

		await grpcClient.CreateAsync(request, cancellationToken: cancellationToken);

		logger.LogInformation("Created resource {Type}/{Namespace}/{Id}",
			resourceType, resource.Metadata.Namespace, resource.Metadata.Id);

		return resource;
	}

	/// <inheritdoc />
	public async Task<TResource> UpdateAsync<TResource>(
		TResource resource,
		string? currentVersion,
		CancellationToken cancellationToken)
		where TResource : IOmniResource
	{
		CheckReadOnly(nameof(UpdateAsync));

		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		logger.LogDebug("Updating resource {Type}/{Namespace}/{Id}",
			resourceType, resource.Metadata.Namespace, resource.Metadata.Id);

		var json = JsonSerializer.Serialize(resource, OmniClient.JsonSerializerOptions);

		var request = new UpdateRequest
		{
			CurrentVersion = currentVersion ?? resource.Metadata.Version,
			Resource = new Resource
			{
				Metadata = resource.Metadata.ToProto(),
				Spec = json
			}
		};

		await grpcClient.UpdateAsync(request, cancellationToken: cancellationToken);

		logger.LogInformation("Updated resource {Type}/{Namespace}/{Id}",
			resourceType, resource.Metadata.Namespace, resource.Metadata.Id);

		return resource;
	}

	/// <inheritdoc />
	public async Task DeleteAsync<TResource>(
		string id,
		string? @namespace,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		CheckReadOnly(nameof(DeleteAsync));

		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		logger.LogDebug("Deleting resource {Type}/{Namespace}/{Id}", resourceType, @namespace, id);

		var request = new DeleteRequest
		{
			Namespace = @namespace ?? options.DefaultNamespace,
			Type = resourceType,
			Id = id
		};

		await grpcClient.DeleteAsync(request, cancellationToken: cancellationToken);

		logger.LogInformation("Deleted resource {Type}/{Namespace}/{Id}", resourceType, @namespace, id);
	}

	/// <inheritdoc />
	public Task<TResource> ApplyAsync<TResource>(
		TResource resource,
		bool dryRun,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		return ApplyInternalAsync(resource, dryRun, cancellationToken);
	}

	private async Task<TResource> ApplyInternalAsync<TResource>(
		TResource resource,
		bool dryRun,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		try
		{
			await GetAsync<TResource>(resource.Metadata.Id, resource.Metadata.Namespace, cancellationToken);

			if (!dryRun)
			{
				return await UpdateAsync(resource, null, cancellationToken);
			}

			logger.LogInformation("Dry run: Would update resource {Type}/{Namespace}/{Id}",
				resource.Kind, resource.Metadata.Namespace, resource.Metadata.Id);
			return resource;
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
		{
			if (!dryRun)
			{
				return await CreateAsync(resource, cancellationToken);
			}

			logger.LogInformation("Dry run: Would create resource {Type}/{Namespace}/{Id}",
				resource.Kind, resource.Metadata.Namespace, resource.Metadata.Id);
			return resource;
		}
	}

	private void CheckReadOnly(string operation)
	{
		if (isReadOnly)
		{
			throw new Exceptions.ReadOnlyModeException(operation, "Resource",
				$"Cannot perform {operation} when client is in read-only mode");
		}
	}

	/// <inheritdoc />
	public async Task<TResource> ApplyYamlAsync<TResource>(
		string yaml,
		bool dryRun,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		var resource = Serialization.ResourceSerializer.FromYaml<TResource>(yaml)
				?? throw new InvalidOperationException("Failed to deserialize YAML to resource");
		return await ApplyAsync(resource, dryRun, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<TResource> ApplyFileAsync<TResource>(
		string filePath,
		bool dryRun,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		var yaml = await File.ReadAllTextAsync(filePath, cancellationToken);
		return await ApplyYamlAsync<TResource>(yaml, dryRun, cancellationToken);
	}

	private static ResourceEventType MapEventType(EventType eventType) => eventType switch
	{
		EventType.Created => ResourceEventType.Created,
		EventType.Updated => ResourceEventType.Updated,
		EventType.Destroyed => ResourceEventType.Destroyed,
		EventType.Bootstrapped => ResourceEventType.Bootstrapped,
		_ => ResourceEventType.Unknown
	};
}
