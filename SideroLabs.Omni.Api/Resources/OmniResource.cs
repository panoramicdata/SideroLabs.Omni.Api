using System.Text.Json;
using YamlDotNet.Serialization;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Base class for Omni resources with typed spec and status
/// </summary>
/// <typeparam name="TSpec">Resource specification type</typeparam>
/// <typeparam name="TStatus">Resource status type</typeparam>
public abstract class OmniResource<TSpec, TStatus> : IOmniResource
	where TSpec : class, new()
	where TStatus : class, new()
{
	/// <inheritdoc />
	public ResourceMetadata Metadata { get; set; } = new();

	/// <summary>
	/// Resource specification
	/// </summary>
	public TSpec Spec { get; set; } = new();

	/// <summary>
	/// Resource status
	/// </summary>
	public TStatus? Status { get; set; }

	/// <inheritdoc />
	public abstract string Kind { get; }

	/// <inheritdoc />
	public abstract string ApiVersion { get; }

	/// <inheritdoc />
	public string ToYaml()
	{
		var serializer = new SerializerBuilder()
			.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
			.Build();

		var obj = new
		{
			ApiVersion,
			Kind,
			Metadata = new
			{
				Metadata.Namespace,
				Name = Metadata.Id,
				Metadata.Labels,
				Metadata.Annotations
			},
			Spec,
			Status
		};

		return serializer.Serialize(obj);
	}

	/// <inheritdoc />
	public string ToJson()
	{
		var obj = new
		{
			ApiVersion,
			Kind,
			Metadata = new
			{
				Metadata.Namespace,
				Name = Metadata.Id,
				Metadata.Labels,
				Metadata.Annotations,
				Metadata.Version,
				Metadata.Owner,
				Metadata.Phase,
				Metadata.Created,
				Metadata.Updated,
				Metadata.Finalizers
			},
			Spec,
			Status
		};

		return JsonSerializer.Serialize(obj, OmniClient.JsonSerializerOptions);
	}
}
