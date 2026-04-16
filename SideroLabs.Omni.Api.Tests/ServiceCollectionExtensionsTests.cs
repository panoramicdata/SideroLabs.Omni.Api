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
	/// <summary>
	/// Verifies that AddOmniClient with a configure action registers OmniClientOptions and OmniClient in the service container.
	/// </summary>
	[Fact]
	public void AddOmniClient_WithConfigureAction_RegistersServices()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddOmniClient(options =>
		{
			options.BaseUrl = new("https://test.example.com:8443");
			options.Identity = "test-user";
			options.PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----";
		});

		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var omniClientOptions = serviceProvider.GetRequiredService<IOptions<OmniClientOptions>>().Value;
		omniClientOptions.Should().NotBeNull();
		omniClientOptions.BaseUrl.ToString().Should().Be("https://test.example.com:8443/");
		omniClientOptions.Identity.Should().Be("test-user");
		omniClientOptions.PgpPrivateKey.Should().NotBeNullOrEmpty();

		var omniClient = serviceProvider.GetService<OmniClient>();
		omniClient.Should().NotBeNull();
	}

	/// <summary>
	/// Verifies that AddOmniClient with an options instance registers all settings including custom timeout.
	/// </summary>
	[Fact]
	public void AddOmniClient_WithOptionsInstance_RegistersServices()
	{
		// Arrange
		var services = new ServiceCollection();
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.example.com:8443"),
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
		registeredOptions.BaseUrl.ToString().Should().Be("https://test.example.com:8443/");
		registeredOptions.Identity.Should().Be("test-user");
		registeredOptions.PgpPrivateKey.Should().NotBeNullOrEmpty();
		registeredOptions.TimeoutSeconds.Should().Be(60);

		var omniClient = serviceProvider.GetService<OmniClient>();
		omniClient.Should().NotBeNull();
	}

	/// <summary>
	/// Verifies that options configured via the action delegate are correctly bound to the registered OmniClientOptions.
	/// </summary>
	[Fact]
	public void AddOmniClient_WithAction_ConfiguresOptions()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddOmniClient(options =>
		{
			options.BaseUrl = new("https://test.example.com");
			options.Identity = "test-user";
			options.PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----";
		});

		var serviceProvider = services.BuildServiceProvider();
		var omniClientOptions = serviceProvider.GetRequiredService<IOptions<OmniClientOptions>>().Value;

		// Assert
		omniClientOptions.BaseUrl.Should().Be("https://test.example.com");
		omniClientOptions.Identity.Should().Be("test-user");
		omniClientOptions.PgpPrivateKey.Should().NotBeNullOrEmpty();
	}
}
