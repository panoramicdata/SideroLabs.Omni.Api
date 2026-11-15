using AwesomeAssertions;
using SideroLabs.Omni.Api.Builders;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for basic CRUD operations on cluster resources
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ClusterCrudTests(ITestOutputHelper testOutputHelper) : ResourceCrudTestBase<Api.Resources.Cluster>(testOutputHelper)
{
	/// <inheritdoc />
	protected override string Label => "cluster";

	/// <inheritdoc />
	protected override string IdPrefix => "test-cluster";

	/// <inheritdoc />
	protected override Api.Resources.Cluster Build(string id) => new ClusterBuilder(id)
		.WithKubernetesVersion("v1.29.0")
		.WithTalosVersion("v1.7.0")
		.Build();

	/// <inheritdoc />
	protected override void Modify(Api.Resources.Cluster resource) => resource.Spec.KubernetesVersion = "v1.30.0";

	/// <inheritdoc />
	protected override void AssertCreated(Api.Resources.Cluster created)
	{
		created.Spec.KubernetesVersion.Should().Be("v1.29.0");
		created.Spec.TalosVersion.Should().Be("v1.7.0");
	}

	/// <inheritdoc />
	protected override void AssertRetrieved(Api.Resources.Cluster retrieved, Api.Resources.Cluster original) =>
		retrieved.Spec.KubernetesVersion.Should().Be(original.Spec.KubernetesVersion);

	/// <inheritdoc />
	protected override void AssertUpdated(Api.Resources.Cluster updated) =>
		updated.Spec.KubernetesVersion.Should().Be("v1.30.0");
}
