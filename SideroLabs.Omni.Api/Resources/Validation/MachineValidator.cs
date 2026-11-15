using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for Machine resources
/// </summary>
public class MachineValidator : AbstractValidator<Machine>
{
	/// <summary>
	/// Initializes validation rules for <see cref="Machine"/> resources, including role and identity constraints.
	/// </summary>
	public MachineValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("Machine ID (Metadata.Id) is required")
			.Matches("^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$")
			.WithMessage("Machine ID must be a valid UUID");

		this.RequireNamespace();

		this.RequireNodeRole(x => x.Spec.Role);

		this.RequireKind("Machine");
	}
}
