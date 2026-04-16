using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Builders;

/// <summary>
/// Unit tests for the ClusterBuilder fluent API.
/// </summary>
public class ClusterBuilderTests
{
	/// <summary>
	/// Verifies that building with a valid Kubernetes and Talos version produces a correctly populated Cluster resource.
	/// </summary>
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

	/// <summary>
	/// Verifies that building without specifying a Kubernetes version throws InvalidOperationException.
	/// </summary>
	[Fact]
	public void Build_WithoutKubernetesVersion_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new ClusterBuilder("test-cluster")
			.WithTalosVersion("v1.7.0");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	/// <summary>
	/// Verifies that building without specifying a Talos version throws InvalidOperationException.
	/// </summary>
	[Fact]
	public void Build_WithoutTalosVersion_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	/// <summary>
	/// Verifies that a single label is added to cluster metadata.
	/// </summary>
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

	/// <summary>
	/// Verifies that a dictionary of labels is added to cluster metadata.
	/// </summary>
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

	/// <summary>
	/// Verifies that the specified namespace is set in cluster metadata.
	/// </summary>
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

	/// <summary>
	/// Verifies that a single annotation is added to cluster metadata.
	/// </summary>
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

	/// <summary>
	/// Verifies that a ClusterBuilder can be implicitly converted to a Cluster resource.
	/// </summary>
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

	/// <summary>
	/// Verifies that passing null as the cluster name throws ArgumentException.
	/// </summary>
	[Fact]
	public void Constructor_WithNullClusterName_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ClusterBuilder(null!));
	}

	/// <summary>
	/// Verifies that an empty or whitespace cluster name throws ArgumentException.
	/// </summary>
	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyClusterName_ThrowsArgumentException(string clusterName)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ClusterBuilder(clusterName));
	}
}
