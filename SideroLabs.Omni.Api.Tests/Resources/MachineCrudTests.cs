using AwesomeAssertions;
using SideroLabs.Omni.Api.Builders;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for basic CRUD operations on machine resources
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class MachineCrudTests(ITestOutputHelper testOutputHelper) : ResourceCrudTestBase<Api.Resources.Machine>(testOutputHelper)
{
	/// <inheritdoc />
	protected override string Label => "machine";

	/// <inheritdoc />
	protected override string IdPrefix => "test-machine";

	/// <inheritdoc />
	protected override Api.Resources.Machine Build(string id) => new MachineBuilder(id).Build();

	/// <inheritdoc />
	protected override void Modify(Api.Resources.Machine resource)
	{
		resource.Metadata.Labels ??= new Dictionary<string, string>();
		resource.Metadata.Labels["updated"] = "true";
	}

	/// <inheritdoc />
	protected override void AssertCreated(Api.Resources.Machine created) =>
		created.Kind.Should().Be("Machine");

	/// <inheritdoc />
	protected override void AssertRetrieved(Api.Resources.Machine retrieved, Api.Resources.Machine original) =>
		retrieved.Metadata.Namespace.Should().Be(original.Metadata.Namespace);

	/// <inheritdoc />
	protected override void AssertUpdated(Api.Resources.Machine updated)
	{
		updated.Metadata.Labels.Should().ContainKey("updated");
		updated.Metadata.Labels["updated"].Should().Be("true");
	}
}
