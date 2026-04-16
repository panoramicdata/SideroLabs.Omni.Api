using FluentValidation;
using FluentValidation.Results;
using SideroLabs.Omni.Api.Resources.Validation;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Strongly-typed Cluster resource
/// </summary>
public class Cluster : OmniResource<ClusterSpec, ClusterStatus>
{
	private static readonly ClusterValidator _validator = new();

	/// <summary>
	/// Gets the Omni resource kind for cluster resources.
	/// </summary>
	public override string Kind => "Cluster";

	/// <summary>
	/// Gets the Omni API version used for cluster resources.
	/// </summary>
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Gets the cluster identifier from resource metadata.
	/// </summary>
	public string ClusterId => Metadata.Id;

	/// <summary>
	/// Gets the configured Kubernetes version for the cluster specification.
	/// </summary>
	public string KubernetesVersion => Spec.KubernetesVersion ?? string.Empty;

	/// <summary>
	/// Gets a value indicating whether Omni reports the cluster as ready.
	/// </summary>
	public bool IsReady => Status?.Ready ?? false;

	/// <summary>
	/// Validates the cluster resource
	/// </summary>
	/// <returns>Validation result</returns>
	public ValidationResult Validate()
	{
		return _validator.Validate(this);
	}

	/// <summary>
	/// Validates the cluster resource and throws if invalid
	/// </summary>
	/// <exception cref="ValidationException">Thrown when validation fails</exception>
	public void ValidateAndThrow()
	{
		_validator.ValidateAndThrow(this);
	}
}
