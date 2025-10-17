using AwesomeAssertions;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

public class ResourceTypeRegistryTests
{
	[Fact]
	public void Initialize_RegistersCoreResourceTypes()
	{
		// Arrange & Act
		ResourceTypes.Initialize();

		// Assert
		ResourceTypeRegistry.IsRegistered<Cluster>().Should().BeTrue();
		ResourceTypeRegistry.IsRegistered<Machine>().Should().BeTrue();
		ResourceTypeRegistry.IsRegistered<ClusterMachine>().Should().BeTrue();
	}

	[Fact]
	public void GetProtoTypeName_ForCluster_ReturnsCorrectType()
	{
		// Arrange
		ResourceTypes.Initialize();

		// Act
		var typeName = ResourceTypeRegistry.GetProtoTypeName<Cluster>();

		// Assert
		typeName.Should().Be("Clusters.omni.sidero.dev");
	}

	[Fact]
	public void GetProtoTypeName_ForMachine_ReturnsCorrectType()
	{
		// Arrange
		ResourceTypes.Initialize();

		// Act
		var typeName = ResourceTypeRegistry.GetProtoTypeName<Machine>();

		// Assert
		typeName.Should().Be("Machines.omni.sidero.dev");
	}

	[Fact]
	public void GetProtoTypeName_ForClusterMachine_ReturnsCorrectType()
	{
		// Arrange
		ResourceTypes.Initialize();

		// Act
		var typeName = ResourceTypeRegistry.GetProtoTypeName<ClusterMachine>();

		// Assert
		typeName.Should().Be("ClusterMachines.omni.sidero.dev");
	}

	[Fact]
	public void Initialize_CalledMultipleTimes_DoesNotThrow()
	{
		// Act & Assert (should not throw)
		ResourceTypes.Initialize();
		ResourceTypes.Initialize();
		ResourceTypes.Initialize();
	}

	[Fact]
	public void IsInitialized_AfterInitialize_ReturnsTrue()
	{
		// Arrange
		ResourceTypes.Initialize();

		// Act & Assert
		ResourceTypes.IsInitialized.Should().BeTrue();
	}
}
