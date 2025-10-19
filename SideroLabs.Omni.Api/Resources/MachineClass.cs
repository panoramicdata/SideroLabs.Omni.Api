namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents a class/type of machines
/// Proto type: MachineClasses.omni.sidero.dev
/// </summary>
public class MachineClass : OmniResource<MachineClassSpec, MachineClassStatus>
{
    /// <inheritdoc />
    public override string Kind => "MachineClass";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}