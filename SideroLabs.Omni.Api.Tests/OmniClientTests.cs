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
		// Arrange - Use the test client which has valid credentials
		// The TestBase already creates a properly configured client
		var client = OmniClient;

		// Assert
		client.BaseUrl.Should().NotBeNull();
		client.UseTls.Should().BeTrue();
		client.IsReadOnly.Should().BeFalse(); // Admin account has write access
		
		// Verify new focused services are available
		client.KubeConfig.Should().NotBeNull();
		client.TalosConfig.Should().NotBeNull();
		client.OmniConfig.Should().NotBeNull();
		client.ServiceAccounts.Should().NotBeNull();
		client.Validation.Should().NotBeNull();
		client.Kubernetes.Should().NotBeNull();
		client.Schematics.Should().NotBeNull();
		client.MachineManagement.Should().NotBeNull();
		client.Support.Should().NotBeNull();
		
		// Verify resource-specific operations are available
		client.Machines.Should().NotBeNull();
		client.Clusters.Should().NotBeNull();
		client.ClusterMachines.Should().NotBeNull();
	}

	[Fact]
	public void Constructor_WithInvalidTimeoutSeconds_ThrowsOmniConfigurationException()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.example.com"),
			AuthToken = "test-token", // Use AuthToken instead of PGP key
			TimeoutSeconds = -1 // Invalid timeout
		};

		// Act & Assert
		var exception = ((Action)(() => _ = new OmniClient(options))).Should().Throw<OmniConfigurationException>();
		exception.Which.ValidationErrors.Should().Contain("TimeoutSeconds must be positive");
	}

	[Fact]
	public void FocusedServices_AreNotNull()
	{
		// Arrange - Use the test client from TestBase
		var client = OmniClient;

		// Assert - Configuration services
		client.KubeConfig.Should().NotBeNull();
		client.KubeConfig.Should().BeAssignableTo<Interfaces.IKubeConfigService>();
		
		client.TalosConfig.Should().NotBeNull();
		client.TalosConfig.Should().BeAssignableTo<Interfaces.ITalosConfigService>();
		
		client.OmniConfig.Should().NotBeNull();
		client.OmniConfig.Should().BeAssignableTo<Interfaces.IOmniConfigService>();

		// Assert - Management services
		client.ServiceAccounts.Should().NotBeNull();
		client.ServiceAccounts.Should().BeAssignableTo<Interfaces.IServiceAccountService>();
		
		client.Validation.Should().NotBeNull();
		client.Validation.Should().BeAssignableTo<Interfaces.IValidationService>();
		
		client.Kubernetes.Should().NotBeNull();
		client.Kubernetes.Should().BeAssignableTo<Interfaces.IKubernetesService>();
		
		client.Schematics.Should().NotBeNull();
		client.Schematics.Should().BeAssignableTo<Interfaces.ISchematicService>();
		
		client.MachineManagement.Should().NotBeNull();
		client.MachineManagement.Should().BeAssignableTo<Interfaces.IMachineService>();
		
		client.Support.Should().NotBeNull();
		client.Support.Should().BeAssignableTo<Interfaces.ISupportService>();
	}

	[Fact]
	public void ResourceOperations_AreNotNull()
	{
		// Arrange - Use the test client from TestBase
		var client = OmniClient;

		// Assert - Resource-specific operations
		client.Machines.Should().NotBeNull();
		client.Machines.Should().BeAssignableTo<Interfaces.IMachineOperations>();
		
		client.Clusters.Should().NotBeNull();
		client.Clusters.Should().BeAssignableTo<Interfaces.IClusterOperations>();
		
		client.ClusterMachines.Should().NotBeNull();
		client.ClusterMachines.Should().BeAssignableTo<Interfaces.IClusterMachineOperations>();
		
		client.MachineSets.Should().NotBeNull();
		client.MachineSets.Should().BeAssignableTo<Interfaces.IMachineSetOperations>();
		
		client.MachineSetNodes.Should().NotBeNull();
		client.MachineSetNodes.Should().BeAssignableTo<Interfaces.IMachineSetNodeOperations>();
	}

	[Fact]
	public void LegacyManagementService_StillWorks()
	{
		// Arrange - Use the test client from TestBase
		var client = OmniClient;

		// Assert - Legacy Management service still works (but is deprecated)
#pragma warning disable CS0618 // Type or member is obsolete
		client.Management.Should().NotBeNull();
		client.Management.Should().BeAssignableTo<Interfaces.IManagementService>();
#pragma warning restore CS0618 // Type or member is obsolete
	}

	[Fact]
	public void Properties_ReturnCorrectValues()
	{
		// Arrange - Use the test client from TestBase
		var client = OmniClient;

		// Assert
		client.BaseUrl.Should().NotBeNull();
		client.BaseUrl.ToString().Should().NotBeNullOrEmpty();
		client.UseTls.Should().BeTrue();
		client.IsReadOnly.Should().BeFalse(); // Admin account has write access
		client.Identity.Should().Be("nuget-dev"); // From test config
	}

	[Fact]
	public void Dispose_DoesNotThrow()
	{
		// Arrange - Create a temporary client with auth token (simpler than PGP key)
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.omni.siderolabs.io"),
			AuthToken = "test-token",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = false
		};

		var client = new OmniClient(options);

		// Act & Assert
		((Action)(() => client.Dispose())).Should().NotThrow();
	}

	[Fact]
	public void Constructor_WithoutCredentials_ThrowsOmniConfigurationException()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.example.com")
			// No Identity, PgpPrivateKey, or AuthToken provided
		};

		// Act & Assert
		// Constructing this should fail with an OmniConfigurationException due to missing credentials
		var exception = ((Action)(() => _ = new OmniClient(options))).Should().Throw<OmniConfigurationException>();
		exception.Which.ValidationErrors.Should().Contain("One of PgpPrivateKey, PgpKeyFilePath, or AuthToken must be provided for authentication");
	}

	[Fact]
	public void Constructor_WithAuthToken_CreatesClientSuccessfully()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.omni.siderolabs.io"),
			AuthToken = "test-auth-token",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = false
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Should().NotBeNull();
		client.BaseUrl.Should().Be("https://test.omni.siderolabs.io/");
		client.UseTls.Should().BeTrue();
		client.IsReadOnly.Should().BeFalse(); // Default when not specified
	}

	[Fact]
	public void Constructor_WithReadOnlyMode_SetsIsReadOnlyCorrectly()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test.omni.siderolabs.io"),
			AuthToken = "test-auth-token",
			IsReadOnly = true,
			TimeoutSeconds = 30
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.IsReadOnly.Should().BeTrue();
	}
}
