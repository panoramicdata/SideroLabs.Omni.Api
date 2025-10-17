using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

/// <summary>
/// Validator for Cluster resources
/// </summary>
public class ClusterValidator : AbstractValidator<Cluster>
{
	public ClusterValidator()
	{
		RuleFor(x => x.Metadata.Id)
			.NotEmpty()
			.WithMessage("Cluster ID (Metadata.Id) is required")
			.Matches("^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
			.WithMessage("Cluster ID must be a valid DNS-1123 subdomain (lowercase alphanumeric and hyphens)");

		RuleFor(x => x.Metadata.Namespace)
			.NotEmpty()
			.WithMessage("Namespace is required");

		RuleFor(x => x.Spec.KubernetesVersion)
			.NotEmpty()
			.WithMessage("Kubernetes version is required")
			.Matches(@"^v\d+\.\d+\.\d+$")
			.WithMessage("Kubernetes version must be in format vX.Y.Z (e.g., v1.29.0)");

		RuleFor(x => x.Spec.TalosVersion)
			.NotEmpty()
			.WithMessage("Talos version is required")
			.Matches(@"^v\d+\.\d+\.\d+$")
			.WithMessage("Talos version must be in format vX.Y.Z (e.g., v1.7.0)");

		RuleFor(x => x.Kind)
			.Equal("Cluster")
			.WithMessage("Kind must be 'Cluster'");

		RuleFor(x => x.ApiVersion)
			.Equal("omni.sidero.dev/v1alpha1")
			.WithMessage("ApiVersion must be 'omni.sidero.dev/v1alpha1'");
	}
}
