using AwesomeAssertions;
using SideroLabs.Omni.Api.Builders;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for basic CRUD operations on extensions configuration resources
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ExtensionsConfigurationCrudTests(ITestOutputHelper testOutputHelper) : ResourceCrudTestBase<Api.Resources.ExtensionsConfiguration>(testOutputHelper)
{
	/// <inheritdoc />
	protected override string Label => "extensions configuration";

	/// <inheritdoc />
	protected override string IdPrefix => "test-extensions";

	/// <inheritdoc />
	protected override Api.Resources.ExtensionsConfiguration Build(string id) => new ExtensionsConfigurationBuilder(id)
		.WithExtensions(["siderolabs/iscsi-tools", "siderolabs/util-linux-tools"])
		.Build();

	/// <inheritdoc />
	protected override void Modify(Api.Resources.ExtensionsConfiguration resource) =>
		resource.Spec.Extensions = ["siderolabs/qemu-guest-agent"];

	/// <inheritdoc />
	protected override void AssertCreated(Api.Resources.ExtensionsConfiguration created)
	{
		created.Spec.Extensions.Should().HaveCount(2);
		created.Spec.Extensions.Should().Contain("siderolabs/iscsi-tools");
	}

	/// <inheritdoc />
	protected override void AssertRetrieved(Api.Resources.ExtensionsConfiguration retrieved, Api.Resources.ExtensionsConfiguration original) =>
		retrieved.Spec.Extensions.Should().HaveCount(2);

	/// <inheritdoc />
	protected override void AssertUpdated(Api.Resources.ExtensionsConfiguration updated)
	{
		updated.Spec.Extensions.Should().HaveCount(1);
		updated.Spec.Extensions.Should().Contain("siderolabs/qemu-guest-agent");
	}
}
