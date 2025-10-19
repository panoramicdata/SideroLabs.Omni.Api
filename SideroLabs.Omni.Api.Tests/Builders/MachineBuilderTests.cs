using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;
using Machine = SideroLabs.Omni.Api.Resources.Machine;

namespace SideroLabs.Omni.Api.Tests.Builders;

public class MachineBuilderTests
{
	[Fact]
	public void Build_WithValidConfiguration_CreatesMachine()
	{
		// Arrange
		var builder = new MachineBuilder("machine-001")
			.WithRole("controlplane")
			.WithImage("ghcr.io/siderolabs/installer:v1.7.0");

		// Act
		var machine = builder.Build();

		// Assert
		Assert.NotNull(machine);
		Assert.Equal("machine-001", machine.Metadata.Id);
		Assert.Equal("controlplane", machine.Spec.Role);
		Assert.Equal("ghcr.io/siderolabs/installer:v1.7.0", machine.Spec.Image);
		Assert.Equal("Machine", machine.Kind);
		Assert.Equal("omni.sidero.dev/v1alpha1", machine.ApiVersion);
	}

	[Fact]
	public void Build_WithoutRole_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new MachineBuilder("machine-001")
			.WithImage("ghcr.io/siderolabs/installer:v1.7.0");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	[Fact]
	public void WithLabel_AddsMetadataLabel()
	{
		// Arrange & Act
		var machine = new MachineBuilder("machine-001")
			.WithRole("worker")
			.WithLabel("rack", "rack-1")
			.Build();

		// Assert
		Assert.NotNull(machine.Metadata.Labels);
		Assert.True(machine.Metadata.Labels.ContainsKey("rack"));
		Assert.Equal("rack-1", machine.Metadata.Labels["rack"]);
	}

	[Fact]
	public void WithSpecLabel_AddsSpecLabel()
	{
		// Arrange & Act
		var machine = new MachineBuilder("machine-001")
			.WithRole("worker")
			.WithSpecLabel("gpu", "nvidia-a100")
			.Build();

		// Assert
		Assert.NotNull(machine.Spec.Labels);
		Assert.True(machine.Spec.Labels.ContainsKey("gpu"));
		Assert.Equal("nvidia-a100", machine.Spec.Labels["gpu"]);
	}

	[Fact]
	public void WithLabels_AddsMultipleLabels()
	{
		// Arrange
		var labels = new Dictionary<string, string>
		{
			["datacenter"] = "us-west-1",
			["zone"] = "zone-a"
		};

		// Act
		var machine = new MachineBuilder("machine-001")
			.WithRole("worker")
			.WithLabels(labels)
			.Build();

		// Assert
		Assert.NotNull(machine.Metadata.Labels);
		Assert.Equal(2, machine.Metadata.Labels.Count);
		Assert.Equal("us-west-1", machine.Metadata.Labels["datacenter"]);
		Assert.Equal("zone-a", machine.Metadata.Labels["zone"]);
	}

	[Fact]
	public void InNamespace_SetsNamespace()
	{
		// Arrange & Act
		var machine = new MachineBuilder("machine-001")
			.WithRole("worker")
			.InNamespace("fleet")
			.Build();

		// Assert
		Assert.Equal("fleet", machine.Metadata.Namespace);
	}

	[Fact]
	public void WithAnnotation_AddsAnnotation()
	{
		// Arrange & Act
		var machine = new MachineBuilder("machine-001")
			.WithRole("worker")
			.WithAnnotation("owner", "team-platform")
			.Build();

		// Assert
		Assert.NotNull(machine.Metadata.Annotations);
		Assert.True(machine.Metadata.Annotations.ContainsKey("owner"));
		Assert.Equal("team-platform", machine.Metadata.Annotations["owner"]);
	}

	[Fact]
	public void ImplicitConversion_ConvertsToMachine()
	{
		// Arrange
		MachineBuilder builder = new MachineBuilder("machine-001")
			.WithRole("controlplane");

		// Act
		Machine machine = builder;

		// Assert
		Assert.NotNull(machine);
		Assert.Equal("machine-001", machine.Metadata.Id);
	}

	[Fact]
	public void Constructor_WithNullMachineId_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new MachineBuilder(null!));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyMachineId_ThrowsArgumentException(string machineId)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new MachineBuilder(machineId));
	}
}
