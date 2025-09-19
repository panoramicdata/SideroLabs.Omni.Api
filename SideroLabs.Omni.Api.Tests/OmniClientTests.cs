using AwesomeAssertions;
using SideroLabs.Omni.Api.Exceptions;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for the OmniClient class
/// Tests only the gRPC-based client functionality
/// </summary>
public class OmniClientTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Tests for the OmniClient constructor
	/// </summary>
	[Fact]
	public void Constructor_WithNullOptions_ThrowsArgumentNullException() =>
		// Act & Assert
		((Action)(() => _ = new OmniClient(null!))).Should().Throw<ArgumentNullException>();

	[Fact]
	public void Constructor_WithValidOptions_SetsProperties()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 60,
			UseTls = true,
			ValidateCertificate = false
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Endpoint.Should().Be("https://test.example.com");
		client.UseTls.Should().BeTrue();
		client.IsReadOnly.Should().BeFalse(); // Default value
		client.Management.Should().NotBeNull(); // Verify ManagementService is available
	}

	[Fact]
	public void Constructor_WithInvalidEndpoint_ThrowsOmniConfigurationException()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "", // Invalid endpoint
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"
		};

		// Act & Assert
		var exception = ((Action)(() => _ = new OmniClient(options))).Should().Throw<OmniConfigurationException>();
		exception.Which.ValidationErrors.Should().Contain("Endpoint is required");
	}

	[Fact]
	public void Constructor_WithInvalidTimeoutSeconds_ThrowsOmniConfigurationException()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = -1 // Invalid timeout
		};

		// Act & Assert
		var exception = ((Action)(() => _ = new OmniClient(options))).Should().Throw<OmniConfigurationException>();
		exception.Which.ValidationErrors.Should().Contain("TimeoutSeconds must be positive");
	}

	[Fact]
	public void ManagementService_IsNotNull()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Management.Should().NotBeNull();
		client.Management.Should().BeAssignableTo<Interfaces.IManagementService>();
	}

	[Fact]
	public void Properties_ReturnCorrectValues()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://omni.example.com:8443",
			Identity = "test-identity",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----",
			UseTls = true,
			IsReadOnly = true
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Endpoint.Should().Be("https://omni.example.com:8443");
		client.UseTls.Should().BeTrue();
		client.IsReadOnly.Should().BeTrue();
		// Note: Identity may be null if authenticator creation fails (e.g., invalid PGP key)
		// This is expected behavior for invalid credentials
	}

	[Fact]
	public void Dispose_DoesNotThrow()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"
		};

		var client = new OmniClient(options);

		// Act & Assert
		((Action)(() => client.Dispose())).Should().NotThrow();
	}

	[Fact]
	public void Constructor_WithoutCredentials_CreatesClientWithoutAuthenticator()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com"
			// No Identity or PgpPrivateKey provided
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Endpoint.Should().Be("https://test.example.com");
		client.Identity.Should().BeNull(); // No authenticator should be created
		client.Management.Should().NotBeNull(); // ManagementService should still be available
	}
}
