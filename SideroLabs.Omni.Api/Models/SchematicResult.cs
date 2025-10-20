namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Result of creating a schematic for machine provisioning
/// </summary>
public class SchematicResult
{
	/// <summary>
	/// The generated schematic ID
	/// </summary>
	public required string SchematicId { get; init; }

	/// <summary>
	/// The PXE URL for network booting with this schematic
	/// </summary>
	public required string PxeUrl { get; init; }

	/// <summary>
	/// Whether gRPC tunnel is enabled for this schematic
	/// </summary>
	public required bool GrpcTunnelEnabled { get; init; }

	/// <summary>
	/// Returns a formatted string representation of the schematic result
	/// </summary>
	public override string ToString() =>
		$"Schematic: {SchematicId}, PXE URL: {PxeUrl}, gRPC Tunnel: {(GrpcTunnelEnabled ? "Enabled" : "Disabled")}";
}
