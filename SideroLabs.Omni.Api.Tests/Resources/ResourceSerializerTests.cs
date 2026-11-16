using AwesomeAssertions;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Serialization;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Unit tests for ResourceSerializer JSON and YAML round-trip serialization.
/// </summary>
public class ResourceSerializerTests
{
	private static Cluster NewCluster(string id, string kubernetesVersion, bool ready) => new()
	{
		Metadata = new ResourceMetadata { Namespace = "default", Id = id },
		Spec = new ClusterSpec { KubernetesVersion = kubernetesVersion, TalosVersion = "v1.7.0" },
		Status = new ClusterStatus { Ready = ready }
	};

	/// <summary>
	/// Verifies that a Cluster resource survives a JSON serialization/deserialization round-trip with data integrity.
	/// </summary>
	[Fact]
	public void JsonRoundtrip_Cluster()
	{
		var cluster = NewCluster("test-cluster", "v1.29.0", true);

		var json = ResourceSerializer.ToJson(cluster);
		var deserialized = ResourceSerializer.FromJson<Cluster>(json);

		deserialized.Should().NotBeNull();
		deserialized!.Metadata.Id.Should().Be(cluster.Metadata.Id);
		deserialized.Spec.KubernetesVersion.Should().Be(cluster.Spec.KubernetesVersion);
		Assert.True(deserialized.Status?.Ready);
	}

	/// <summary>
	/// Verifies that a Cluster resource survives a YAML serialization/deserialization round-trip with data integrity.
	/// </summary>
	[Fact]
	public void YamlRoundtrip_Cluster()
	{
		var cluster = NewCluster("yaml-cluster", "v1.28.0", false);

		var yaml = ResourceSerializer.ToYaml(cluster);
		var deserialized = ResourceSerializer.FromYaml<Cluster>(yaml);

		deserialized.Should().NotBeNull();
		deserialized!.Metadata.Id.Should().Be(cluster.Metadata.Id);
		deserialized.Spec.KubernetesVersion.Should().Be(cluster.Spec.KubernetesVersion);
	}

	/// <summary>
	/// Verifies that a custom proto type name can be registered and retrieved for a resource type.
	/// </summary>
	[Fact]
	public void ResourceTypeRegistry_RegisterAndGetName()
	{
		var protoName = "test.cluster.v1";
		ResourceTypeRegistry.Register<Cluster>(protoName);

		var resolved = ResourceTypeRegistry.GetProtoTypeName<Cluster>();

		resolved.Should().Be(protoName);
	}
}
