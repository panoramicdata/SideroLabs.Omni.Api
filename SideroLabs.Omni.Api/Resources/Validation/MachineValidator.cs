using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for Machine resources
/// </summary>
public class MachineValidator : AbstractValidator<Machine>
{
	public MachineValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("Machine ID (Metadata.Id) is required")
			.Matches("^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$")
			.WithMessage("Machine ID must be a valid UUID");

		RuleFor(x => x.Metadata.Namespace)
			.NotEmpty()
			.WithMessage("Namespace is required");

		RuleFor(x => x.Spec.Role)
			.NotEmpty()
			.WithMessage("Role is required")
			.Must(role => role == "controlplane" || role == "worker")
			.WithMessage("Role must be either 'controlplane' or 'worker'");

		RuleFor(x => x.Kind)
			.Equal("Machine")
			.WithMessage("Kind must be 'Machine'");

		RuleFor(x => x.ApiVersion)
			.Equal("omni.sidero.dev/v1alpha1")
			.WithMessage("ApiVersion must be 'omni.sidero.dev/v1alpha1'");
	}
}
