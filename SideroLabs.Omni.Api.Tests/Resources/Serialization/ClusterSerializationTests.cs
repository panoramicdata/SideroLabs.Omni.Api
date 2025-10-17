using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SideroLabs.Omni.Api.Tests.Resources.Serialization;

public class ClusterSerializationTests
{
	[Fact]
	public void ToYaml_WithValidCluster_ProducesValidYaml()
	{
		// Arrange
		var cluster = new ClusterBuilder("production-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.WithLabel("environment", "production")
			.WithLabel("region", "us-west-2")
			.Build();

		// Act
		var yaml = cluster.ToYaml();

		// Assert
		Assert.NotNull(yaml);
		Assert.Contains("apiVersion: omni.sidero.dev/v1alpha1", yaml);
		Assert.Contains("kind: Cluster", yaml);
		Assert.Contains("name: production-cluster", yaml);
		Assert.Contains("kubernetesVersion: v1.29.0", yaml);
		Assert.Contains("talosVersion: v1.7.0", yaml);
		Assert.Contains("environment: production", yaml);
		Assert.Contains("region: us-west-2", yaml);
	}

	[Fact]
	public void ToYaml_WithStatus_IncludesStatusInYaml()
	{
		// Arrange
		var cluster = new ClusterBuilder("test-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		cluster.Status = new ClusterStatus
		{
			Ready = true,
			Phase = "Running",
			ControlPlaneCount = 3,
			WorkerCount = 5
		};

		// Act
		var yaml = cluster.ToYaml();

		// Assert
		Assert.Contains("status:", yaml);
		Assert.Contains("ready: true", yaml);
		Assert.Contains("phase: Running", yaml);
		Assert.Contains("controlPlaneCount: 3", yaml);
		Assert.Contains("workerCount: 5", yaml);
	}

	[Fact]
	public void ToJson_WithValidCluster_ProducesValidJson()
	{
		// Arrange
		var cluster = new ClusterBuilder("production-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.WithLabel("environment", "production")
			.Build();

		// Act
		var json = cluster.ToJson();

		// Assert
		Assert.NotNull(json);
		Assert.Contains("\"apiVersion\":\"omni.sidero.dev/v1alpha1\"", json);
		Assert.Contains("\"kind\":\"Cluster\"", json);
		Assert.Contains("\"name\":\"production-cluster\"", json);
		Assert.Contains("\"kubernetesVersion\":\"v1.29.0\"", json);
		Assert.Contains("\"talosVersion\":\"v1.7.0\"", json);
	}

	[Fact]
	public void FromYaml_WithValidYaml_DeserializesCluster()
	{
		// Arrange
		var yaml = @"
apiVersion: omni.sidero.dev/v1alpha1
kind: Cluster
metadata:
  namespace: default
  name: test-cluster
  labels:
    environment: test
spec:
  kubernetesVersion: v1.29.0
  talosVersion: v1.7.0
";

		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();

		// Act
		var doc = deserializer.Deserialize<Dictionary<string, object>>(yaml);

		// Assert
		Assert.NotNull(doc);
		Assert.Equal("omni.sidero.dev/v1alpha1", doc["apiVersion"]);
		Assert.Equal("Cluster", doc["kind"]);
		
		var metadata = doc["metadata"] as Dictionary<object, object>;
		Assert.NotNull(metadata);
		Assert.Equal("test-cluster", metadata["name"]);
		Assert.Equal("default", metadata["namespace"]);

		var spec = doc["spec"] as Dictionary<object, object>;
		Assert.NotNull(spec);
		Assert.Equal("v1.29.0", spec["kubernetesVersion"]);
		Assert.Equal("v1.7.0", spec["talosVersion"]);
	}

	[Fact]
	public void RoundTrip_YamlToObjectToYaml_PreservesData()
	{
		// Arrange
		var originalCluster = new ClusterBuilder("roundtrip-cluster")
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.WithLabel("test", "roundtrip")
			.WithAnnotation("description", "Round trip test")
			.Build();

		// Act - Convert to YAML and back
		var yaml = originalCluster.ToYaml();
		
		// Parse YAML to verify structure
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();
		
		var doc = deserializer.Deserialize<Dictionary<string, object>>(yaml);

		// Assert - Verify key fields are preserved
		Assert.Equal("omni.sidero.dev/v1alpha1", doc["apiVersion"]);
		Assert.Equal("Cluster", doc["kind"]);
		
		var metadata = doc["metadata"] as Dictionary<object, object>;
		Assert.NotNull(metadata);
		Assert.Equal("roundtrip-cluster", metadata["name"]);

		var labels = metadata["labels"] as Dictionary<object, object>;
		Assert.NotNull(labels);
		Assert.Equal("roundtrip", labels["test"]);

		var annotations = metadata["annotations"] as Dictionary<object, object>;
		Assert.NotNull(annotations);
		Assert.Equal("Round trip test", annotations["description"]);
	}
}
