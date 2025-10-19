namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Represents a node in a machine set
/// Proto type: MachineSetNodes.omni.sidero.dev
/// </summary>
public class MachineSetNode : OmniResource<MachineSetNodeSpec, MachineSetNodeStatus>
{
    /// <inheritdoc />
    public override string Kind => "MachineSetNode";

    /// <inheritdoc />
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
}