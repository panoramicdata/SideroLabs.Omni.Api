using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Builders;

/// <summary>
/// Builder for creating Machine resources with a fluent API
/// </summary>
public class MachineBuilder
{
	private readonly Resources.Machine _machine;

	/// <summary>
	/// Initializes a new instance of the MachineBuilder class
	/// </summary>
	/// <param name="machineId">The ID of the machine</param>
	public MachineBuilder(string machineId)
	{
		if (string.IsNullOrWhiteSpace(machineId))
		{
			throw new ArgumentException("Machine ID cannot be null or empty", nameof(machineId));
		}

		_machine = new Resources.Machine
		{
			Metadata = new ResourceMetadata
			{
				Id = machineId,
				Namespace = "default"
			}
		};
	}

	/// <summary>
	/// Sets the role for the machine (e.g., "controlplane", "worker")
	/// </summary>
	public MachineBuilder WithRole(string role)
	{
		if (string.IsNullOrWhiteSpace(role))
		{
			throw new ArgumentException("Role cannot be null or empty", nameof(role));
		}

		_machine.Spec.Role = role;
		return this;
	}

	/// <summary>
	/// Sets the image for the machine
	/// </summary>
	public MachineBuilder WithImage(string image)
	{
		if (string.IsNullOrWhiteSpace(image))
		{
			throw new ArgumentException("Image cannot be null or empty", nameof(image));
		}

		_machine.Spec.Image = image;
		return this;
	}

	/// <summary>
	/// Sets the namespace for the machine
	/// </summary>
	public MachineBuilder InNamespace(string @namespace)
	{
		if (string.IsNullOrWhiteSpace(@namespace))
		{
			throw new ArgumentException("Namespace cannot be null or empty", nameof(@namespace));
		}

		_machine.Metadata.Namespace = @namespace;
		return this;
	}

	/// <summary>
	/// Adds a label to the machine metadata
	/// </summary>
	public MachineBuilder WithLabel(string key, string value)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Label key cannot be null or empty", nameof(key));
		}

		_machine.Metadata.Labels ??= [];
		_machine.Metadata.Labels[key] = value ?? string.Empty;
		return this;
	}

	/// <summary>
	/// Adds multiple labels to the machine metadata
	/// </summary>
	public MachineBuilder WithLabels(Dictionary<string, string> labels)
	{
		ArgumentNullException.ThrowIfNull(labels);

		_machine.Metadata.Labels ??= [];
		foreach (var (key, value) in labels)
		{
			_machine.Metadata.Labels[key] = value;
		}

		return this;
	}

	/// <summary>
	/// Adds a spec label to the machine
	/// </summary>
	public MachineBuilder WithSpecLabel(string key, string value)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Spec label key cannot be null or empty", nameof(key));
		}

		_machine.Spec.Labels ??= [];
		_machine.Spec.Labels[key] = value ?? string.Empty;
		return this;
	}

	/// <summary>
	/// Adds an annotation to the machine metadata
	/// </summary>
	public MachineBuilder WithAnnotation(string key, string value)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Annotation key cannot be null or empty", nameof(key));
		}

		_machine.Metadata.Annotations ??= [];
		_machine.Metadata.Annotations[key] = value ?? string.Empty;
		return this;
	}

	/// <summary>
	/// Builds and returns the configured Machine resource
	/// </summary>
	public Resources.Machine Build()
	{
		// Validate required fields
		if (string.IsNullOrWhiteSpace(_machine.Spec.Role))
		{
			throw new InvalidOperationException("Role must be set before building the machine");
		}

		return _machine;
	}

	/// <summary>
	/// Implicitly converts the builder to a Machine
	/// </summary>
	public static implicit operator Resources.Machine(MachineBuilder builder) => builder.Build();
}
