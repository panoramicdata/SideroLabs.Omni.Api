using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for ConfigPatch resources
/// </summary>
public class ConfigPatchValidator : AbstractValidator<ConfigPatch>
{
	public ConfigPatchValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("ConfigPatch ID is required")
			.Matches("^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
			.WithMessage("ConfigPatch ID must be a valid DNS-1123 label (lowercase alphanumeric with hyphens)");

		RuleFor(x => x.Spec.Data)
			.NotEmpty()
			.WithMessage("ConfigPatch data is required")
			.Must(BeValidYaml)
			.WithMessage("ConfigPatch data must be valid YAML");
	}

	private static bool BeValidYaml(string data)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return false;
		}

		try
		{
			// Try to parse as YAML
			var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
			deserializer.Deserialize(new StringReader(data));
			return true;
		}
		catch
		{
			return false;
		}
	}
}
