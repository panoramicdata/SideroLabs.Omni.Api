using Google.Protobuf;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Handles deserialization of COSI resource proto specs to our resource types
/// Maps between proto message types and our resource spec classes
/// NOTE: Currently returns null for all types - implementation in progress
/// </summary>
internal static class ProtoSpecDeserializer
{
	/// <summary>
	/// Deserializes a proto spec for a given resource type
	/// </summary>
	/// <typeparam name="TResource">The resource type</typeparam>
	/// <param name="protoSpecBytes">The protobuf-encoded spec bytes</param>
	/// <returns>The deserialized spec object, or null if deserialization is not yet implemented</returns>
	public static object? DeserializeSpec<TResource>(ByteString protoSpecBytes)
		where TResource : IOmniResource
	{
		// TODO: Implement proto spec deserialization
		// We have the proto files and they compile, but need to:
		// 1. Find the correct namespace for generated classes
		// 2. Map proto messages to our resource spec classes
		// 3. Handle all the field mappings
		
		// For now, return null so tests still pass with metadata-only
		return null;
	}

	/// <summary>
	/// Checks if spec deserialization is implemented for a resource type
	/// </summary>
	public static bool IsImplemented<TResource>() where TResource : IOmniResource
	{
		// Not implemented yet
		return false;
	}
}
