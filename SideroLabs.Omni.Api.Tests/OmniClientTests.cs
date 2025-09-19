using AwesomeAssertions;
using SideroLabs.Omni.Api.Models;
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
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----",
			TimeoutSeconds = 60,
			UseTls = true,
			ValidateCertificate = false
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.Endpoint.Should().Be("https://test.example.com:8443");
		client.UseTls.Should().BeTrue();
	}

	[Fact]
	public async Task ListClustersAsync_ReturnsClusterList()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com:8443",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"
		};

		using var client = new OmniClient(options);

		// Act
		var result = await client.Clusters.ListClustersAsync(CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Clusters.Should().NotBeNull();
		result.Clusters.Should().HaveCountGreaterThan(0);
	}

	[Fact]
	public async Task CreateClusterAsync_WithValidInput_ReturnsCreatedCluster()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com:8443",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"
		};

		using var client = new OmniClient(options);

		var clusterSpec = new ClusterSpec
		{
			KubernetesVersion = "v1.28.0",
			TalosVersion = "v1.5.0"
		};

		// Act
		var result = await client.Clusters.CreateClusterAsync("test-cluster", clusterSpec, CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Cluster.Should().NotBeNull();
		result.Cluster.Name.Should().Be("test-cluster");
		result.Cluster.Spec.Should().BeEquivalentTo(clusterSpec);
	}

	[Fact]
	public async Task GetStatusAsync_ReturnsStatus()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com:8443",
			Identity = "test-user",
			PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"
		};

		using var client = new OmniClient(options);

		// Act
		var result = await client.Status.GetStatusAsync(CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Ready.Should().BeTrue();
		result.Version.Should().NotBeNullOrEmpty();
	}
}
