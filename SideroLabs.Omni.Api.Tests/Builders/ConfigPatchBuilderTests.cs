using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Builders;

/// <summary>
/// Unit tests for the ConfigPatchBuilder fluent API.
/// </summary>
public class ConfigPatchBuilderTests
{
	/// <summary>
	/// Verifies that building with valid data produces a correctly populated ConfigPatch resource.
	/// </summary>
	[Fact]
	public void Build_WithValidConfiguration_CreatesConfigPatch()
	{
		// Arrange
		var builder = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node");

		// Act
		var configPatch = builder.Build();

		// Assert
		Assert.NotNull(configPatch);
		Assert.Equal("test-patch", configPatch.Metadata.Id);
		Assert.NotNull(configPatch.Spec.Data);
		Assert.Contains("hostname", configPatch.Spec.Data);
		Assert.Equal("ConfigPatch", configPatch.Kind);
		Assert.Equal("omni.sidero.dev/v1alpha1", configPatch.ApiVersion);
	}

	/// <summary>
	/// Verifies that building without specifying patch data throws InvalidOperationException.
	/// </summary>
	[Fact]
	public void Build_WithoutData_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new ConfigPatchBuilder("test-patch");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	/// <summary>
	/// Verifies that a single label is added to config patch metadata.
	/// </summary>
	[Fact]
	public void WithLabel_AddsLabel()
	{
		// Arrange & Act
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node")
			.WithLabel("environment", "production")
			.Build();

		// Assert
		Assert.NotNull(configPatch.Metadata.Labels);
		Assert.True(configPatch.Metadata.Labels.ContainsKey("environment"));
		Assert.Equal("production", configPatch.Metadata.Labels["environment"]);
	}

	/// <summary>
	/// Verifies that a dictionary of labels is added to config patch metadata.
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
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node")
			.WithLabels(labels)
			.Build();

		// Assert
		Assert.NotNull(configPatch.Metadata.Labels);
		Assert.Equal(2, configPatch.Metadata.Labels.Count);
		Assert.Equal("production", configPatch.Metadata.Labels["environment"]);
		Assert.Equal("us-west-2", configPatch.Metadata.Labels["region"]);
	}

	/// <summary>
	/// Verifies that the specified namespace is set in config patch metadata.
	/// </summary>
	[Fact]
	public void InNamespace_SetsNamespace()
	{
		// Arrange & Act
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node")
			.InNamespace("production")
			.Build();

		// Assert
		Assert.Equal("production", configPatch.Metadata.Namespace);
	}

	/// <summary>
	/// Verifies that a single annotation is added to config patch metadata.
	/// </summary>
	[Fact]
	public void WithAnnotation_AddsAnnotation()
	{
		// Arrange & Act
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node")
			.WithAnnotation("description", "Test patch")
			.Build();

		// Assert
		Assert.NotNull(configPatch.Metadata.Annotations);
		Assert.True(configPatch.Metadata.Annotations.ContainsKey("description"));
		Assert.Equal("Test patch", configPatch.Metadata.Annotations["description"]);
	}

	/// <summary>
	/// Verifies that a ConfigPatchBuilder can be built into a ConfigPatch resource.
	/// </summary>
	[Fact]
	public void ImplicitConversion_ConvertsToConfigPatch()
	{
		// Arrange
		ConfigPatchBuilder builder = new ConfigPatchBuilder("test-patch")
			.WithData("machine:\n  network:\n    hostname: test-node");

		// Act
		var configPatch = builder.Build();

		// Assert
		Assert.NotNull(configPatch);
		Assert.Equal("test-patch", configPatch.Metadata.Id);
	}

	/// <summary>
	/// Verifies that passing null as the patch ID throws ArgumentException.
	/// </summary>
	[Fact]
	public void Constructor_WithNullPatchId_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ConfigPatchBuilder(null!));
	}

	/// <summary>
	/// Verifies that an empty or whitespace patch ID throws ArgumentException.
	/// </summary>
	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyPatchId_ThrowsArgumentException(string patchId)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ConfigPatchBuilder(patchId));
	}

	/// <summary>
	/// Verifies that complex multi-level YAML configuration data is stored verbatim.
	/// </summary>
	[Fact]
	public void WithData_WithComplexYaml_StoresCorrectly()
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

		// Act
		var configPatch = new ConfigPatchBuilder("test-patch")
			.WithData(complexYaml)
			.Build();

		// Assert
		Assert.Equal(complexYaml, configPatch.Spec.Data);
		Assert.Contains("diskSelector", configPatch.Spec.Data);
	}
}

