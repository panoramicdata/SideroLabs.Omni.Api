using FluentValidation;
using SideroLabs.Omni.Api.Resources.Validation;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Strongly-typed Machine resource
/// </summary>
public class Machine : OmniResource<MachineSpec, MachineStatus>
{
	private static readonly MachineValidator _validator = new();

	public override string Kind => "Machine";
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	public string MachineId => Metadata.Id;
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
