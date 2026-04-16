using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Enums;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Management;

/// <summary>
/// Integration tests for ManagementService provisioning operations
/// Tests schematic creation and join token management
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ManagementService")]
[Trait("Category", "Provisioning")]
public class ManagementProvisioningTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Skipped test that creates a default Talos installer schematic and verifies the returned ID and PXE URL.
	/// </summary>
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_Default_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange & Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);
		Assert.Contains("schematic", result.PxeUrl, StringComparison.OrdinalIgnoreCase);

		Logger.LogInformation("✓ Created schematic:");
		Logger.LogInformation("  ID: {Id}", result.SchematicId);
		Logger.LogInformation("  PXE URL: {Url}", result.PxeUrl);
		Logger.LogInformation("  gRPC Tunnel: {Enabled}", result.GrpcTunnelEnabled);
	}

	/// <summary>
	/// Skipped test that creates a schematic with ISCSI and util-linux extensions.
	/// </summary>
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_WithExtensions_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var extensions = new[]
		{
			"siderolabs/iscsi-tools",
			"siderolabs/util-linux-tools"
		};

		// Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			extensions: extensions,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);
		Assert.Contains(result.SchematicId, result.PxeUrl);

		Logger.LogInformation("✓ Created schematic with extensions:");
		Logger.LogInformation("  Extensions: {Extensions}", string.Join(", ", extensions));
		Logger.LogInformation("  Schematic ID: {Id}", result.SchematicId);
	}

	/// <summary>
	/// Skipped test that creates a schematic with custom kernel arguments.
	/// </summary>
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_WithKernelArgs_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var kernelArgs = new[]
		{
			"console=ttyS0",
			"panic=10"
		};

		// Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			extensions: null,
			extraKernelArgs: kernelArgs,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);

		Logger.LogInformation("✓ Created schematic with kernel args:");
		Logger.LogInformation("  Kernel Args: {Args}", string.Join(" ", kernelArgs));
		Logger.LogInformation("  Schematic ID: {Id}", result.SchematicId);
	}

	/// <summary>
	/// Skipped test that creates a schematic with custom SMBIOS meta values.
	/// </summary>
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_WithMetaValues_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var metaValues = new Dictionary<uint, string>
		{
			{ 0x0a, "rack-1" },
			{ 0x0b, "datacenter-west" }
		};

		// Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			extensions: null,
			extraKernelArgs: null,
			metaValues: metaValues,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);

		Logger.LogInformation("✓ Created schematic with meta values:");
		Logger.LogInformation("  Schematic ID: {Id}", result.SchematicId);
	}

	/// <summary>
	/// Skipped test that creates a schematic with extensions, kernel args, and meta values combined.
	/// </summary>
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_WithAllOptions_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var extensions = new[] { "siderolabs/iscsi-tools" };
		var kernelArgs = new[] { "console=ttyS0" };
		var metaValues = new Dictionary<uint, string> { { 0x0a, "test" } };

		// Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			extensions: extensions,
			extraKernelArgs: kernelArgs,
			metaValues: metaValues,
			talosVersion: "v1.6.0",
			mediaId: null,
			secureBoot: false,
			siderolinkGrpcTunnelMode: SiderolinkGrpcTunnelMode.Auto,
			joinToken: null,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);

		Logger.LogInformation("✓ Created comprehensive schematic:");
		Logger.LogInformation("  ID: {Id}", result.SchematicId);
		Logger.LogInformation("  URL: {Url}", result.PxeUrl);
		Logger.LogInformation("  gRPC Tunnel: {Enabled}", result.GrpcTunnelEnabled);
	}

	/// <summary>
	/// Verifies that creating a schematic with secure boot enabled returns valid schematic information.
	/// </summary>
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_WithSecureBoot_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange & Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			extensions: null,
			extraKernelArgs: null,
			metaValues: null,
			talosVersion: null,
			mediaId: null,
			secureBoot: true,
			siderolinkGrpcTunnelMode: SiderolinkGrpcTunnelMode.Auto,
			joinToken: null,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Logger.LogInformation("✓ Created schematic with secure boot enabled: {Id}", result.SchematicId);
	}

	/// <summary>
	/// Verifies that creating a schematic with the specified gRPC tunnel mode returns valid schematic information.
	/// </summary>
	/// <param name="tunnelMode">The gRPC tunnel mode to use when creating the schematic.</param>
	[Theory(Skip = "Creates schematics - manual test only")]
	[InlineData(SiderolinkGrpcTunnelMode.Auto)]
	[InlineData(SiderolinkGrpcTunnelMode.Enabled)]
	[InlineData(SiderolinkGrpcTunnelMode.Disabled)]
	public async Task CreateSchematic_WithGrpcTunnelMode_ReturnsSchematicInfo(
		SiderolinkGrpcTunnelMode tunnelMode)
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange & Act - NEW API!
		var result = await OmniClient.Schematics.CreateAsync(
			extensions: null,
			extraKernelArgs: null,
			metaValues: null,
			talosVersion: null,
			mediaId: null,
			secureBoot: false,
			siderolinkGrpcTunnelMode: tunnelMode,
			joinToken: null,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Logger.LogInformation("✓ Created schematic with tunnel mode {Mode}: gRPC Tunnel={Enabled}",
			tunnelMode, result.GrpcTunnelEnabled);
	}

	/// <summary>
	/// Verifies that creating a join token with valid parameters returns a non-empty token ID.
	/// </summary>
	[Fact(Skip = "Creates join token - manual test only")]
	public async Task CreateJoinToken_ValidParameters_ReturnsTokenId()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var tokenName = $"test-token-{GenerateUniqueId()}";
		var expiration = DateTimeOffset.UtcNow.AddHours(24);

		// Act - NEW API!
		var tokenId = await OmniClient.MachineManagement.CreateJoinTokenAsync(
			tokenName,
			expiration,
			CancellationToken);

		// Assert
		Assert.NotEmpty(tokenId);
		Logger.LogInformation("✓ Created join token:");
		Logger.LogInformation("  Name: {Name}", tokenName);
		Logger.LogInformation("  Token ID: {TokenId}", tokenId);
		Logger.LogInformation("  Expires: {Expiration}", expiration);

		// Note: Would need to delete the token in cleanup
	}

	/// <summary>
	/// Verifies that creating a join token with a short TTL (5 minutes) returns a valid token ID.
	/// </summary>
	[Fact(Skip = "Creates join token - manual test only")]
	public async Task CreateJoinToken_ShortTTL_ReturnsTokenId()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var tokenName = $"test-short-ttl-{GenerateUniqueId()}";
		var expiration = DateTimeOffset.UtcNow.AddMinutes(5);

		// Act - NEW API!
		var tokenId = await OmniClient.MachineManagement.CreateJoinTokenAsync(
			tokenName,
			expiration,
			CancellationToken);

		// Assert
		Assert.NotEmpty(tokenId);
		Logger.LogInformation("✓ Created short-lived join token (5 min): {TokenId}", tokenId);
	}

	/// <summary>
	/// Verifies that creating a join token with an expiration time in the past throws an <see cref="StatusCode.InvalidArgument"/> gRPC error.
	/// </summary>
	[Fact]
	public async Task CreateJoinToken_PastExpiration_ThrowsInvalidArgument()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var tokenName = "test-expired";
		var pastExpiration = DateTimeOffset.UtcNow.AddHours(-1); // In the past

		// Act & Assert - NEW API!
		var exception = await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.MachineManagement.CreateJoinTokenAsync(tokenName, pastExpiration, CancellationToken));

		Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
		Logger.LogInformation("✓ Past expiration time correctly rejected");
	}

	/// <summary>
	/// Verifies that creating a join token with an empty name throws an <see cref="StatusCode.InvalidArgument"/> gRPC error.
	/// </summary>
	[Fact]
	public async Task CreateJoinToken_EmptyName_ThrowsInvalidArgument()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var emptyName = "";
		var expiration = DateTimeOffset.UtcNow.AddHours(1);

		// Act & Assert - NEW API!
		await Assert.ThrowsAnyAsync<Exception>(async () =>
			await OmniClient.MachineManagement.CreateJoinTokenAsync(emptyName, expiration, CancellationToken));

		Logger.LogInformation("✓ Empty token name correctly rejected");
	}

	/// <summary>
	/// Verifies that retrieving the machine join configuration with a valid token returns a non-null config.
	/// </summary>
	[Fact(Skip = "Requires valid join token - manual test only")]
	public async Task GetMachineJoinConfig_ValidToken_ReturnsConfig()
	{
		// This test requires a valid join token created first
		// var joinToken = "valid-token-id";

		// NEW API!
		// var config = await OmniClient.MachineManagement.GetJoinConfigAsync(
		//     useGrpcTunnel: true,
		//     joinToken: joinToken,
		//     CancellationToken);

		// Assert.NotNull(config);
		// Assert.NotEmpty(config.Config);
		// Assert.NotNull(config.KernelArgs);

		Logger.LogInformation("Join config test requires valid token");
	}

	/// <summary>
	/// Verifies that retrieving the machine join configuration with an invalid token throws an <see cref="RpcException"/>.
	/// </summary>
	[Fact]
	public async Task GetMachineJoinConfig_InvalidToken_ThrowsException()
	{
		// Arrange
		var invalidToken = "invalid-token-that-does-not-exist";

		// Act & Assert - NEW API!
		await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.MachineManagement.GetJoinConfigAsync(
				useGrpcTunnel: true,
				joinToken: invalidToken,
				CancellationToken));

		Logger.LogInformation("✓ Invalid join token correctly rejected");
	}
}
