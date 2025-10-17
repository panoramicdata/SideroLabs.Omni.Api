using FluentValidation;
using SideroLabs.Omni.Api.Resources.Validation;

namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents a machine's membership in a cluster
/// </summary>
public class ClusterMachine : OmniResource<ClusterMachineSpec, ClusterMachineStatus>
{
	private static readonly ClusterMachineValidator _validator = new();

	public override string Kind => "ClusterMachine";
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Gets the cluster ID this machine belongs to
	/// </summary>
	public string ClusterId => Spec?.ClusterId ?? string.Empty;

	/// <summary>
	/// Gets the machine ID
	/// </summary>
	public string MachineId => Metadata.Id;

	/// <summary>
	/// Gets whether this is a control plane machine
	/// </summary>
	public bool IsControlPlane => Spec?.Role == "controlplane";

	/// <summary>
	/// Validates the cluster machine resource
	/// </summary>
	/// <returns>Validation result</returns>
	public FluentValidation.Results.ValidationResult Validate()
	{
		return _validator.Validate(this);
	}

	/// <summary>
	/// Validates the cluster machine resource and throws if invalid
	/// </summary>
	/// <exception cref="ValidationException">Thrown when validation fails</exception>
	public void ValidateAndThrow()
	{
		_validator.ValidateAndThrow(this);
	}
}
