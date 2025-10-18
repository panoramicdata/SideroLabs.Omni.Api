using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for User resources
/// </summary>
public class UserValidator : AbstractValidator<User>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UserValidator"/> class
	/// </summary>
	public UserValidator()
	{
		// Validate metadata
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("User ID is required");

		RuleFor(x => x.Metadata.Namespace)
			.NotEmpty()
			.WithMessage("Namespace is required");

		// Validate spec
		RuleFor(x => x.Spec.Role)
			.NotEmpty()
			.WithMessage("Role is required")
			.Must(BeValidRole)
			.WithMessage("Role must be one of: Admin, Operator, Reader, None");
	}

	private static bool BeValidRole(string role)
	{
		var validRoles = new[] { "Admin", "Operator", "Reader", "None" };
		return validRoles.Contains(role);
	}
}
