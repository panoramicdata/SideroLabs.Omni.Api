namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents the configuration for a machine to join a cluster
/// </summary>
public class MachineJoinConfig
{
	/// <summary>
	/// Gets or sets the kernel arguments for the machine
	/// </summary>
	public List<string> KernelArgs { get; set; } = [];

	/// <summary>
	/// Gets or sets the configuration data for the machine
	/// </summary>
	public string Config { get; set; } = "";

	/// <summary>
	/// Gets a value indicating whether this configuration has kernel arguments
	/// </summary>
	public bool HasKernelArgs => KernelArgs.Count > 0;

	/// <summary>
	/// Gets a value indicating whether this configuration has config data
	/// </summary>
	public bool HasConfig => !string.IsNullOrEmpty(Config);

	/// <summary>
	/// Gets the total number of configuration items
	/// </summary>
	public int TotalItems => (HasKernelArgs ? 1 : 0) + (HasConfig ? 1 : 0);

	/// <summary>
	/// Gets a formatted string representation of the kernel arguments
	/// </summary>
	public string GetKernelArgsString() => string.Join(" ", KernelArgs);

	/// <summary>
	/// Gets a summary of the configuration
	/// </summary>
	public string GetSummary()
	{
		var parts = new List<string>();

		if (HasKernelArgs)
		{
			parts.Add($"{KernelArgs.Count} kernel argument(s)");
		}

		if (HasConfig)
		{
			parts.Add($"config ({Config.Length} characters)");
		}

		return parts.Count > 0 ? string.Join(", ", parts) : "Empty configuration";
	}
}
