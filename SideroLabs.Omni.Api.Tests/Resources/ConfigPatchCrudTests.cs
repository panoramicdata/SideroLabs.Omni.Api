using AwesomeAssertions;
using SideroLabs.Omni.Api.Builders;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for basic CRUD operations on config patch resources
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ConfigPatchCrudTests(ITestOutputHelper testOutputHelper) : ResourceCrudTestBase<Api.Resources.ConfigPatch>(testOutputHelper)
{
	/// <inheritdoc />
	protected override string Label => "config patch";

	/// <inheritdoc />
	protected override string IdPrefix => "test-patch";

	/// <inheritdoc />
	protected override Api.Resources.ConfigPatch Build(string id) => new ConfigPatchBuilder(id)
		.WithData("machine:\n  network:\n    hostname: test-node")
		.Build();

	/// <inheritdoc />
	protected override void Modify(Api.Resources.ConfigPatch resource) =>
		resource.Spec.Data = "machine:\n  network:\n    hostname: updated-node";

	/// <inheritdoc />
	protected override void AssertCreated(Api.Resources.ConfigPatch created) =>
		created.Spec.Data.Should().Contain("hostname: test-node");

	/// <inheritdoc />
	protected override void AssertRetrieved(Api.Resources.ConfigPatch retrieved, Api.Resources.ConfigPatch original) =>
		retrieved.Spec.Data.Should().Contain("hostname: test-node");

	/// <inheritdoc />
	protected override void AssertUpdated(Api.Resources.ConfigPatch updated) =>
		updated.Spec.Data.Should().Contain("updated-node");

	/// <inheritdoc />
	protected override string PluralLabel => "config patches";
}
