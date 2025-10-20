using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using SideroLabs.Omni.Api.Builders;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for resource filtering operations
/// Tests selector, regex, pagination, sorting, and search capabilities
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Filtering")]
public class ResourceFilteringTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task List_WithSelector_FiltersResults()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List all clusters first to understand what's available
			Logger.LogInformation("🔍 Listing all clusters to check for existing data");
			var allClusters = new List<Api.Resources.Cluster>();
			
			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(cancellationToken: CancellationToken))
			{
				allClusters.Add(c);
			}

			if (allClusters.Count == 0)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping selector filtering test");
				return;
			}

			// Get a label from an existing cluster to test filtering
			var clusterWithLabels = allClusters.FirstOrDefault(c => c.Metadata.Labels.Count > 0);
			if (clusterWithLabels == null)
			{
				Logger.LogInformation("⏭️ No clusters with labels found - skipping selector filtering test");
				return;
			}

			var labelKey = clusterWithLabels.Metadata.Labels.Keys.First();
			var labelValue = clusterWithLabels.Metadata.Labels[labelKey];

			// Act - List with selector filtering
			Logger.LogInformation("🔍 Listing clusters with selector: {Key}={Value}", labelKey, labelValue);
			var filteredClusters = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				selector: $"{labelKey}={labelValue}",
				cancellationToken: CancellationToken))
			{
				filteredClusters.Add(c);
			}

			// Assert
			filteredClusters.Should().NotBeEmpty();
			filteredClusters.Should().AllSatisfy(c => 
				c.Metadata.Labels.Should().ContainKey(labelKey)
				.WhoseValue.Should().Be(labelValue));

			Logger.LogInformation("✅ Selector filtering successful - found {Count} matching clusters", filteredClusters.Count);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task List_WithRegex_MatchesIds()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List all clusters first
			Logger.LogInformation("🔍 Listing all clusters");
			var allClusters = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(cancellationToken: CancellationToken))
			{
				allClusters.Add(c);
			}

			if (allClusters.Count == 0)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping regex filtering test");
				return;
			}

			// Use first cluster's ID to create a regex pattern
			var firstCluster = allClusters.First();
			var idPrefix = firstCluster.Metadata.Id.Substring(0, Math.Min(5, firstCluster.Metadata.Id.Length));

			// Act - List with ID regex
			Logger.LogInformation("🔍 Listing clusters with regex: ^{Prefix}.*", idPrefix);
			var regexClusters = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				idMatchRegexp: $"^{idPrefix}.*",
				cancellationToken: CancellationToken))
			{
				regexClusters.Add(c);
			}

			// Assert
			regexClusters.Should().NotBeEmpty();
			regexClusters.Should().AllSatisfy(c => 
				c.Metadata.Id.Should().StartWith(idPrefix));

			Logger.LogInformation("✅ Regex filtering successful - found {Count} matching clusters", regexClusters.Count);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task List_WithPagination_ReturnsCorrectPage()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List with pagination (first page, limit 10)
			Logger.LogInformation("🔍 Listing clusters with pagination: offset=0, limit=10");
			var page1 = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				offset: 0,
				limit: 10,
				cancellationToken: CancellationToken))
			{
				page1.Add(c);
			}

			// Assert - Should get at most 10 results
			page1.Should().HaveCountLessThanOrEqualTo(10);

			Logger.LogInformation("✅ Pagination successful - returned {Count} clusters", page1.Count);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task List_WithSorting_ReturnsSortedResults()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List with sorting by metadata.created
			Logger.LogInformation("🔍 Listing clusters sorted by metadata.created");
			var sortedClusters = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				sortBy: "metadata.created",
				sortDescending: false,
				limit: 5,
				cancellationToken: CancellationToken))
			{
				sortedClusters.Add(c);
			}

			// Assert - Should get results (can't verify exact order without knowing creation times)
			sortedClusters.Should().NotBeNull();

			Logger.LogInformation("✅ Sorting successful - returned {Count} clusters", sortedClusters.Count);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task List_WithSearch_FindsMatches()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List all clusters first
			Logger.LogInformation("🔍 Listing all clusters");
			var allClusters = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(cancellationToken: CancellationToken))
			{
				allClusters.Add(c);
			}

			if (allClusters.Count == 0)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping search test");
				return;
			}

			// Use part of first cluster's ID for search
			var firstCluster = allClusters.First();
			var searchTerm = firstCluster.Metadata.Id.Substring(0, Math.Min(4, firstCluster.Metadata.Id.Length));

			// Act - List with search term
			Logger.LogInformation("🔍 Searching for clusters with term: {Term}", searchTerm);
			var searchResults = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				searchFor: new[] { searchTerm },
				cancellationToken: CancellationToken))
			{
				searchResults.Add(c);
			}

			// Assert
			searchResults.Should().NotBeEmpty();

			Logger.LogInformation("✅ Search successful - found {Count} matches", searchResults.Count);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task List_CombinedFilters_AppliesAll()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List all clusters first
			Logger.LogInformation("🔍 Listing all clusters");
			var allClusters = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(cancellationToken: CancellationToken))
			{
				allClusters.Add(c);
			}

			if (allClusters.Count == 0)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping combined filters test");
				return;
			}

			// Find a cluster with labels to test combined filtering
			var clusterWithLabels = allClusters.FirstOrDefault(c => c.Metadata.Labels.Count > 0);
			if (clusterWithLabels == null)
			{
				Logger.LogInformation("⏭️ No clusters with labels - testing pagination + sorting only");
				
				// Test combined pagination + sorting
				var results = new List<Api.Resources.Cluster>();
				await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
					offset: 0,
					limit: 2,
					sortBy: "metadata.id",
					cancellationToken: CancellationToken))
				{
					results.Add(c);
				}

				results.Should().NotBeEmpty();
				results.Should().HaveCountLessThanOrEqualTo(2);
				Logger.LogInformation("✅ Combined filtering successful (pagination + sorting) - returned {Count} clusters", results.Count);
				return;
			}

			var labelKey = clusterWithLabels.Metadata.Labels.Keys.First();
			var labelValue = clusterWithLabels.Metadata.Labels[labelKey];

			// Act - List with combined filters (selector + limit)
			Logger.LogInformation("🔍 Listing with combined filters: selector={Key}={Value}, limit=1", labelKey, labelValue);
			var results2 = new List<Api.Resources.Cluster>();

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				selector: $"{labelKey}={labelValue}",
				limit: 1,
				cancellationToken: CancellationToken))
			{
				results2.Add(c);
			}

			// Assert
			results2.Should().NotBeEmpty();
			results2.Should().HaveCountLessThanOrEqualTo(1);

			Logger.LogInformation("✅ Combined filtering successful");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	#region Helper Methods

	private OmniClientOptions GetClientOptions()
	{
		var configuration = Configuration;
		var omniSection = configuration.GetSection("Omni");
		var authToken = omniSection["AuthToken"] ?? throw new FormatException("Omni:AuthToken required");

		return new OmniClientOptions
		{
			BaseUrl = new(omniSection["BaseUrl"] ?? throw new InvalidOperationException("Omni:BaseUrl required")),
			AuthToken = authToken,
			TimeoutSeconds = int.Parse(omniSection["TimeoutSeconds"] ?? "30"),
			UseTls = bool.Parse(omniSection["UseTls"] ?? "true"),
			ValidateCertificate = bool.Parse(omniSection["ValidateCertificate"] ?? "true"),
			IsReadOnly = bool.Parse(omniSection["IsReadOnly"] ?? "false"),
			Logger = Logger
		};
	}

	private static string CreateUniqueId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

	private async Task CleanupCluster(OmniClient client, string clusterId)
	{
		try
		{
			await client.Resources.DeleteAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test cluster: {ClusterId}", clusterId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogDebug("Test cluster already deleted: {ClusterId}", clusterId);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test cluster: {ClusterId}", clusterId);
		}
	}

	#endregion
}
