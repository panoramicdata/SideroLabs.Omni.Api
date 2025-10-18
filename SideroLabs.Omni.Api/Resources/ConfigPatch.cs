using FluentValidation;
using FluentValidation.Results;
using SideroLabs.Omni.Api.Resources.Validation;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Strongly-typed ConfigPatch resource
/// Represents a Talos machine configuration patch
/// </summary>
public class ConfigPatch : OmniResource<ConfigPatchSpec, ConfigPatchStatus>
{
	private static readonly ConfigPatchValidator _validator = new();

	public override string Kind => "ConfigPatch";
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Patch ID
	/// </summary>
	public string PatchId => Metadata.Id;

	/// <summary>
	/// Validates the config patch resource
	/// </summary>
	/// <returns>Validation result</returns>
	public ValidationResult Validate()
	{
		return _validator.Validate(this);
	}

	/// <summary>
	/// Validates the config patch resource and throws if invalid
	/// </summary>
	/// <exception cref="ValidationException">Thrown when validation fails</exception>
	public void ValidateAndThrow()
	{
		_validator.ValidateAndThrow(this);
	}
}
