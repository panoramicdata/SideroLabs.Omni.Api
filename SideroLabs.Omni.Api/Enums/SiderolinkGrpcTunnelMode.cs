namespace SideroLabs.Omni.Api.Enums;

/// <summary>
/// Siderolink gRPC tunnel mode for schematic creation
/// Corresponds to the SiderolinkGRPCTunnelMode enum in management.proto
/// </summary>
public enum SiderolinkGrpcTunnelMode
{
	/// <summary>
	/// Automatically determine whether to use gRPC tunnel
	/// </summary>
	Auto = 0,

	/// <summary>
	/// Disable gRPC tunnel
	/// </summary>
	Disabled = 1,

	/// <summary>
	/// Enable gRPC tunnel
	/// </summary>
	Enabled = 2
}
