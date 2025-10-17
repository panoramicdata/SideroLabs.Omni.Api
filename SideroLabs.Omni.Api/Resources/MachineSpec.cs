namespace SideroLabs.Omni.Api.Resources;

public class MachineSpec
{
    public string Role { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public Dictionary<string, string>? Labels { get; set; }
}
