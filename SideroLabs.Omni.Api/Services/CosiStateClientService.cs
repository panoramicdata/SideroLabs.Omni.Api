using System.Runtime.CompilerServices;
using System.Text.Json;
using Cosi.Resource;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of COSI State service client
/// This is the CORRECT service to use for resource operations on Omni SaaS (not ResourceService!)
/// Uses the COSI v1alpha1 State gRPC service at /cosi.resource.State/*
/// </summary>
internal class CosiStateClientService : IOmniResourceClient
{
	private readonly State.StateClient _grpcClient;
	private readonly ILogger _logger;
	private readonly bool _isReadOnly;
	private readonly OmniClientOptions _options;
	private readonly OmniAuthenticator? _authenticator;

	private const string ServiceBasePath = "/cosi.resource.State";

	public CosiStateClientService(
		GrpcChannel channel,
		ILogger logger,
		bool isReadOnly,
		OmniClientOptions options,
		OmniAuthenticator? authenticator)
	{
		_grpcClient = new State.StateClient(channel);
		_logger = logger;
		_isReadOnly = isReadOnly;
		_options = options;
		_authenticator = authenticator;
	}

	/// <summary>
	/// Creates headers with authentication for COSI State calls
	/// </summary>
	private Grpc.Core.Metadata CreateAuthHeaders(string method)
	{
		var headers = new Grpc.Core.Metadata();

		if (_authenticator != null)
		{
			_authenticator.SignRequest(headers, $"{ServiceBasePath}/{method}");
		}

		return headers;
	}

