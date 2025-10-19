namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents a set of machines grouped together
/// Proto type: MachineSets.omni.sidero.dev
/// </summary>
public class MachineSet : OmniResource<MachineSetSpec, MachineSetStatus>
{
    /// <inheritdoc />
    public override string Kind => "MachineSet";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}