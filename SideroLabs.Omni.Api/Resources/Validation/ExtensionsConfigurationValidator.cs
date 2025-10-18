using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for ExtensionsConfiguration resources
/// </summary>
public class ExtensionsConfigurationValidator : AbstractValidator<ExtensionsConfiguration>
{
	public ExtensionsConfigurationValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("ExtensionsConfiguration ID is required")
			.Matches("^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
			.WithMessage("ExtensionsConfiguration ID must be a valid DNS-1123 label (lowercase alphanumeric with hyphens)");

		RuleFor(x => x.Spec.Extensions)
			.NotNull()
			.WithMessage("Extensions list cannot be null")
			.Must(extensions => extensions == null || extensions.Count > 0)
			.WithMessage("At least one extension must be specified");

		RuleForEach(x => x.Spec.Extensions)
			.NotEmpty()
			.WithMessage("Extension name cannot be empty")
			.Must(ext => ext.Contains('/') || !ext.Contains('@'))
			.WithMessage("Extension must be in format 'org/name' or 'org/name@version'");

		RuleFor(x => x.Spec.TalosVersion)
			.Matches(@"^v\d+\.\d+\.\d+(-[a-z0-9]+)?$")
			.When(x => !string.IsNullOrEmpty(x.Spec.TalosVersion))
			.WithMessage("Talos version must be in format vX.Y.Z (e.g., v1.7.0)");
	}
}
