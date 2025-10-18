using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

public class ExtensionsConfigurationValidatorTests
{
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
