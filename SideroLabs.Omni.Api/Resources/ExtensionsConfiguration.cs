using FluentValidation;
using FluentValidation.Results;
using SideroLabs.Omni.Api.Resources.Validation;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Strongly-typed ExtensionsConfiguration resource
/// Represents Talos extensions configuration
/// </summary>
public class ExtensionsConfiguration : OmniResource<ExtensionsConfigurationSpec, ExtensionsConfigurationStatus>
{
	private static readonly ExtensionsConfigurationValidator _validator = new();

	public override string Kind => "ExtensionsConfiguration";
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Configuration ID
	/// </summary>
	public string ConfigurationId => Metadata.Id;

	/// <summary>
	/// Number of extensions configured
	/// </summary>
	public int ExtensionCount => Spec.Extensions?.Count ?? 0;

	/// <summary>
	/// Validates the extensions configuration resource
	/// </summary>
	/// <returns>Validation result</returns>
	public ValidationResult Validate()
	{
		return _validator.Validate(this);
	}

	/// <summary>
	/// Validates the extensions configuration resource and throws if invalid
	/// </summary>
	/// <exception cref="ValidationException">Thrown when validation fails</exception>
	public void ValidateAndThrow()
	{
		_validator.ValidateAndThrow(this);
	}
}
