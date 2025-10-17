using AwesomeAssertions;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Serialization;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

public class ResourceSerializerTests
{
	[Fact]
	public void JsonRoundtrip_Cluster()
	{
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Namespace = "default", Id = "test-cluster" },
			Spec = new ClusterSpec { KubernetesVersion = "v1.29.0", TalosVersion = "v1.7.0" },
			Status = new ClusterStatus { Ready = true }
		};

		var json = ResourceSerializer.ToJson(cluster);
		var deserialized = ResourceSerializer.FromJson<Cluster>(json);

		deserialized.Should().NotBeNull();
		deserialized!.Metadata.Id.Should().Be(cluster.Metadata.Id);
		deserialized.Spec.KubernetesVersion.Should().Be(cluster.Spec.KubernetesVersion);
		Assert.True(deserialized.Status?.Ready);
	}

	[Fact]
	public void YamlRoundtrip_Cluster()
	{
		var cluster = new Cluster
		{
			Metadata = new ResourceMetadata { Namespace = "default", Id = "yaml-cluster" },
			Spec = new ClusterSpec { KubernetesVersion = "v1.28.0" },
			Status = new ClusterStatus { Ready = false }
		};

		var yaml = ResourceSerializer.ToYaml(cluster);
		var deserialized = ResourceSerializer.FromYaml<Cluster>(yaml);

		deserialized.Should().NotBeNull();
		deserialized!.Metadata.Id.Should().Be(cluster.Metadata.Id);
		deserialized.Spec.KubernetesVersion.Should().Be(cluster.Spec.KubernetesVersion);
	}

	[Fact]
	public void ResourceTypeRegistry_RegisterAndGetName()
	{
		var protoName = "test.cluster.v1";
		ResourceTypeRegistry.Register<Cluster>(protoName);

		var resolved = ResourceTypeRegistry.GetProtoTypeName<Cluster>();

		resolved.Should().Be(protoName);
	}
}
