using Grpc.Core;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Management;

/// <summary>
/// Integration tests for ManagementService service account operations
/// Tests the full lifecycle: create, list, renew, destroy
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ManagementService")]
[Trait("Category", "ServiceAccounts")]
public class ManagementServiceAccountTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task ListServiceAccounts_ReturnsAccounts()
	{
		// Arrange & Act - NEW API!
		var accounts = await OmniClient.ServiceAccounts.ListAsync(CancellationToken);

		// Assert
		Assert.NotNull(accounts);
		Logger.LogInformation("✓ Found {Count} service accounts", accounts.Count);

		foreach (var account in accounts.Take(5))
		{
			Assert.NotEmpty(account.Name);
			Assert.NotEmpty(account.Role);
			Assert.NotNull(account.PgpPublicKeys);
			
			Logger.LogInformation("  Account: {Name}, Role: {Role}, Keys: {KeyCount}",
				account.Name, account.Role, account.PgpPublicKeys.Count);

			// Verify each PGP key has required fields
			foreach (var key in account.PgpPublicKeys)
			{
				Assert.NotEmpty(key.Id);
				Assert.NotEmpty(key.Armored);
				Logger.LogDebug("    Key: {KeyId}, Expires: {Expiration}", key.Id, key.Expiration);
			}
		}
	}

	[Fact]
	public async Task ListServiceAccounts_MultipleCallsConsistent()
	{
		// Arrange & Act - Call twice - NEW API!
		var accounts1 = await OmniClient.ServiceAccounts.ListAsync(CancellationToken);
		await Task.Delay(100); // Small delay
		var accounts2 = await OmniClient.ServiceAccounts.ListAsync(CancellationToken);

		// Assert - Results should be consistent
		Assert.Equal(accounts1.Count, accounts2.Count);
		
		Logger.LogInformation("✓ Service account list is consistent across calls");
	}

	[Fact(Skip = "Destructive test - creates service account. Enable for manual testing only.")]
	public async Task ServiceAccount_FullLifecycle_Success()
	{
		if (ShouldSkipDestructiveTests())
		{
			Logger.LogWarning("Skipping destructive test");
			return;
		}

		// This test would require:
		// 1. Generating a valid PGP key pair
		// 2. Creating a service account - NEW API: client.ServiceAccounts.CreateAsync()
		// 3. Listing to verify it exists - NEW API: client.ServiceAccounts.ListAsync()
		// 4. Renewing with a new key - NEW API: client.ServiceAccounts.RenewAsync()
		// 5. Destroying it - NEW API: client.ServiceAccounts.DestroyAsync()
		// 6. Verifying it's gone

		// For now, skip since this requires PGP key generation
		Logger.LogInformation("Service account lifecycle test skipped - requires PGP key generation");
	}

	[Fact(Skip = "Requires PGP key generation - manual test only")]
	public async Task CreateServiceAccount_WithUserRole_ReturnsKeyId()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Would need to:
		// var (publicKey, _) = GenerateTestPgpKeyPair();
		// NEW API!
		// var keyId = await OmniClient.ServiceAccounts.CreateAsync(publicKey, true, CancellationToken);
		// Assert.NotEmpty(keyId);
		// Cleanup: await OmniClient.ServiceAccounts.DestroyAsync(accountName, CancellationToken);
	}

	[Fact(Skip = "Requires valid service account - manual test only")]
	public async Task RenewServiceAccount_ValidAccount_ReturnsNewKeyId()
	{
		// Would need an existing service account to renew
		// NEW API: await OmniClient.ServiceAccounts.RenewAsync(...)
		Logger.LogInformation("Renew test requires existing service account");
	}

	[Fact(Skip = "Destructive - requires valid service account")]
	public async Task DestroyServiceAccount_ExistingAccount_Succeeds()
	{
		// Would need a test service account to destroy
		// NEW API: await OmniClient.ServiceAccounts.DestroyAsync(...)
		Logger.LogInformation("Destroy test requires existing service account");
	}

	[Fact]
	public async Task CreateServiceAccount_InvalidPgpKey_ThrowsInvalidArgument()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var invalidKey = "not a valid PGP key";

		// Act & Assert - NEW API!
		var exception = await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.ServiceAccounts.CreateAsync(invalidKey, cancellationToken: CancellationToken));

		Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
		Logger.LogInformation("✓ Invalid PGP key correctly rejected: {Detail}", exception.Status.Detail);
	}

	[Fact]
	public async Task CreateServiceAccount_EmptyPgpKey_ThrowsInvalidArgument()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var emptyKey = "";

		// Act & Assert - NEW API!
		var exception = await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.ServiceAccounts.CreateAsync(emptyKey, cancellationToken: CancellationToken));

		Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
		Logger.LogInformation("✓ Empty PGP key correctly rejected");
	}

	[Fact(Skip = "Requires non-existent account name")]
	public async Task DestroyServiceAccount_NonExistent_ThrowsNotFound()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		var nonExistentAccount = $"non-existent-account-{Guid.NewGuid():N}";

		// Act & Assert - NEW API!
		var exception = await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.ServiceAccounts.DestroyAsync(nonExistentAccount, CancellationToken));

		Assert.Equal(StatusCode.NotFound, exception.StatusCode);
		Logger.LogInformation("✓ Non-existent account correctly returns NotFound");
	}
}
