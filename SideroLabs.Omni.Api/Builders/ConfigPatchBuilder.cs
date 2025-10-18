using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Builders;

/// <summary>
/// Builder for creating ConfigPatch resources
/// </summary>
public class ConfigPatchBuilder
{
	private readonly ConfigPatch _configPatch;

	/// <summary>
	/// Initializes a new ConfigPatchBuilder
	/// </summary>
	/// <param name="patchId">Unique identifier for the patch</param>
	public ConfigPatchBuilder(string patchId)
	{
		if (string.IsNullOrWhiteSpace(patchId))
		{
			throw new ArgumentException("Patch ID cannot be null or empty", nameof(patchId));
		}

		_configPatch = new ConfigPatch
		{
			Metadata = new ResourceMetadata
			{
				Id = patchId,
				Namespace = "default"
			},
			Spec = new ConfigPatchSpec()
		};
	}

	/// <summary>
	/// Sets the YAML patch data
	/// </summary>
	public ConfigPatchBuilder WithData(string data)
	{
		_configPatch.Spec.Data = data ?? throw new ArgumentNullException(nameof(data));
		return this;
	}

	/// <summary>
	/// Sets the namespace
	/// </summary>
	public ConfigPatchBuilder InNamespace(string @namespace)
	{
		_configPatch.Metadata.Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
		return this;
	}

	/// <summary>
	/// Adds a label to the patch
	/// </summary>
	public ConfigPatchBuilder WithLabel(string key, string value)
	{
		_configPatch.Metadata.Labels[key] = value;
		return this;
	}

	/// <summary>
	/// Adds multiple labels to the patch
	/// </summary>
	public ConfigPatchBuilder WithLabels(Dictionary<string, string> labels)
	{
		foreach (var kvp in labels)
		{
			_configPatch.Metadata.Labels[kvp.Key] = kvp.Value;
		}
		return this;
	}

	/// <summary>
	/// Adds an annotation to the patch
	/// </summary>
	public ConfigPatchBuilder WithAnnotation(string key, string value)
	{
		_configPatch.Metadata.Annotations[key] = value;
		return this;
	}

	/// <summary>
	/// Builds the ConfigPatch resource
	/// </summary>
	public ConfigPatch Build()
	{
		if (string.IsNullOrWhiteSpace(_configPatch.Spec.Data))
		{
			throw new InvalidOperationException("ConfigPatch data is required. Call WithData() before Build().");
		}

		// Validate the patch
		_configPatch.ValidateAndThrow();

		return _configPatch;
	}

	/// <summary>
	/// Implicit conversion to ConfigPatch
	/// </summary>
	public static implicit operator ConfigPatch(ConfigPatchBuilder builder) => builder.Build();
}
