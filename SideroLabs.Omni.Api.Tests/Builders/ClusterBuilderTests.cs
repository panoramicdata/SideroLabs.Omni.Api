using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Builders;

public class ClusterBuilderTests
{
	[Fact]
	public void Build_WithValidConfiguration_CreatesCluster()
	{
		// Arrange
		var builder = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0");

		// Act
		var cluster = builder.Build();

		// Assert
		Assert.NotNull(cluster);
		Assert.Equal("test-cluster", cluster.Metadata.Id);
		Assert.Equal("v1.29.0", cluster.Spec.KubernetesVersion);
		Assert.Equal("v1.7.0", cluster.Spec.TalosVersion);
		Assert.Equal("Cluster", cluster.Kind);
		Assert.Equal("omni.sidero.dev/v1alpha1", cluster.ApiVersion);
	}

	[Fact]
	public void Build_WithoutKubernetesVersion_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new ClusterBuilder("test-cluster")
			.WithTalosVersion("v1.7.0");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	[Fact]
	public void Build_WithoutTalosVersion_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	[Fact]
	public void WithLabel_AddsLabel()
	{
		// Arrange & Act
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.WithLabel("environment", "production")
			.Build();

		// Assert
		Assert.NotNull(cluster.Metadata.Labels);
		Assert.True(cluster.Metadata.Labels.ContainsKey("environment"));
		Assert.Equal("production", cluster.Metadata.Labels["environment"]);
	}

	[Fact]
	public void WithLabels_AddsMultipleLabels()
	{
		// Arrange
		var labels = new Dictionary<string, string>
		{
			["environment"] = "production",
			["region"] = "us-west-2"
		};

		// Act
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.WithLabels(labels)
			.Build();

		// Assert
		Assert.NotNull(cluster.Metadata.Labels);
		Assert.Equal(2, cluster.Metadata.Labels.Count);
		Assert.Equal("production", cluster.Metadata.Labels["environment"]);
		Assert.Equal("us-west-2", cluster.Metadata.Labels["region"]);
	}

	[Fact]
	public void InNamespace_SetsNamespace()
	{
		// Arrange & Act
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.InNamespace("production")
			.Build();

		// Assert
		Assert.Equal("production", cluster.Metadata.Namespace);
	}

	[Fact]
	public void WithAnnotation_AddsAnnotation()
	{
		// Arrange & Act
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.WithAnnotation("description", "Test cluster")
			.Build();

		// Assert
		Assert.NotNull(cluster.Metadata.Annotations);
		Assert.True(cluster.Metadata.Annotations.ContainsKey("description"));
		Assert.Equal("Test cluster", cluster.Metadata.Annotations["description"]);
	}

	[Fact]
	public void ImplicitConversion_ConvertsToCluster()
	{
		// Arrange
		ClusterBuilder builder = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0");

		// Act
		Cluster cluster = builder;

		// Assert
		Assert.NotNull(cluster);
		Assert.Equal("test-cluster", cluster.Metadata.Id);
	}

	[Fact]
	public void Constructor_WithNullClusterName_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ClusterBuilder(null!));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyClusterName_ThrowsArgumentException(string clusterName)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ClusterBuilder(clusterName));
	}
}
