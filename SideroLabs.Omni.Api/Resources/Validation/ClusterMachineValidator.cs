using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for ClusterMachine resources
/// </summary>
public class ClusterMachineValidator : AbstractValidator<ClusterMachine>
{
	/// <summary>
	/// Initializes validation rules for <see cref="ClusterMachine"/> resources before create or update operations.
	/// </summary>
	public ClusterMachineValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("ClusterMachine ID (Metadata.Id) is required");

		this.RequireNamespace();

		RuleFor(x => x.Spec.ClusterId)
			.NotEmpty()
			.WithMessage("Cluster ID is required")
			.Matches("^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
			.WithMessage("Cluster ID must be a valid DNS-1123 subdomain");

		this.RequireNodeRole(x => x.Spec.Role);

		this.RequireKind("ClusterMachine");
	}
}
