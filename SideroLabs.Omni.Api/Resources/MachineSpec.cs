namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Defines the desired machine configuration managed by Omni.
/// </summary>
public class MachineSpec
{
    /// <summary>
    /// Gets or sets the machine role, typically <c>controlplane</c> or <c>worker</c>.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Talos installer image reference used to provision this machine.
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional labels applied to the machine resource for selection and policy.
    /// </summary>
    public Dictionary<string, string>? Labels { get; set; }
}
