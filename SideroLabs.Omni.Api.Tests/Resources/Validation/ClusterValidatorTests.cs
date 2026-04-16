using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

/// <summary>
/// Unit tests for ClusterValidator FluentValidation rules.
/// </summary>
public class ClusterValidatorTests
{
	/// <summary>
	/// Verifies that a fully populated valid Cluster passes all validation rules.
	/// </summary>
	[Fact]
	public void Validate_WithValidCluster_ReturnsSuccess()
	{
		// Arrange
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		// Act
		var result = cluster.Validate();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	/// <summary>
	/// Verifies that a Cluster missing a Kubernetes version fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithoutKubernetesVersion_ReturnsError()
	{
		// Arrange
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Id = "test-cluster", Namespace = "default" },
			Spec = new ClusterSpec
			{
				TalosVersion = "v1.7.0"
			}
		};

		// Act
		var result = cluster.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Spec.KubernetesVersion");
	}

	/// <summary>
	/// Verifies that a Kubernetes version missing the 'v' prefix (e.g. '1.29.0') fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidKubernetesVersionFormat_ReturnsError()
	{
		// Arrange
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Id = "test-cluster", Namespace = "default" },
			Spec = new ClusterSpec
			{
				KubernetesVersion = "1.29.0", // Missing 'v' prefix
				TalosVersion = "v1.7.0"
			}
		};

		// Act
		var result = cluster.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Spec.KubernetesVersion" && 
			e.ErrorMessage.Contains("vX.Y.Z"));
	}

	/// <summary>
	/// Verifies that a cluster ID containing uppercase letters or underscores fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidClusterId_ReturnsError()
	{
		// Arrange
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Id = "Test_Cluster", Namespace = "default" }, // Invalid: uppercase and underscore
			Spec = new ClusterSpec
			{
				KubernetesVersion = "v1.29.0",
				TalosVersion = "v1.7.0"
			}
		};

		// Act
		var result = cluster.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Metadata.Id" && 
			e.ErrorMessage.Contains("DNS-1123"));
	}

	/// <summary>
	/// Verifies that a Cluster missing a Talos version fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithoutTalosVersion_ReturnsError()
	{
		// Arrange
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Id = "test-cluster", Namespace = "default" },
			Spec = new ClusterSpec
			{
				KubernetesVersion = "v1.29.0"
			}
		};

		// Act
		var result = cluster.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Spec.TalosVersion");
	}

	/// <summary>
	/// Verifies that ValidateAndThrow throws ValidationException for an invalid Cluster.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithInvalidCluster_ThrowsValidationException()
	{
		// Arrange
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Id = "test-cluster", Namespace = "default" },
			Spec = new ClusterSpec
			{
				KubernetesVersion = "v1.29.0"
				// Missing TalosVersion
			}
		};

		// Act & Assert
		Assert.Throws<ValidationException>(() => cluster.ValidateAndThrow());
	}

	/// <summary>
	/// Verifies that ValidateAndThrow does not throw for a fully valid Cluster.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithValidCluster_DoesNotThrow()
	{
		// Arrange
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		// Act & Assert (should not throw)
		cluster.ValidateAndThrow();
	}
}
