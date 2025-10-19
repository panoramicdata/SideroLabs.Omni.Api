namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents load balancer configuration
/// Proto type: LoadBalancerConfigs.omni.sidero.dev
/// </summary>
public class LoadBalancerConfig : OmniResource<LoadBalancerConfigSpec, LoadBalancerConfigStatus>
{
    /// <inheritdoc />
    public override string Kind => "LoadBalancerConfig";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}