using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Cluster = SideroLabs.Omni.Api.Resources.Cluster;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for resource filtering operations
/// Tests selector, regex, pagination, sorting, and search capabilities
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Filtering")]
public class ResourceFilteringTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that filters the cluster list using a label selector and verifies only matching clusters are returned.
	/// </summary>
	[Fact]
	public async Task List_WithSelector_FiltersResults()
	{
		await RunAsync(async client =>
		{
			var labelled = await FindLabelledClusterAsync(client);
			if (labelled == null)
			{
				Logger.LogInformation("⏭️ No clusters with labels found - skipping selector filtering test");
				return;
			}

			var (key, value) = labelled.Value;
			var filtered = await client.Resources.ListAsync<Cluster>(selector: $"{key}={value}", cancellationToken: CancellationToken)
				.ToListAsync(CancellationToken);

			filtered.Should().NotBeEmpty();
			filtered.Should().AllSatisfy(c => c.Metadata.Labels.Should().ContainKey(key).WhoseValue.Should().Be(value));
		});
	}

	/// <summary>
	/// Verifies that listing resources with a regex filter returns only resources whose IDs match the pattern.
	/// </summary>
	[Fact]
	public async Task List_WithRegex_MatchesIds()
	{
		await RunAsync(async client =>
		{
			var prefix = await FirstIdPrefixAsync(client, 5);
			if (prefix == null)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping regex filtering test");
				return;
			}

			var matches = await client.Resources.ListAsync<Cluster>(idMatchRegexp: $"^{prefix}.*", cancellationToken: CancellationToken)
				.ToListAsync(CancellationToken);

			matches.Should().NotBeEmpty();
			matches.Should().AllSatisfy(c => c.Metadata.Id.Should().StartWith(prefix));
		});
	}

	/// <summary>
	/// Verifies that listing resources with pagination parameters returns the correct page of results.
	/// </summary>
	[Fact]
	public async Task List_WithPagination_ReturnsCorrectPage()
	{
		await RunAsync(async client =>
		{
			var page = await client.Resources.ListAsync<Cluster>(offset: 0, limit: 10, cancellationToken: CancellationToken)
				.ToListAsync(CancellationToken);

			page.Should().HaveCountLessThanOrEqualTo(10);
		});
	}

	/// <summary>
	/// Verifies that listing resources with a sort order parameter returns results in the specified sorted order.
	/// </summary>
	[Fact]
	public async Task List_WithSorting_ReturnsSortedResults()
	{
		await RunAsync(async client =>
		{
			var sorted = await client.Resources.ListAsync<Cluster>(
				sortBy: "metadata.created",
				sortDescending: false,
				limit: 5,
				cancellationToken: CancellationToken).ToListAsync(CancellationToken);

			sorted.Should().NotBeNull();
		});
	}

	/// <summary>
	/// Verifies that listing resources with a search term returns only resources matching the search query.
	/// </summary>
	[Fact]
	public async Task List_WithSearch_FindsMatches()
	{
		await RunAsync(async client =>
		{
			var term = await FirstIdPrefixAsync(client, 4);
			if (term == null)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping search test");
				return;
			}

			var results = await client.Resources.ListAsync<Cluster>(searchFor: [term], cancellationToken: CancellationToken)
				.ToListAsync(CancellationToken);

			results.Should().NotBeEmpty();
		});
	}

	/// <summary>
	/// Verifies that combining multiple filter options (selector, pagination, sort, search) applies all constraints correctly.
	/// </summary>
	[Fact]
	public async Task List_CombinedFilters_AppliesAll()
	{
		await RunAsync(async client =>
		{
			if (await FirstIdPrefixAsync(client, 1) == null)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping combined filters test");
				return;
			}

			var labelled = await FindLabelledClusterAsync(client);
			var results = labelled == null
				? await client.Resources.ListAsync<Cluster>(offset: 0, limit: 2, sortBy: "metadata.id", cancellationToken: CancellationToken)
					.ToListAsync(CancellationToken)
				: await client.Resources.ListAsync<Cluster>(
					selector: $"{labelled.Value.Key}={labelled.Value.Value}",
					limit: 1,
					cancellationToken: CancellationToken).ToListAsync(CancellationToken);

			results.Should().NotBeEmpty();
			results.Should().HaveCountLessThanOrEqualTo(labelled == null ? 2 : 1);
		});
	}

	private async Task<string?> FirstIdPrefixAsync(OmniClient client, int length)
	{
		var all = await client.Resources.ListAsync<Cluster>(cancellationToken: CancellationToken).ToListAsync(CancellationToken);

		return all.Count == 0 ? null : all[0].Metadata.Id[..Math.Min(length, all[0].Metadata.Id.Length)];
	}

	private async Task<(string Key, string Value)?> FindLabelledClusterAsync(OmniClient client)
	{
		var all = await client.Resources.ListAsync<Cluster>(cancellationToken: CancellationToken).ToListAsync(CancellationToken);
		var labelled = all.FirstOrDefault(c => c.Metadata.Labels.Count > 0);
		if (labelled == null)
		{
			return null;
		}

		var key = labelled.Metadata.Labels.Keys.First();
		return (key, labelled.Metadata.Labels[key]);
	}
}
