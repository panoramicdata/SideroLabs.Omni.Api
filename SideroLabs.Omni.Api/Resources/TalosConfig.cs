namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents Talos configuration
/// Proto type: TalosConfigs.omni.sidero.dev
/// </summary>
public class TalosConfig : OmniResource<TalosConfigSpec, TalosConfigStatus>
{
    /// <inheritdoc />
    public override string Kind => "TalosConfig";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}