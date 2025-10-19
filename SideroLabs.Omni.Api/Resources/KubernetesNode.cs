namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents a Kubernetes node
/// Proto type: KubernetesNodes.omni.sidero.dev
/// </summary>
public class KubernetesNode : OmniResource<KubernetesNodeSpec, KubernetesNodeStatus>
{
    /// <inheritdoc />
    public override string Kind => "KubernetesNode";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}