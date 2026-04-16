using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

/// <summary>
/// Unit tests for ExtensionsConfigurationValidator FluentValidation rules.
/// </summary>
public class ExtensionsConfigurationValidatorTests
{
	/// <summary>
	/// Verifies that a fully populated valid ExtensionsConfiguration passes all validation rules.
	/// </summary>
	[Fact]
	public void Validate_WithValidConfiguration_ReturnsSuccess()
	{
		// Arrange
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.ForTalosVersion("v1.7.0")
			.Build();

		// Act
		var result = config.Validate();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	/// <summary>
	/// Verifies that an ExtensionsConfiguration with an empty extension list fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithoutExtensions_ReturnsError()
	{
		// Arrange
		var config = new ExtensionsConfiguration
		{
			Metadata = new ResourceMetadata { Id = "test-config", Namespace = "default" },
			Spec = new ExtensionsConfigurationSpec
			{
				Extensions = []
			}
		};

		// Act
		var result = config.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Spec.Extensions");
	}

	/// <summary>
	/// Verifies that an ExtensionsConfiguration containing an empty extension name string fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithEmptyExtensionName_ReturnsError()
	{
		// Arrange
		var config = new ExtensionsConfiguration
		{
			Metadata = new ResourceMetadata { Id = "test-config", Namespace = "default" },
			Spec = new ExtensionsConfigurationSpec
			{
				Extensions = [""]
			}
		};

		// Act
		var result = config.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName.Contains("Extensions") && 
			e.ErrorMessage.Contains("empty"));
	}

	/// <summary>
	/// Verifies that a configuration ID containing uppercase letters or underscores fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidConfigId_ReturnsError()
	{
		// Arrange
		var config = new ExtensionsConfiguration
		{
			Metadata = new ResourceMetadata { Id = "Test_Config", Namespace = "default" }, // Invalid: uppercase and underscore
			Spec = new ExtensionsConfigurationSpec
			{
				Extensions = ["siderolabs/util-linux-tools"]
			}
		};

		// Act
		var result = config.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Metadata.Id" && 
			e.ErrorMessage.Contains("DNS-1123"));
	}

	/// <summary>
	/// Verifies that a Talos version missing the 'v' prefix fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidTalosVersion_ReturnsError()
	{
		// Arrange
		var config = new ExtensionsConfiguration
		{
			Metadata = new ResourceMetadata { Id = "test-config", Namespace = "default" },
			Spec = new ExtensionsConfigurationSpec
			{
				Extensions = ["siderolabs/util-linux-tools"],
				TalosVersion = "1.7.0" // Missing 'v' prefix
			}
		};

		// Act
		var result = config.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Spec.TalosVersion" && 
			e.ErrorMessage.Contains("vX.Y.Z"));
	}

	/// <summary>
	/// Verifies that ValidateAndThrow throws ValidationException for an invalid ExtensionsConfiguration.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithInvalidConfiguration_ThrowsValidationException()
	{
		// Arrange
		var config = new ExtensionsConfiguration
		{
			Metadata = new ResourceMetadata { Id = "test-config", Namespace = "default" },
			Spec = new ExtensionsConfigurationSpec
			{
				Extensions = [] // Empty list
			}
		};

		// Act & Assert
		Assert.Throws<ValidationException>(() => config.ValidateAndThrow());
	}

	/// <summary>
	/// Verifies that ValidateAndThrow does not throw for a fully valid ExtensionsConfiguration.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithValidConfiguration_DoesNotThrow()
	{
		// Arrange
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.ForTalosVersion("v1.7.0")
			.Build();

		// Act & Assert (should not throw)
		config.ValidateAndThrow();
	}

	/// <summary>
	/// Verifies that an extensions configuration containing a versioned extension reference passes validation successfully.
	/// </summary>
	[Fact]
	public void Validate_WithVersionedExtension_ReturnsSuccess()
	{
		// Arrange
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools@v2.13.1")
			.Build();

		// Act
		var result = config.Validate();

		// Assert
		Assert.True(result.IsValid);
	}

	/// <summary>
	/// Verifies that an extensions configuration containing multiple extension references passes validation successfully.
	/// </summary>
	[Fact]
	public void Validate_WithMultipleExtensions_ReturnsSuccess()
	{
		// Arrange
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtensions(
				"siderolabs/util-linux-tools",
				"siderolabs/qemu-guest-agent",
				"siderolabs/iscsi-tools")
			.ForTalosVersion("v1.7.0")
			.Build();

		// Act
		var result = config.Validate();

		// Assert
		Assert.True(result.IsValid);
	}
}
