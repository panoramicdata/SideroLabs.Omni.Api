using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Maps between COSI wire resources and Omni resource types
/// </summary>
internal static class CosiResourceMapper
{
	public static Cosi.Resource.Resource Serialize<TResource>(TResource resource)
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
			Spec = new Cosi.Resource.Spec
			{
				YamlSpec = specJson
			}
		};
	}

	public static TResource Deserialize<TResource>(Cosi.Resource.Resource cosiResource, ILogger logger)
		where TResource : IOmniResource, new()
	{
		var resource = new TResource
		{
			Metadata = ResourceMetadata.FromProto(cosiResource.Metadata)
		};

		var type = cosiResource.Metadata.Type;
		var spec = cosiResource.Spec ?? new Cosi.Resource.Spec();
		var yamlSpec = spec.YamlSpec;
		var protoSpec = spec.ProtoSpec;
		var hasYamlSpec = yamlSpec.Length > 0;
		var hasProtoSpec = !protoSpec.IsEmpty;

		logger.LogInformation(
			"Spec format - Type: {Type}, YamlSpec: {HasYaml} ({YamlLength} bytes), ProtoSpec: {HasProto} ({ProtoLength} bytes)",
			type,
			hasYamlSpec,
			yamlSpec.Length,
			hasProtoSpec,
			protoSpec.Length);

		if (hasYamlSpec && TryApplyYamlSpec(resource, yamlSpec, type, logger))
		{
			return resource;
		}

		if (hasProtoSpec && TryApplyProtoSpec(resource, protoSpec, type, logger))
		{
			return resource;
		}

		logger.LogDebug("Returning resource with metadata only - no spec deserialization available for {Type}", type);
		return resource;
	}

	private static bool TryApplyYamlSpec<TResource>(TResource resource, string yamlSpec, string type, ILogger logger)
		where TResource : IOmniResource, new()
	{
		try
		{
			logger.LogDebug("Attempting to deserialize YamlSpec as JSON for {Type}", type);

			var deserialized = JsonSerializer.Deserialize<TResource>(yamlSpec, OmniClient.JsonSerializerOptions);
			var spec = deserialized == null ? null : typeof(TResource).GetProperty("Spec")?.GetValue(deserialized);
			if (spec == null)
			{
				return false;
			}

			typeof(TResource).GetProperty("Spec")!.SetValue(resource, spec);
			logger.LogInformation("Successfully deserialized spec from YamlSpec for {Type}", type);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogDebug(ex, "Failed to deserialize YamlSpec as JSON for {Type}", type);
			return false;
		}
	}

	private static bool TryApplyProtoSpec<TResource>(TResource resource, ByteString protoSpec, string type, ILogger logger)
		where TResource : IOmniResource, new()
	{
		try
		{
			var spec = ProtoSpecDeserializer.DeserializeSpec(protoSpec);
			var specProperty = typeof(TResource).GetProperty("Spec");
			if (spec == null || specProperty is not { CanWrite: true })
			{
				return false;
			}

			specProperty.SetValue(resource, spec);
			logger.LogInformation("Successfully deserialized spec from ProtoSpec for {Type}", type);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogDebug(ex, "Failed to deserialize ProtoSpec for {Type}", type);
			return false;
		}
	}
}