namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents the control plane of a cluster
/// Proto type: ControlPlanes.omni.sidero.dev
/// </summary>
public class ControlPlane : OmniResource<ControlPlaneSpec, ControlPlaneStatus>
{
    /// <inheritdoc />
    public override string Kind => "ControlPlane";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}