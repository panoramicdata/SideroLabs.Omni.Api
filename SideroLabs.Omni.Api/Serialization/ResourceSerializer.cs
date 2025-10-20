using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SideroLabs.Omni.Api.Serialization;

/// <summary>
/// Helper for serializing and deserializing resources
/// </summary>
public static class ResourceSerializer
{
	private static readonly ISerializer _yamlSerializer = new SerializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.Build();

	private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.IgnoreUnmatchedProperties()
		.Build();

	/// <summary>
	/// Serializes an object to JSON
	/// </summary>
	public static string ToJson<T>(T obj)
	{
		return JsonSerializer.Serialize(obj, OmniClient.JsonSerializerOptions);
	}

	/// <summary>
	/// Deserializes JSON to an object
	/// </summary>
	public static T? FromJson<T>(string json)
	{
		return JsonSerializer.Deserialize<T>(json, OmniClient.JsonSerializerOptions);
	}

	/// <summary>
	/// Serializes an object to YAML
	/// </summary>
	public static string ToYaml<T>(T obj)
	{
		return _yamlSerializer.Serialize(obj);
	}

	/// <summary>
	/// Deserializes YAML to an object
	/// </summary>
	public static T? FromYaml<T>(string yaml)
	{
		return _yamlDeserializer.Deserialize<T>(yaml);
	}

	/// <summary>
	/// Converts YAML to JSON
	/// </summary>
	public static string YamlToJson(string yaml)
	{
		var obj = _yamlDeserializer.Deserialize<object>(yaml);
		return JsonSerializer.Serialize(obj, OmniClient.JsonSerializerOptions);
	}

	/// <summary>
	/// Converts JSON to YAML
	/// </summary>
	public static string JsonToYaml(string json)
	{
		var obj = JsonSerializer.Deserialize<object>(json, OmniClient.JsonSerializerOptions);
		return _yamlSerializer.Serialize(obj);
	}
}