	/// <summary>
	/// Creates call options for gRPC calls
	/// </summary>
	private CallOptions CreateCallOptions(string method)
	{
		var headers = CreateAuthHeaders(method);
		var deadline = DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds);
		return new CallOptions(headers: headers, deadline: deadline);
	}

	/// <inheritdoc />
	public async Task<TResource> GetAsync<TResource>(
		string id,
		string? @namespace,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		var resourceType = ResourceTypeRegistry.GetProtoTypeName<TResource>();
		_logger.LogDebug("Getting resource {Type}/{Namespace}/{Id} via COSI State", resourceType, @namespace, id);

		var request = new GetRequest
		{
			Namespace = @namespace ?? "default",
			Type = resourceType,
			Id = id
		};

		var callOptions = CreateCallOptions("Get");
		var response = await _grpcClient.GetAsync(request, callOptions);

		var resource = DeserializeResource<TResource>(response.Resource);
		_logger.LogInformation("Retrieved resource {Type}/{Namespace}/{Id}", resourceType, @namespace, id);
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
		_logger.LogDebug("Listing resources {Type}/{Namespace} via COSI State", resourceType, @namespace);

		var request = new ListRequest
		{
			Namespace = @namespace ?? "default",
			Type = resourceType
		};

		var callOptions = CreateCallOptions("List");

		_logger.LogDebug("Calling COSI State.List for {Type}", resourceType);

		using var call = _grpcClient.List(request, callOptions);

		var count = 0;
		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			if (response.Resource != null)
			{
				var resource = DeserializeResource<TResource>(response.Resource);
				count++;
				yield return resource;
			}
		}

		_logger.LogInformation("Listed {Count} resources of type {Type}", count, resourceType);
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
		_logger.LogDebug("Watching resources {Type}/{Namespace} via COSI State", resourceType, @namespace);

		var request = new WatchRequest
		{
			Namespace = @namespace ?? _options.DefaultNamespace,
			Type = resourceType,
			ApiVersion = 1 // Use API version 1 for extended event types
		};

		if (!string.IsNullOrEmpty(id))
		{
			request.Id = id;
		}

		request.Options = new WatchOptions
		{
			BootstrapContents = tailEvents > 0,
			TailEvents = tailEvents
		};

		var callOptions = CreateCallOptions("Watch");
		using var call = _grpcClient.Watch(request, callOptions);

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			foreach (var evt in response.Event)
			{
				if (evt.Resource == null) continue;

				var resource = DeserializeResource<TResource>(evt.Resource);

				TResource? oldResource = default;
				if (evt.Old != null)
				{
					oldResource = DeserializeResource<TResource>(evt.Old);
				}

				yield return new ResourceEvent<TResource>
				{
					Type = MapEventType(evt.EventType),
					Resource = resource,
					OldResource = oldResource
				};
			}
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
		_logger.LogDebug("Creating resource {Type}/{Namespace}/{Id} via COSI State",
			resourceType, resource.Metadata.Namespace, resource.Metadata.Id);

		var cosiResource = SerializeResource(resource);

		var request = new CreateRequest
		{
			Resource = cosiResource
		};

		var callOptions = CreateCallOptions("Create");
		await _grpcClient.CreateAsync(request, callOptions);

		_logger.LogInformation("Created resource {Type}/{Namespace}/{Id}",
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
		_logger.LogDebug("Updating resource {Type}/{Namespace}/{Id} via COSI State",
			resourceType, resource.Metadata.Namespace, resource.Metadata.Id);

		var cosiResource = SerializeResource(resource);

		var request = new UpdateRequest
		{
			NewResource = cosiResource
		};

		var callOptions = CreateCallOptions("Update");
		await _grpcClient.UpdateAsync(request, callOptions);

		_logger.LogInformation("Updated resource {Type}/{Namespace}/{Id}",
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
		_logger.LogDebug("Deleting resource {Type}/{Namespace}/{Id} via COSI State", resourceType, @namespace, id);

		var request = new DestroyRequest
		{
			Namespace = @namespace ?? _options.DefaultNamespace,
			Type = resourceType,
			Id = id
		};

		var callOptions = CreateCallOptions("Destroy");
		await _grpcClient.DestroyAsync(request, callOptions);

		_logger.LogInformation("Deleted resource {Type}/{Namespace}/{Id}", resourceType, @namespace, id);
	}

	/// <inheritdoc />
	public async Task<int> DeleteManyAsync<TResource>(
		string? selector,
		string? @namespace,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		CheckReadOnly(nameof(DeleteManyAsync));

		var resourcesToDelete = new List<string>();
		await foreach (var resource in ListAsync<TResource>(@namespace, selector, null, 0, 0, null, false, null, cancellationToken))
		{
			resourcesToDelete.Add(resource.Metadata.Id);
		}

		var deletedCount = 0;
		foreach (var resourceId in resourcesToDelete)
		{
			try
			{
				await DeleteAsync<TResource>(resourceId, @namespace, cancellationToken);
				deletedCount++;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to delete resource {Id}", resourceId);
			}
		}

		return deletedCount;
	}

	/// <inheritdoc />
	public Task<int> DeleteAllAsync<TResource>(
		string? @namespace,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		return DeleteManyAsync<TResource>(null, @namespace, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<TResource> ApplyAsync<TResource>(
		TResource resource,
		bool dryRun,
		CancellationToken cancellationToken)
		where TResource : IOmniResource, new()
	{
		try
		{
			await GetAsync<TResource>(resource.Metadata.Id, resource.Metadata.Namespace, cancellationToken);
			return dryRun ? resource : await UpdateAsync(resource, null, cancellationToken);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
		{
			return dryRun ? resource : await CreateAsync(resource, cancellationToken);
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
				?? throw new InvalidOperationException("Failed to deserialize YAML");
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

	private void CheckReadOnly(string operation)
	{
		if (_isReadOnly)
		{
			throw new Exceptions.ReadOnlyModeException(operation, "Resource",
				$"Cannot perform {operation} when client is in read-only mode");
		}
	}

	private Cosi.Resource.Resource SerializeResource<TResource>(TResource resource)
		where TResource : IOmniResource
	{
		var specJson = JsonSerializer.Serialize(resource, OmniClient.JsonSerializerOptions);

		return new Cosi.Resource.Resource
		{
			Metadata = new Cosi.Resource.Metadata
			{
				Namespace = resource.Metadata.Namespace,
				Type = ResourceTypeRegistry.GetProtoTypeName<TResource>(),
				Id = resource.Metadata.Id,
				Version = resource.Metadata.Version
			},
			Spec = new Spec
			{
				YamlSpec = specJson
			}
		};
	}

	private TResource DeserializeResource<TResource>(Cosi.Resource.Resource cosiResource)
		where TResource : IOmniResource, new()
	{
		// Create a new instance of the resource
		var resource = new TResource();

		// Map metadata from COSI to our resource
		resource.Metadata = ResourceMetadata.FromProto(cosiResource.Metadata);

		// DIAGNOSTIC: Log what format we receive
		var hasYamlSpec = cosiResource.Spec != null && !string.IsNullOrEmpty(cosiResource.Spec.YamlSpec);
		var hasProtoSpec = cosiResource.Spec?.ProtoSpec != null && !cosiResource.Spec.ProtoSpec.IsEmpty;

		_logger.LogInformation(
			"Spec format - Type: {Type}, YamlSpec: {HasYaml} ({YamlLength} bytes), ProtoSpec: {HasProto} ({ProtoLength} bytes)",
			cosiResource.Metadata.Type,
			hasYamlSpec,
			hasYamlSpec ? cosiResource.Spec!.YamlSpec.Length : 0,
			hasProtoSpec,
			hasProtoSpec ? cosiResource.Spec!.ProtoSpec.Length : 0);

		// Try YamlSpec first (JSON format)
		if (hasYamlSpec)
		{
			try
			{
				_logger.LogDebug("Attempting to deserialize YamlSpec as JSON for {Type}", cosiResource.Metadata.Type);

				var deserializedResource = JsonSerializer.Deserialize<TResource>(
					cosiResource.Spec!.YamlSpec,
					OmniClient.JsonSerializerOptions);

				if (deserializedResource != null)
				{
					var specProperty = typeof(TResource).GetProperty("Spec");
					if (specProperty != null)
					{
						var spec = specProperty.GetValue(deserializedResource);
						if (spec != null)
						{
							specProperty.SetValue(resource, spec);
							_logger.LogInformation("? Successfully deserialized spec from YamlSpec for {Type}", cosiResource.Metadata.Type);
							return resource;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogDebug(ex, "Failed to deserialize YamlSpec as JSON for {Type}", cosiResource.Metadata.Type);
			}
		}

		// Try ProtoSpec with our deserializer
		if (hasProtoSpec)
		{
			try
			{
				var spec = ProtoSpecDeserializer.DeserializeSpec<TResource>(cosiResource.Spec!.ProtoSpec);
				if (spec != null)
				{
					var specProperty = typeof(TResource).GetProperty("Spec");
					if (specProperty != null && specProperty.CanWrite)
					{
						specProperty.SetValue(resource, spec);
						_logger.LogInformation("? Successfully deserialized spec from ProtoSpec for {Type}", cosiResource.Metadata.Type);
						return resource;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogDebug(ex, "Failed to deserialize ProtoSpec for {Type}", cosiResource.Metadata.Type);
			}
		}

		_logger.LogDebug("Returning resource with metadata only - no spec deserialization available for {Type}", cosiResource.Metadata.Type);
		return resource;
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
