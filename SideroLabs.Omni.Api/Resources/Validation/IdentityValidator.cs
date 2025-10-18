using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for Identity resources
/// </summary>
public class IdentityValidator : AbstractValidator<Identity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="IdentityValidator"/> class
	/// </summary>
	public IdentityValidator()
	{
		// Validate metadata
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("Email is required")
			.EmailAddress()
			.WithMessage("Must be a valid email address");

		RuleFor(x => x.Metadata.Namespace)
			.NotEmpty()
			.WithMessage("Namespace is required");

		// Validate spec
		RuleFor(x => x.Spec.UserId)
			.NotEmpty()
			.WithMessage("User ID is required");

		// Validate labels
		RuleFor(x => x.Metadata.Labels)
			.Must(labels => labels.ContainsKey(Identity.LabelUserID))
			.WithMessage($"Identity must have '{Identity.LabelUserID}' label");

		RuleFor(x => x.Metadata.Labels)
			.Must(labels => !labels.ContainsKey(Identity.LabelType) || 
			                labels[Identity.LabelType] == Identity.TypeUser ||
			                labels[Identity.LabelType] == Identity.TypeServiceAccount)
			.WithMessage($"Identity type must be '{Identity.TypeUser}' or '{Identity.TypeServiceAccount}'");

		// Validate that label matches spec
		RuleFor(x => x)
			.Must(identity => !identity.Metadata.Labels.ContainsKey(Identity.LabelUserID) ||
			                  identity.Metadata.Labels[Identity.LabelUserID] == identity.Spec.UserId)
			.WithMessage("Label user ID must match spec user ID");
	}
}
