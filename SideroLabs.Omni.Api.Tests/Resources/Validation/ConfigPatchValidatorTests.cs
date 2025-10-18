using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

public class ConfigPatchValidatorTests
{
	[Fact]
	public void Validate_WithValidConfigPatch_ReturnsSuccess()
	{
		// Arrange
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node")
			.Build();

		// Act
		var result = configPatch.Validate();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithoutData_ReturnsError()
	{
		// Arrange
		var configPatch = new ConfigPatch
		{
			Metadata = new ResourceMetadata { Id = "test-patch", Namespace = "default" },
			Spec = new ConfigPatchSpec()
		};

		// Act
		var result = configPatch.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Spec.Data");
	}

	[Fact]
	public void Validate_WithInvalidYamlData_ReturnsError()
	{
		// Arrange
		var configPatch = new ConfigPatch
		{
			Metadata = new ResourceMetadata { Id = "test-patch", Namespace = "default" },
			Spec = new ConfigPatchSpec
			{
				Data = "invalid: yaml: data: [unclosed"
			}
		};

		// Act
		var result = configPatch.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Spec.Data" && 
			e.ErrorMessage.Contains("valid YAML"));
	}

	[Fact]
	public void Validate_WithInvalidPatchId_ReturnsError()
	{
		// Arrange
		var configPatch = new ConfigPatch
		{
			Metadata = new ResourceMetadata { Id = "Test_Patch", Namespace = "default" }, // Invalid: uppercase and underscore
			Spec = new ConfigPatchSpec
			{
				Data = "machine:\n  network:\n    hostname: test-node"
			}
		};

		// Act
		var result = configPatch.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Metadata.Id" && 
			e.ErrorMessage.Contains("DNS-1123"));
	}

	[Fact]
	public void ValidateAndThrow_WithInvalidConfigPatch_ThrowsValidationException()
	{
		// Arrange
		var configPatch = new ConfigPatch
		{
			Metadata = new ResourceMetadata { Id = "test-patch", Namespace = "default" },
			Spec = new ConfigPatchSpec()
			// Missing Data
		};

		// Act & Assert
		Assert.Throws<ValidationException>(() => configPatch.ValidateAndThrow());
	}

	[Fact]
	public void ValidateAndThrow_WithValidConfigPatch_DoesNotThrow()
	{
		// Arrange
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node")
			.Build();

		// Act & Assert (should not throw)
		configPatch.ValidateAndThrow();
	}

	[Fact]
	public void Validate_WithComplexValidYaml_ReturnsSuccess()
	{
		// Arrange
		var complexYaml = @"
machine:
  install:
    diskSelector:
      size: '>= 100GB'
  network:
    hostname: test-node
    interfaces:
      - interface: eth0
        dhcp: true
";

		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData(complexYaml)
			.Build();

		// Act
		var result = configPatch.Validate();

		// Assert
		Assert.True(result.IsValid);
	}
}
