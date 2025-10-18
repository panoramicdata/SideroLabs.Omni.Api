using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Builders;

/// <summary>
/// Builder for creating ExtensionsConfiguration resources
/// </summary>
public class ExtensionsConfigurationBuilder
{
	private readonly ExtensionsConfiguration _config;

	/// <summary>
	/// Initializes a new ExtensionsConfigurationBuilder
	/// </summary>
	/// <param name="configId">Unique identifier for the configuration</param>
	public ExtensionsConfigurationBuilder(string configId)
	{
		if (string.IsNullOrWhiteSpace(configId))
		{
			throw new ArgumentException("Configuration ID cannot be null or empty", nameof(configId));
		}

		_config = new ExtensionsConfiguration
		{
			Metadata = new ResourceMetadata
			{
				Id = configId,
				Namespace = "default"
			},
			Spec = new ExtensionsConfigurationSpec
			{
				Extensions = []
			}
		};
	}

	/// <summary>
	/// Adds an extension to the configuration
	/// </summary>
	public ExtensionsConfigurationBuilder WithExtension(string extension)
	{
		if (string.IsNullOrWhiteSpace(extension))
		{
			throw new ArgumentException("Extension name cannot be null or empty", nameof(extension));
		}

		_config.Spec.Extensions ??= [];
		_config.Spec.Extensions.Add(extension);
		return this;
	}

	/// <summary>
	/// Adds multiple extensions to the configuration
	/// </summary>
	public ExtensionsConfigurationBuilder WithExtensions(params string[] extensions)
	{
		foreach (var extension in extensions)
		{
			WithExtension(extension);
		}
		return this;
	}

	/// <summary>
	/// Sets the Talos version for the extensions
	/// </summary>
	public ExtensionsConfigurationBuilder ForTalosVersion(string talosVersion)
	{
		_config.Spec.TalosVersion = talosVersion ?? throw new ArgumentNullException(nameof(talosVersion));
		return this;
	}

	/// <summary>
	/// Sets the namespace
	/// </summary>
	public ExtensionsConfigurationBuilder InNamespace(string @namespace)
	{
		_config.Metadata.Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
		return this;
	}

	/// <summary>
	/// Adds a label to the configuration
	/// </summary>
	public ExtensionsConfigurationBuilder WithLabel(string key, string value)
	{
		_config.Metadata.Labels[key] = value;
		return this;
	}

	/// <summary>
	/// Adds multiple labels to the configuration
	/// </summary>
	public ExtensionsConfigurationBuilder WithLabels(Dictionary<string, string> labels)
	{
		foreach (var kvp in labels)
		{
			_config.Metadata.Labels[kvp.Key] = kvp.Value;
		}
		return this;
	}

	/// <summary>
	/// Adds an annotation to the configuration
	/// </summary>
	public ExtensionsConfigurationBuilder WithAnnotation(string key, string value)
	{
		_config.Metadata.Annotations[key] = value;
		return this;
	}

	/// <summary>
	/// Builds the ExtensionsConfiguration resource
	/// </summary>
	public ExtensionsConfiguration Build()
	{
		if (_config.Spec.Extensions == null || _config.Spec.Extensions.Count == 0)
		{
			throw new InvalidOperationException("At least one extension is required. Call WithExtension() before Build().");
		}

		// Validate the configuration
		_config.ValidateAndThrow();

		return _config;
	}

	/// <summary>
	/// Implicit conversion to ExtensionsConfiguration
	/// </summary>
	public static implicit operator ExtensionsConfiguration(ExtensionsConfigurationBuilder builder) => builder.Build();
}
