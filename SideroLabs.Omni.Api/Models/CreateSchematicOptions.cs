using SideroLabs.Omni.Api.Enums;

namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Options for creating a schematic for machine provisioning
/// </summary>
public class CreateSchematicOptions
{
	/// <summary>
	/// Extensions to include
	/// </summary>
	public string[]? Extensions { get; init; }

	/// <summary>
	/// Extra kernel arguments
	/// </summary>
	public string[]? ExtraKernelArgs { get; init; }

	/// <summary>
	/// Meta values mapping
	/// </summary>
	public Dictionary<uint, string>? MetaValues { get; init; }

	/// <summary>
	/// Talos version to use
	/// </summary>
	public string? TalosVersion { get; init; }

	/// <summary>
	/// Media ID for the installation media
	/// </summary>
	public string? MediaId { get; init; }

	/// <summary>
	/// Whether to enable secure boot
	/// </summary>
	public bool SecureBoot { get; init; }

	/// <summary>
	/// Siderolink gRPC tunnel mode
	/// </summary>
	public SiderolinkGrpcTunnelMode SiderolinkGrpcTunnelMode { get; init; } = SiderolinkGrpcTunnelMode.Auto;

	/// <summary>
	/// Join token for the machine
	/// </summary>
	public string? JoinToken { get; init; }
}