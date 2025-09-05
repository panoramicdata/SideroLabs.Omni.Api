using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
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
			options.AuthToken = "test-token";
		});

		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var omniClientOptions = serviceProvider.GetService<OmniClientOptions>();
		omniClientOptions.Should().NotBeNull();
		omniClientOptions!.Endpoint.Should().Be("https://test.example.com:8443");
		omniClientOptions.AuthToken.Should().Be("test-token");

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
			AuthToken = "test-token",
			TimeoutSeconds = 60
		};

		// Act
		services.AddOmniClient(options);

		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var registeredOptions = serviceProvider.GetService<OmniClientOptions>();
		registeredOptions.Should().NotBeNull();
		registeredOptions!.Endpoint.Should().Be("https://test.example.com:8443");
		registeredOptions.AuthToken.Should().Be("test-token");
		registeredOptions.TimeoutSeconds.Should().Be(60);

		var omniClient = serviceProvider.GetService<OmniClient>();
		omniClient.Should().NotBeNull();
	}
}
