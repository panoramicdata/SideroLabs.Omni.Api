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
	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_Default_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange & Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);
		Assert.Contains("schematic", result.PxeUrl, StringComparison.OrdinalIgnoreCase);

		Logger.LogInformation("✓ Created schematic:");
		Logger.LogInformation("  ID: {Id}", result.SchematicId);
		Logger.LogInformation("  PXE URL: {Url}", result.PxeUrl);
		Logger.LogInformation("  gRPC Tunnel: {Enabled}", result.GrpcTunnelEnabled);
	}

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

		// Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			extensions,
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);
		Assert.Contains(result.SchematicId, result.PxeUrl);

		Logger.LogInformation("✓ Created schematic with extensions:");
		Logger.LogInformation("  Extensions: {Extensions}", string.Join(", ", extensions));
		Logger.LogInformation("  Schematic ID: {Id}", result.SchematicId);
	}

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

		// Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			extensions: null,
			extraKernelArgs: kernelArgs,
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);

		Logger.LogInformation("✓ Created schematic with kernel args:");
		Logger.LogInformation("  Kernel Args: {Args}", string.Join(" ", kernelArgs));
		Logger.LogInformation("  Schematic ID: {Id}", result.SchematicId);
	}

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

		// Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			extensions: null,
			extraKernelArgs: null,
			metaValues: metaValues,
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);

		Logger.LogInformation("✓ Created schematic with meta values:");
		Logger.LogInformation("  Schematic ID: {Id}", result.SchematicId);
	}

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

		// Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			extensions,
			kernelArgs,
			metaValues,
			talosVersion: "v1.6.0",
			mediaId: null,
			secureBoot: false,
			siderolinkGrpcTunnelMode: SiderolinkGrpcTunnelMode.Auto,
			joinToken: null,
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Assert.NotEmpty(result.PxeUrl);

		Logger.LogInformation("✓ Created comprehensive schematic:");
		Logger.LogInformation("  ID: {Id}", result.SchematicId);
		Logger.LogInformation("  URL: {Url}", result.PxeUrl);
		Logger.LogInformation("  gRPC Tunnel: {Enabled}", result.GrpcTunnelEnabled);
	}

	[Fact(Skip = "Creates schematic - manual test only")]
	public async Task CreateSchematic_WithSecureBoot_ReturnsSchematicInfo()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange & Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			extensions: null,
			extraKernelArgs: null,
			metaValues: null,
			talosVersion: null,
			mediaId: null,
			secureBoot: true,
			siderolinkGrpcTunnelMode: SiderolinkGrpcTunnelMode.Auto,
			joinToken: null,
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Logger.LogInformation("✓ Created schematic with secure boot enabled: {Id}", result.SchematicId);
	}

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

		// Arrange & Act
		var result = await OmniClient.Management.CreateSchematicAsync(
			extensions: null,
			extraKernelArgs: null,
			metaValues: null,
			talosVersion: null,
			mediaId: null,
			secureBoot: false,
			siderolinkGrpcTunnelMode: tunnelMode,
			joinToken: null,
			CancellationToken);

		// Assert
		Assert.NotEmpty(result.SchematicId);
		Logger.LogInformation("✓ Created schematic with tunnel mode {Mode}: gRPC Tunnel={Enabled}",
			tunnelMode, result.GrpcTunnelEnabled);
	}

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

		// Act
		var tokenId = await OmniClient.Management.CreateJoinTokenAsync(
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

		// Act
		var tokenId = await OmniClient.Management.CreateJoinTokenAsync(
			tokenName,
			expiration,
			CancellationToken);

		// Assert
		Assert.NotEmpty(tokenId);
		Logger.LogInformation("✓ Created short-lived join token (5 min): {TokenId}", tokenId);
	}

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

		// Act & Assert
		var exception = await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.Management.CreateJoinTokenAsync(tokenName, pastExpiration, CancellationToken));

		Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
		Logger.LogInformation("✓ Past expiration time correctly rejected");
	}

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

		// Act & Assert
		await Assert.ThrowsAnyAsync<Exception>(async () =>
			await OmniClient.Management.CreateJoinTokenAsync(emptyName, expiration, CancellationToken));

		Logger.LogInformation("✓ Empty token name correctly rejected");
	}

	[Fact(Skip = "Requires valid join token - manual test only")]
	public async Task GetMachineJoinConfig_ValidToken_ReturnsConfig()
	{
		// This test requires a valid join token created first
		// var joinToken = "valid-token-id";

		// var config = await OmniClient.Management.GetMachineJoinConfigAsync(
		//     useGrpcTunnel: true,
		//     joinToken: joinToken,
		//     CancellationToken);

		// Assert.NotNull(config);
		// Assert.NotEmpty(config.Config);
		// Assert.NotNull(config.KernelArgs);

		Logger.LogInformation("Join config test requires valid token");
	}

	[Fact]
	public async Task GetMachineJoinConfig_InvalidToken_ThrowsException()
	{
		// Arrange
		var invalidToken = "invalid-token-that-does-not-exist";

		// Act & Assert
		await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.Management.GetMachineJoinConfigAsync(
				useGrpcTunnel: true,
				joinToken: invalidToken,
				CancellationToken));

		Logger.LogInformation("✓ Invalid join token correctly rejected");
	}
}
