using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SideroLabs.Omni.Api.Extensions;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for the ServiceCollectionExtensions class
/// </summary>
public class ServiceCollectionExtensionsTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public void AddOmniClient_WithConfigureAction_RegistersServices()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddOmniClient(options =>
		{
			options.Endpoint = "https://test.example.com:8443";
			options.Identity = "test-user";
			options.PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----";
		});

		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var omniClientOptions = serviceProvider.GetRequiredService<IOptions<OmniClientOptions>>().Value;
		omniClientOptions.Should().NotBeNull();
		omniClientOptions.Endpoint.Should().Be("https://test.example.com:8443");
		omniClientOptions.Identity.Should().Be("test-user");
		omniClientOptions.PgpPrivateKey.Should().NotBeNullOrEmpty();

		var omniClient = serviceProvider.GetService<OmniClient>();
		omniClient.Should().NotBeNull();
	}

	[Fact]
	public void AddOmniClient_WithOptionsInstance_RegistersServices()
	{
		// Arrange
		var services = new ServiceCollection();
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com:8443",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 60
		};

		// Act
		services.AddOmniClient(options);

		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var registeredOptions = serviceProvider.GetRequiredService<IOptions<OmniClientOptions>>().Value;
		registeredOptions.Should().NotBeNull();
		registeredOptions.Endpoint.Should().Be("https://test.example.com:8443");
		registeredOptions.Identity.Should().Be("test-user");
		registeredOptions.PgpPrivateKey.Should().NotBeNullOrEmpty();
		registeredOptions.TimeoutSeconds.Should().Be(60);

		var omniClient = serviceProvider.GetService<OmniClient>();
		omniClient.Should().NotBeNull();
	}

	[Fact]
	public void AddOmniClient_WithAction_ConfiguresOptions()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddOmniClient(options =>
		{
			options.Endpoint = "https://test.example.com";
			options.Identity = "test-user";
			options.PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----";
		});

		var serviceProvider = services.BuildServiceProvider();
		var omniClientOptions = serviceProvider.GetRequiredService<IOptions<OmniClientOptions>>().Value;

		// Assert
		omniClientOptions.Endpoint.Should().Be("https://test.example.com");
		omniClientOptions.Identity.Should().Be("test-user");
		omniClientOptions.PgpPrivateKey.Should().NotBeNullOrEmpty();
	}
}
