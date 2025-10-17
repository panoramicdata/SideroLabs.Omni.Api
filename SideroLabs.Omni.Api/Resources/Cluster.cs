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

	public override string Kind => "Cluster";
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	public string ClusterId => Metadata.Id;
	public string KubernetesVersion => Spec.KubernetesVersion ?? string.Empty;
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
