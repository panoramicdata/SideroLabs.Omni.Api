using AwesomeAssertions;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for the OmniClient class
/// </summary>
public class OmniClientTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Tests for the OmniClient constructor
	/// </summary>
	[Fact]
	public void Constructor_WithNullOptions_ThrowsArgumentNullException()
	{
		// Act & Assert
		((Action)(() => _ = new OmniClient(null!))).Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_WithValidOptions_SetsProperties()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com:8443",
			AuthToken = "test-token",
			UseTls = true
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Endpoint.Should().Be("https://test.example.com:8443");
		client.UseTls.Should().BeTrue();
	}

	[Fact]
	public async Task GetStatusAsync_ReturnsValidResponse()
	{
		// Arrange
		using var client = OmniClient;

		// Act
		var response = await client.GetStatusAsync(CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.Version.Should().NotBeNullOrEmpty();
		response.Ready.Should().BeTrue();
	}

	[Fact]
	public async Task ListClustersAsync_ReturnsValidResponse()
	{
		// Arrange
		using var client = OmniClient;

		// Act
		var response = await client.ListClustersAsync(CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.Clusters.Should().NotBeNull();
	}

	[Fact]
	public async Task CreateClusterAsync_WithValidInput_ReturnsCreatedCluster()
	{
		// Arrange
		using var client = OmniClient;
		var spec = new Models.ClusterSpec
		{
			KubernetesVersion = "v1.28.0",
			TalosVersion = "v1.5.0",
			Features = ["embedded-discovery-service"]
		};

		// Act
		var response = await client.CreateClusterAsync("test-cluster", spec, CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.Cluster.Should().NotBeNull();
		response.Cluster.Name.Should().Be("test-cluster");
		response.Cluster.Spec.Should().NotBeNull();
		response.Cluster.Spec.KubernetesVersion.Should().Be("v1.28.0");
		response.Cluster.Spec.TalosVersion.Should().Be("v1.5.0");
	}

	[Fact]
	public async Task GetClusterAsync_WithValidId_ReturnsCluster()
	{
		// Arrange
		using var client = OmniClient;
		const string clusterId = "test-cluster-id";

		// Act
		var response = await client.GetClusterAsync(clusterId, CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.Cluster.Should().NotBeNull();
		response.Cluster.Id.Should().Be(clusterId);
	}

	[Fact]
	public async Task ListMachinesAsync_WithValidClusterId_ReturnsMachines()
	{
		// Arrange
		using var client = OmniClient;
		const string clusterId = "test-cluster-id";

		// Act
		var response = await client.ListMachinesAsync(clusterId, CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.Machines.Should().NotBeNull();
	}

	[Fact]
	public async Task GetMachineAsync_WithValidId_ReturnsMachine()
	{
		// Arrange
		using var client = OmniClient;
		const string machineId = "test-machine-id";

		// Act
		var response = await client.GetMachineAsync(machineId, CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.Machine.Should().NotBeNull();
		response.Machine.Id.Should().Be(machineId);
	}
}
