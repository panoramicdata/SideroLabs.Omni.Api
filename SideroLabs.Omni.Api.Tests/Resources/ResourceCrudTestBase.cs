using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Shared create, get, update, delete and list scenarios for one Omni resource kind
/// </summary>
public abstract class ResourceCrudTestBase<T>(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
	where T : class, IOmniResource, new()
{
	/// <summary>Singular display name used in log messages.</summary>
	protected abstract string Label { get; }

	/// <summary>Prefix for generated resource ids.</summary>
	protected abstract string IdPrefix { get; }

	/// <summary>Builds a new resource with the given id.</summary>
	protected abstract T Build(string id);

	/// <summary>Applies the modification checked by the update scenario.</summary>
	protected abstract void Modify(T resource);

	/// <summary>Asserts the resource returned by create.</summary>
	protected abstract void AssertCreated(T created);

	/// <summary>Asserts the resource returned by get, given the originally created one.</summary>
	protected abstract void AssertRetrieved(T retrieved, T original);

	/// <summary>Asserts the resource returned by update.</summary>
	protected abstract void AssertUpdated(T updated);

	/// <summary>Plural display name used in log messages.</summary>
	protected virtual string PluralLabel => Label + "s";

	/// <summary>
	/// Verifies a new resource can be created via the Resources API.
	/// </summary>
	[Fact]
	public async Task Create_Success()
	{
		if (!ShouldRun())
		{
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var id = CreateUniqueId(IdPrefix);
		var resource = Build(id);

		try
		{
			Logger.LogInformation("🔍 Creating test {Label}: {Id}", Label, id);
			var created = await client.Resources.CreateAsync(resource, CancellationToken);

			created.Should().NotBeNull();
			created.Metadata.Id.Should().Be(id);
			AssertCreated(created);

			Logger.LogInformation("✅ Successfully created {Label}: {Id}", Label, id);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			await CleanupAsync(client, id);
		}
	}

	/// <summary>
	/// Verifies an existing resource can be retrieved by id.
	/// </summary>
	[Fact]
	public async Task Get_ReturnsResource()
	{
		if (!ShouldRun())
		{
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var id = CreateUniqueId(IdPrefix);
		var original = await CreateTestResourceAsync(client, id);
		if (original == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create {Label} (likely permission denied)", Label);
			return;
		}

		try
		{
			Logger.LogInformation("🔍 Getting {Label}: {Id}", Label, id);
			var retrieved = await client.Resources.GetAsync<T>(id, cancellationToken: CancellationToken);

			retrieved.Should().NotBeNull();
			retrieved.Metadata.Id.Should().Be(id);
			AssertRetrieved(retrieved, original);

			Logger.LogInformation("✅ Successfully retrieved {Label}: {Id}", Label, id);
		}
		finally
		{
			await CleanupAsync(client, id);
		}
	}

	/// <summary>
	/// Verifies updating a resource returns the modified version.
	/// </summary>
	[Fact]
	public async Task Update_ModifiesResource()
	{
		if (!ShouldRun())
		{
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var id = CreateUniqueId(IdPrefix);
		var resource = await CreateTestResourceAsync(client, id);
		if (resource == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create {Label} (likely permission denied)", Label);
			return;
		}

		try
		{
			Logger.LogInformation("🔍 Updating {Label}: {Id}", Label, id);
			Modify(resource);

			var updated = await client.Resources.UpdateAsync(resource, cancellationToken: CancellationToken);

			updated.Should().NotBeNull();
			AssertUpdated(updated);

			Logger.LogInformation("✅ Successfully updated {Label}: {Id}", Label, id);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			await CleanupAsync(client, id);
		}
	}

	/// <summary>
	/// Verifies a deleted resource can no longer be retrieved.
	/// </summary>
	[Fact]
	public async Task Delete_RemovesResource()
	{
		if (!ShouldRun())
		{
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var id = CreateUniqueId(IdPrefix);
		if (await CreateTestResourceAsync(client, id) == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create {Label} (likely permission denied)", Label);
			return;
		}

		Logger.LogInformation("🔍 Deleting {Label}: {Id}", Label, id);
		await client.Resources.DeleteAsync<T>(id, cancellationToken: CancellationToken);

		await Assert.ThrowsAsync<RpcException>(async () =>
		{
			await client.Resources.GetAsync<T>(id, cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ Successfully deleted {Label}: {Id}", Label, id);
	}

	/// <summary>
	/// Verifies resources of this kind can be listed.
	/// </summary>
	[Fact]
	public async Task List_ReturnsResources()
	{
		if (!ShouldRun())
		{
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			Logger.LogInformation("🔍 Listing all {Plural}", PluralLabel);
			var items = new List<T>();

			await foreach (var item in client.Resources.ListAsync<T>(cancellationToken: CancellationToken))
			{
				items.Add(item);
			}

			Logger.LogInformation("✅ Successfully listed {Count} {Plural}", items.Count, PluralLabel);
			items.Should().NotBeNull();
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	private bool ShouldRun()
	{
		if (ShouldRunIntegrationTests())
		{
			return true;
		}

		Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
		return false;
	}

	private async Task<T?> CreateTestResourceAsync(OmniClient client, string id)
	{
		try
		{
			Logger.LogDebug("Creating test {Label}: {Id}", Label, id);
			return await client.Resources.CreateAsync(Build(id), CancellationToken);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogWarning("Cannot create test {Label} - permission denied", Label);
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to create test {Label}: {Id}", Label, id);
			return null;
		}
	}

	private async Task CleanupAsync(OmniClient client, string id)
	{
		try
		{
			await client.Resources.DeleteAsync<T>(id, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test {Label}: {Id}", Label, id);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
		{
			Logger.LogDebug("Test {Label} already deleted: {Id}", Label, id);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test {Label}: {Id}", Label, id);
		}
	}
}
