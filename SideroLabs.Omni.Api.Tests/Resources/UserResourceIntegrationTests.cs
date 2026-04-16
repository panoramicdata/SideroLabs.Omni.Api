using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for User and Identity resource CRUD operations
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class UserResourceIntegrationTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that creates a user, reads it back, updates its role, lists all users, and then deletes it.
	/// </summary>
	/// <summary>
	/// Integration test that creates a user, reads it back, updates its role, lists all users, and then deletes it.
	/// </summary>
	[Fact]
	public async Task User_FullCRUDLifecycle_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var testEmail = GenerateTestEmail();
		User? createdUser = null;
		Identity? createdIdentity = null;

		try
		{
			Logger.LogInformation("🚀 Starting User CRUD Lifecycle test for: {Email}", testEmail);

			// ACT 1: CREATE
			Logger.LogInformation("📝 Step 1: Creating user with Reader role");
			var (user, identity) = await OmniClient.Users.CreateAsync(
				testEmail,
				"Reader",
				CancellationToken);

			createdUser = user;
			createdIdentity = identity;

			// ASSERT CREATE
			Logger.LogInformation("✓ User created: {UserId}", user.Metadata.Id);
			AssertResourceCreated(user);
			AssertResourceCreated(identity);
			Assert.Equal("Reader", user.Spec.Role);
			Assert.Equal(testEmail, identity.Spec.UserId);

			// ACT 2: READ
			Logger.LogInformation("📖 Step 2: Reading user by email");
			var retrievedUser = await OmniClient.Users.GetAsync(
				testEmail,
				CancellationToken);

			// ASSERT READ
			Assert.NotNull(retrievedUser);
			Assert.Equal(user.Metadata.Id, retrievedUser.UserId);
			Assert.Equal("Reader", retrievedUser.Role);
			Logger.LogInformation("✓ User retrieved successfully");

			// ACT 3: UPDATE
			Logger.LogInformation("🔄 Step 3: Updating user role to Operator");
			var updatedUser = await OmniClient.Users.SetRoleAsync(
				testEmail,
				"Operator",
				CancellationToken);

			// ASSERT UPDATE
			Assert.Equal("Operator", updatedUser.Spec.Role);
			Assert.Equal(user.Metadata.Id, updatedUser.Metadata.Id);
			AssertResourceUpdated(user, updatedUser);
			Logger.LogInformation("✓ User role updated successfully");

			// ACT 4: LIST
			Logger.LogInformation("📋 Step 4: Listing all users");
			var allUsers = await OmniClient.Users.ListAsync(CancellationToken);

			// ASSERT LIST
			Assert.NotEmpty(allUsers);
			Assert.Contains(allUsers, u => u.Email == testEmail);
			Logger.LogInformation("✓ User found in list (total users: {Count})", allUsers.Count);

			// ACT 5: DELETE
			Logger.LogInformation("🗑️ Step 5: Deleting user");
			await OmniClient.Users.DeleteAsync(testEmail, CancellationToken);

			// ASSERT DELETE
			await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
				await OmniClient.Users.GetAsync(testEmail, CancellationToken));

			Logger.LogInformation("✓ User deleted successfully");
			Logger.LogInformation("✅ Full CRUD Lifecycle completed successfully!");

			// Clear references to prevent double-delete in finally
			createdUser = null;
			createdIdentity = null;
		}
		finally
		{
			// CLEANUP - Even if test fails
			if (createdUser != null || createdIdentity != null)
			{
				await SafeDeleteUserAsync(testEmail);
			}
		}
	}

	/// <summary>
	/// Verifies that a user can be created using the builder pattern and the resource is persisted correctly.
	/// </summary>
	[Fact]
	public async Task User_Create_WithBuilder_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var testEmail = GenerateTestEmail();
		var userId = GenerateUniqueId("user");

		try
		{
			Logger.LogInformation("🚀 Creating user via Builder pattern");

			// Create using builders
			var user = new UserBuilder()
				.WithUserId(userId)
				.AsOperator()
				.WithLabel("test", "builder")
				.Build();

			var identity = new IdentityBuilder(testEmail)
				.ForUser(user)
				.AsUserType()
				.Build();

			// Act - Create via Resources API
			var createdUser = await OmniClient.Resources.CreateAsync(user, CancellationToken);
			var createdIdentity = await OmniClient.Resources.CreateAsync(identity, CancellationToken);

			// Assert
			AssertResourceCreated(createdUser);
			AssertResourceCreated(createdIdentity);
			Assert.Equal("Operator", createdUser.Spec.Role);
			Assert.Equal(testEmail, createdIdentity.Spec.UserId);
			Assert.Equal("builder", createdUser.Metadata.Labels["test"]);

			Logger.LogInformation("✅ User created successfully via Builder pattern");
		}
		finally
		{
			await SafeDeleteUserAsync(testEmail);
		}
	}

	/// <summary>
	/// Verifies that when a user update fails midway, cleanup still removes the user resources correctly.
	/// </summary>
	[Fact]
	public async Task User_Update_FailsMidway_CleansUpProperly()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Test that cleanup works even if update throws exception
		var testEmail = GenerateTestEmail();

		try
		{
			Logger.LogInformation("🚀 Testing cleanup on failed update");

			// Create user
			var (user, identity) = await OmniClient.Users.CreateAsync(
				testEmail,
				"Reader",
				CancellationToken);

			Logger.LogInformation("✓ User created: {UserId}", user.Metadata.Id);

			// Force an error during update (empty role should fail validation)
			try
			{
				await OmniClient.Users.SetRoleAsync(testEmail, "", CancellationToken);
				Assert.Fail("Expected ArgumentException for empty role");
			}
			catch (ArgumentException)
			{
				Logger.LogInformation("✓ Update failed as expected with invalid role");
			}

			// User should still exist
			var stillExists = await OmniClient.Users.GetAsync(testEmail, CancellationToken);
			Assert.NotNull(stillExists);
			Assert.Equal("Reader", stillExists.Role); // Should still be Reader

			Logger.LogInformation("✅ User persists correctly after failed update");
		}
		finally
		{
			await SafeDeleteUserAsync(testEmail);
		}
	}

	/// <summary>
	/// Verifies that creating a user with an invalid role name throws an <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public async Task User_CreateWithInvalidRole_ThrowsException()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var testEmail = GenerateTestEmail();

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(async () =>
			await OmniClient.Users.CreateAsync(testEmail, "InvalidRole", CancellationToken));

		Logger.LogInformation("✅ Invalid role correctly rejected");
	}

	/// <summary>
	/// Verifies that creating a user with an email that already exists throws a gRPC <see cref="Grpc.Core.RpcException"/>.
	/// </summary>
	[Fact]
	public async Task User_CreateDuplicate_ThrowsException()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var testEmail = GenerateTestEmail();

		try
		{
			// Create first user
			await OmniClient.Users.CreateAsync(testEmail, "Reader", CancellationToken);

			// Act & Assert - Try to create duplicate
			await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
				await OmniClient.Users.CreateAsync(testEmail, "Reader", CancellationToken));

			Logger.LogInformation("✅ Duplicate user creation correctly rejected");
		}
		finally
		{
			await SafeDeleteUserAsync(testEmail);
		}
	}

	/// <summary>
	/// Verifies that retrieving a non-existent user throws a <see cref="Grpc.Core.RpcException"/> with a NotFound status.
	/// </summary>
	[Fact]
	public async Task User_GetNonExistent_ThrowsNotFoundException()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var nonExistentEmail = $"nonexistent-{GenerateUniqueId()}@test.com";

		// Act & Assert
		await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
			await OmniClient.Users.GetAsync(nonExistentEmail, CancellationToken));

		Logger.LogInformation("✅ Non-existent user correctly returns NotFound");
	}

	/// <summary>
	/// Verifies that deleting a non-existent user completes successfully (idempotent delete).
	/// </summary>
	[Fact]
	public async Task User_DeleteNonExistent_Succeeds()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var nonExistentEmail = $"nonexistent-{GenerateUniqueId()}@test.com";

		// Act - Should not throw
		await OmniClient.Users.DeleteAsync(nonExistentEmail, CancellationToken);

		Logger.LogInformation("✅ Deleting non-existent user succeeds (idempotent)");
	}

	/// <summary>
	/// Verifies that a user can be created with each of the supported roles (Admin, Operator, Reader, None).
	/// </summary>
	/// <param name="role">The role to assign to the user being created.</param>
	[Theory]
	[InlineData("Admin")]
	[InlineData("Operator")]
	[InlineData("Reader")]
	[InlineData("None")]
	public async Task User_CreateWithEachRole_Success(string role)
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var testEmail = GenerateTestEmail();

		try
		{
			Logger.LogInformation("🚀 Testing role: {Role}", role);

			// Act
			var (user, identity) = await OmniClient.Users.CreateAsync(
				testEmail,
				role,
				CancellationToken);

			// Assert
			Assert.Equal(role, user.Spec.Role);
			Logger.LogInformation("✅ User created successfully with role: {Role}", role);
		}
		finally
		{
			await SafeDeleteUserAsync(testEmail);
		}
	}

	/// <summary>
	/// Verifies that listing all users returns a non-empty collection of user records.
	/// </summary>
	[Fact]
	public async Task User_List_ReturnsAllUsers()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Act
		var users = await OmniClient.Users.ListAsync(CancellationToken);

		// Assert
		Assert.NotEmpty(users);
		Logger.LogInformation("✅ Listed {Count} users", users.Count);

		// Verify each user has valid data
		foreach (var userInfo in users)
		{
			Assert.NotNull(userInfo);
			Assert.NotEmpty(userInfo.UserId);
			Assert.NotEmpty(userInfo.Email);
		}
	}
}
