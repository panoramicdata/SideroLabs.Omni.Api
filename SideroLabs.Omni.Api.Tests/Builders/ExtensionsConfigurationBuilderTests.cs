using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Builders;

public class ExtensionsConfigurationBuilderTests
{
	[Fact]
	public void Build_WithValidConfiguration_CreatesExtensionsConfiguration()
	{
		// Arrange
		var builder = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.ForTalosVersion("v1.7.0");

		// Act
		var config = builder.Build();

		// Assert
		Assert.NotNull(config);
		Assert.Equal("test-config", config.Metadata.Id);
		Assert.NotNull(config.Spec.Extensions);
		Assert.Single(config.Spec.Extensions);
		Assert.Contains("siderolabs/util-linux-tools", config.Spec.Extensions);
		Assert.Equal("v1.7.0", config.Spec.TalosVersion);
		Assert.Equal("ExtensionsConfiguration", config.Kind);
		Assert.Equal("omni.sidero.dev/v1alpha1", config.ApiVersion);
	}

	[Fact]
	public void Build_WithoutExtensions_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new ExtensionsConfigurationBuilder("test-config");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	[Fact]
	public void WithExtensions_AddsMultipleExtensions()
	{
		// Arrange & Act
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtensions(
				"siderolabs/util-linux-tools",
				"siderolabs/qemu-guest-agent",
				"siderolabs/iscsi-tools")
			.Build();

		// Assert
		Assert.NotNull(config.Spec.Extensions);
		Assert.Equal(3, config.Spec.Extensions.Count);
		Assert.Contains("siderolabs/util-linux-tools", config.Spec.Extensions);
		Assert.Contains("siderolabs/qemu-guest-agent", config.Spec.Extensions);
		Assert.Contains("siderolabs/iscsi-tools", config.Spec.Extensions);
	}

	[Fact]
	public void WithLabel_AddsLabel()
	{
		// Arrange & Act
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.WithLabel("environment", "production")
			.Build();

		// Assert
		Assert.NotNull(config.Metadata.Labels);
		Assert.True(config.Metadata.Labels.ContainsKey("environment"));
		Assert.Equal("production", config.Metadata.Labels["environment"]);
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
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.WithLabels(labels)
			.Build();

		// Assert
		Assert.NotNull(config.Metadata.Labels);
		Assert.Equal(2, config.Metadata.Labels.Count);
		Assert.Equal("production", config.Metadata.Labels["environment"]);
		Assert.Equal("us-west-2", config.Metadata.Labels["region"]);
	}

	[Fact]
	public void InNamespace_SetsNamespace()
	{
		// Arrange & Act
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.InNamespace("production")
			.Build();

		// Assert
		Assert.Equal("production", config.Metadata.Namespace);
	}

	[Fact]
	public void WithAnnotation_AddsAnnotation()
	{
		// Arrange & Act
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools")
			.WithAnnotation("description", "Test extensions")
			.Build();

		// Assert
		Assert.NotNull(config.Metadata.Annotations);
		Assert.True(config.Metadata.Annotations.ContainsKey("description"));
		Assert.Equal("Test extensions", config.Metadata.Annotations["description"]);
	}

	[Fact]
	public void ImplicitConversion_ConvertsToExtensionsConfiguration()
	{
		// Arrange
		ExtensionsConfigurationBuilder builder = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools");

		// Act
		ExtensionsConfiguration config = builder;

		// Assert
		Assert.NotNull(config);
		Assert.Equal("test-config", config.Metadata.Id);
	}

	[Fact]
	public void Constructor_WithNullConfigId_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ExtensionsConfigurationBuilder(null!));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyConfigId_ThrowsArgumentException(string configId)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new ExtensionsConfigurationBuilder(configId));
	}

	[Fact]
	public void ExtensionCount_ReturnsCorrectCount()
	{
		// Arrange
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtensions(
				"siderolabs/util-linux-tools",
				"siderolabs/qemu-guest-agent")
			.Build();

		// Act & Assert
		Assert.Equal(2, config.ExtensionCount);
	}

	[Fact]
	public void WithExtension_WithVersionedExtension_StoresCorrectly()
	{
		// Arrange & Act
		var config = new ExtensionsConfigurationBuilder("test-config")
			.WithExtension("siderolabs/util-linux-tools@v2.13.1")
			.Build();

		// Assert
		Assert.Contains("siderolabs/util-linux-tools@v2.13.1", config.Spec.Extensions!);
	}
}
