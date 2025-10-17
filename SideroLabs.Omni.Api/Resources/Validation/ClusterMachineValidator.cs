using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for ClusterMachine resources
/// </summary>
public class ClusterMachineValidator : AbstractValidator<ClusterMachine>
{
	public ClusterMachineValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("ClusterMachine ID (Metadata.Id) is required");

		RuleFor(x => x.Metadata.Namespace)
			.NotEmpty()
			.WithMessage("Namespace is required");

		RuleFor(x => x.Spec.ClusterId)
			.NotEmpty()
			.WithMessage("Cluster ID is required")
			.Matches("^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
			.WithMessage("Cluster ID must be a valid DNS-1123 subdomain");

		RuleFor(x => x.Spec.Role)
			.NotEmpty()
			.WithMessage("Role is required")
			.Must(role => role == "controlplane" || role == "worker")
			.WithMessage("Role must be either 'controlplane' or 'worker'");

		RuleFor(x => x.Kind)
			.Equal("ClusterMachine")
			.WithMessage("Kind must be 'ClusterMachine'");

		RuleFor(x => x.ApiVersion)
			.Equal("omni.sidero.dev/v1alpha1")
			.WithMessage("ApiVersion must be 'omni.sidero.dev/v1alpha1'");
	}
}
