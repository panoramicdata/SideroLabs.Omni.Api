using AwesomeAssertions;
using SideroLabs.Omni.Api.Resources;
using Xunit;
using Machine = SideroLabs.Omni.Api.Resources.Machine;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Unit tests for ResourceTypeRegistry initialization and type name resolution.
/// </summary>
public class ResourceTypeRegistryTests
{
	/// <summary>
	/// Verifies that Initialize registers Cluster, Machine, and ClusterMachine resource types.
	/// </summary>
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

	/// <summary>
	/// Verifies that the COSI proto type name for Cluster is 'Clusters.omni.sidero.dev'.
	/// </summary>
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

	/// <summary>
	/// Verifies that the COSI proto type name for Machine is 'Machines.omni.sidero.dev'.
	/// </summary>
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

	/// <summary>
	/// Verifies that the COSI proto type name for ClusterMachine is 'ClusterMachines.omni.sidero.dev'.
	/// </summary>
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

	/// <summary>
	/// Verifies that calling Initialize multiple times is idempotent and does not throw.
	/// </summary>
	[Fact]
	public void Initialize_CalledMultipleTimes_DoesNotThrow()
	{
		// Act & Assert (should not throw)
		ResourceTypes.Initialize();
		ResourceTypes.Initialize();
		ResourceTypes.Initialize();
	}

	/// <summary>
	/// Verifies that IsInitialized returns true after the registry has been initialized.
	/// </summary>
	[Fact]
	public void IsInitialized_AfterInitialize_ReturnsTrue()
	{
		// Arrange
		ResourceTypes.Initialize();

		// Act & Assert
		ResourceTypes.IsInitialized.Should().BeTrue();
	}
}
