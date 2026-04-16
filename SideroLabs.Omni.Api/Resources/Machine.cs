using FluentValidation;
using SideroLabs.Omni.Api.Resources.Validation;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Strongly-typed Machine resource
/// </summary>
public class Machine : OmniResource<MachineSpec, MachineStatus>
{
	private static readonly MachineValidator _validator = new();

	/// <summary>
	/// Gets the Omni resource kind for machine resources.
	/// </summary>
	public override string Kind => "Machine";

	/// <summary>
	/// Gets the Omni API version used for machine resources.
	/// </summary>
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Gets the machine identifier from resource metadata.
	/// </summary>
	public string MachineId => Metadata.Id;

	/// <summary>
	/// Gets a value indicating whether the machine is currently lock-protected in Omni.
	/// </summary>
	public bool IsLocked => Status?.Locked ?? false;

	/// <summary>
	/// Validates the machine resource
	/// </summary>
	/// <returns>Validation result</returns>
	public FluentValidation.Results.ValidationResult Validate()
	{
		return _validator.Validate(this);
	}

	/// <summary>
	/// Validates the machine resource and throws if invalid
	/// </summary>
	/// <exception cref="ValidationException">Thrown when validation fails</exception>
	public void ValidateAndThrow()
	{
		_validator.ValidateAndThrow(this);
	}
}
