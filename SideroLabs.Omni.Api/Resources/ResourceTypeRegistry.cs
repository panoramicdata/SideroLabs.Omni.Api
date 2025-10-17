using System.Collections.Concurrent;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Registry for mapping resource types to their proto type names
/// </summary>
public static class ResourceTypeRegistry
{
	private static readonly ConcurrentDictionary<Type, string> _typeMap = new();

	/// <summary>
	/// Registers a resource type with its proto type name
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <param name="protoTypeName">Proto type name (e.g., "Clusters.omni.sidero.dev")</param>
	public static void Register<TResource>(string protoTypeName) where TResource : IOmniResource
	{
		_typeMap[typeof(TResource)] = protoTypeName;
	}

	/// <summary>
	/// Gets the proto type name for a resource type
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <returns>Proto type name</returns>
	/// <exception cref="InvalidOperationException">If resource type is not registered</exception>
	public static string GetProtoTypeName<TResource>() where TResource : IOmniResource
	{
		if (_typeMap.TryGetValue(typeof(TResource), out var typeName))
		{
			return typeName;
		}

		throw new InvalidOperationException(
			$"Resource type {typeof(TResource).Name} is not registered. " +
			$"Call ResourceTypeRegistry.Register<{typeof(TResource).Name}>(protoTypeName) first.");
	}

	/// <summary>
	/// Checks if a resource type is registered
	/// </summary>
	/// <typeparam name="TResource">Resource type</typeparam>
	/// <returns>True if registered</returns>
	public static bool IsRegistered<TResource>() where TResource : IOmniResource
	{
		return _typeMap.ContainsKey(typeof(TResource));
	}

	/// <summary>
	/// Gets all registered resource types
	/// </summary>
	/// <returns>Dictionary of type to proto type name</returns>
	public static IReadOnlyDictionary<Type, string> GetAll()
	{
		return _typeMap;
	}
}
