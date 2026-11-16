using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Builders;
using Xunit;
using Cluster = SideroLabs.Omni.Api.Resources.Cluster;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for declarative apply operations on Omni resources
/// Tests Apply, ApplyYaml and ApplyFile
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ResourceApplyTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	private static Cluster BuildCluster(string clusterId) => new ClusterBuilder(clusterId)
		.WithKubernetesVersion("v1.29.0")
		.WithTalosVersion("v1.7.0")
		.Build();

	private static string ClusterYaml(string clusterId) => string.Join(
		"\n",
		"metadata:",
		$"  id: {clusterId}",
		"  namespace: default",
		"  type: Clusters.omni.sidero.dev",
		"  version: v1alpha1",
		"spec:",
		"  kubernetesVersion: v1.29.0",
		"  talosVersion: v1.7.0",
		"  installImage: ghcr.io/siderolabs/installer:v1.7.0",
		string.Empty);

	/// <summary>
	/// Runs the body with a client and a fresh cluster id, always cleaning the cluster up afterwards.
	/// </summary>
	private Task RunWithClusterAsync(string prefix, Func<OmniClient, string, Task> body) =>
		RunAsync(async client =>
		{
			var clusterId = CreateUniqueId(prefix);

			try
			{
				await body(client, clusterId);
			}
			finally
			{
				await CleanupAsync<Cluster>(client, clusterId);
			}
		});

	/// <summary>
	/// Integration test that applies a new Cluster resource using the declarative apply API.
	/// </summary>
	[Fact]
	public Task Apply_NewResource_CreatesResource() =>
		RunWithClusterAsync("apply-test", async (client, clusterId) =>
		{
			var applied = await client.Resources.ApplyAsync(BuildCluster(clusterId), dryRun: false, cancellationToken: CancellationToken);

			applied.Metadata.Id.Should().Be(clusterId);
		});

	/// <summary>
	/// Integration test that applies an updated Cluster resource and verifies the resource is updated.
	/// </summary>
	[Fact]
	public Task Apply_ExistingResource_UpdatesResource() =>
		RunWithClusterAsync("apply-update-test", async (client, clusterId) =>
		{
			var created = await client.Resources.CreateAsync(BuildCluster(clusterId), CancellationToken);
			created.Metadata.Labels ??= new Dictionary<string, string>();
			created.Metadata.Labels["updated"] = "true";

			var applied = await client.Resources.ApplyAsync(created, dryRun: false, cancellationToken: CancellationToken);

			applied.Metadata.Labels.Should().ContainKey("updated");
			applied.Metadata.Labels["updated"].Should().Be("true");
			applied.Metadata.Version.Should().NotBe(created.Metadata.Version, "Version should increment on update");
		});

	/// <summary>
	/// Verifies that applying a well-formed YAML document creates the described resource successfully.
	/// </summary>
	[Fact]
	public Task ApplyYaml_ValidYaml_CreatesResource() =>
		RunWithClusterAsync("yaml-test", async (client, clusterId) =>
		{
			var applied = await client.Resources.ApplyYamlAsync<Cluster>(ClusterYaml(clusterId), dryRun: false, cancellationToken: CancellationToken);

			applied.Metadata.Id.Should().Be(clusterId);
		});

	/// <summary>
	/// Verifies that applying a malformed YAML string throws an exception rather than silently failing.
	/// </summary>
	[Fact]
	public Task ApplyYaml_InvalidYaml_ThrowsException() =>
		RunClientAsync(async client =>
		{
			var invalidYaml = "this is not: valid yaml\n  - with invalid\n    structure:\n";

			await Assert.ThrowsAnyAsync<Exception>(() =>
				client.Resources.ApplyYamlAsync<Cluster>(invalidYaml, dryRun: false, cancellationToken: CancellationToken));
		});

	/// <summary>
	/// Verifies that applying a resource from a valid YAML file creates the described resource successfully.
	/// </summary>
	[Fact]
	public Task ApplyFile_ValidFile_CreatesResource() =>
		RunWithClusterAsync("file-test", async (client, clusterId) =>
		{
			var tempFile = Path.GetTempFileName();

			try
			{
				await File.WriteAllTextAsync(tempFile, ClusterYaml(clusterId), CancellationToken);

				var applied = await client.Resources.ApplyFileAsync<Cluster>(tempFile, dryRun: false, cancellationToken: CancellationToken);

				applied.Metadata.Id.Should().Be(clusterId);
			}
			finally
			{
				File.Delete(tempFile);
			}
		});

	/// <summary>
	/// Verifies that attempting to apply from a non-existent file path throws a <see cref="FileNotFoundException"/>.
	/// </summary>
	[Fact]
	public Task ApplyFile_NonExistentFile_ThrowsException() =>
		RunClientAsync(async client =>
		{
			var nonExistentFile = Path.Combine(Path.GetTempPath(), $"non-existent-{Guid.NewGuid()}.yaml");

			await Assert.ThrowsAsync<FileNotFoundException>(() =>
				client.Resources.ApplyFileAsync<Cluster>(nonExistentFile, dryRun: false, cancellationToken: CancellationToken));
		});

	/// <summary>
	/// Verifies that applying the same resource multiple times produces the same result (idempotent behavior).
	/// </summary>
	[Fact]
	public Task Apply_Idempotent_MultipleApplicationsSameResult() =>
		RunWithClusterAsync("idempotent-test", async (client, clusterId) =>
		{
			var current = BuildCluster(clusterId);

			for (var i = 0; i < 3; i++)
			{
				current = await client.Resources.ApplyAsync(current, dryRun: false, cancellationToken: CancellationToken);

				current.Metadata.Id.Should().Be(clusterId);
			}
		});
}
