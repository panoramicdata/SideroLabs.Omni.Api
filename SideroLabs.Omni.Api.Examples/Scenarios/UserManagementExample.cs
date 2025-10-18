using SideroLabs.Omni.Api;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Examples.Scenarios;

/// <summary>
/// Example demonstrating user management operations
/// </summary>
public static class UserManagementExample
{
	/// <summary>
	/// Demonstrates user management operations
	/// </summary>
	public static async Task DemonstrateAsync()
	{
		Console.WriteLine("=== User Management Example ===\n");

		// Configure client
		var options = new OmniClientOptions
		{
			BaseUrl = new Uri("https://omni.example.com"),
			Identity = "admin@example.com",
			PgpPrivateKey = "your-pgp-private-key"
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

		try
		{
			// Example 1: Create a user using the high-level API
			await CreateUserWithHelper(client, cts.Token);

			// Example 2: Create a user using builders and resource client directly
			await CreateUserWithBuilder(client, cts.Token);

			// Example 3: List all users
			await ListUsers(client, cts.Token);

			// Example 4: Update user role
			await UpdateUserRole(client, cts.Token);

			// Example 5: Delete a user
			await DeleteUser(client, cts.Token);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	/// <summary>
	/// Create user using the UserManagement helper service
	/// </summary>
	private static async Task CreateUserWithHelper(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("1. Creating user with helper service...");

		var (user, identity) = await client.Users.CreateAsync(
			"john.doe@example.com",
			"Operator",
			cancellationToken);

		Console.WriteLine($"   Created user: {user.UserId}");
		Console.WriteLine($"   Email: {identity.Email}");
		Console.WriteLine($"   Role: {user.Role}");
		Console.WriteLine();
	}

	/// <summary>
	/// Create user using builders and resource client directly
	/// </summary>
	private static async Task CreateUserWithBuilder(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("2. Creating user with builders...");

		// Build user resource
		var user = new UserBuilder()
			.WithUserId(Guid.NewGuid().ToString())
			.AsAdmin()  // Convenience method for Admin role
			.Build();

		// Build identity resource
		var identity = new IdentityBuilder("jane.smith@example.com")
			.ForUser(user)
			.AsUserType()  // Mark as regular user (not service account)
			.Build();

		// Create resources
		await client.Resources.CreateAsync(user, cancellationToken);
		await client.Resources.CreateAsync(identity, cancellationToken);

		Console.WriteLine($"   Created user: {user.UserId}");
		Console.WriteLine($"   Email: {identity.Email}");
		Console.WriteLine($"   Role: {user.Role}");
		Console.WriteLine();
	}

	/// <summary>
	/// List all users
	/// </summary>
	private static async Task ListUsers(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("3. Listing all users...");

		var users = await client.Users.ListAsync(cancellationToken);

		foreach (var userInfo in users)
		{
			Console.WriteLine($"   {userInfo.Email} ({userInfo.Role})");
		}

		Console.WriteLine($"   Total users: {users.Count}");
		Console.WriteLine();
	}

	/// <summary>
	/// Update user role
	/// </summary>
	private static async Task UpdateUserRole(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("4. Updating user role...");

		var updatedUser = await client.Users.SetRoleAsync(
			"john.doe@example.com",
			"Admin",
			cancellationToken);

		Console.WriteLine($"   Updated role for {updatedUser.UserId} to {updatedUser.Role}");
		Console.WriteLine();
	}

	/// <summary>
	/// Delete a user
	/// </summary>
	private static async Task DeleteUser(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("5. Deleting user...");

		await client.Users.DeleteAsync("jane.smith@example.com", cancellationToken);

		Console.WriteLine("   User deleted successfully");
		Console.WriteLine();
	}

	/// <summary>
	/// Advanced example: Direct resource manipulation
	/// </summary>
	public static async Task AdvancedResourceExample(OmniClient client, CancellationToken cancellationToken)
	{
		Console.WriteLine("=== Advanced Resource Example ===\n");

		// List identities directly (including service accounts)
		Console.WriteLine("Listing all identities...");
		await foreach (var identity in client.Resources.ListAsync<Identity>(cancellationToken: cancellationToken))
		{
			var type = identity.Metadata.Labels.TryGetValue(Identity.LabelType, out var t) ? t : "unknown";
			Console.WriteLine($"   {identity.Email} - Type: {type}, User: {identity.UserId}");
		}

		Console.WriteLine();

		// Watch for user changes
		Console.WriteLine("Watching for user changes (Ctrl+C to stop)...");
		await foreach (var evt in client.Resources.WatchAsync<User>(cancellationToken: cancellationToken))
		{
			Console.WriteLine($"   {evt.Type}: User {evt.Resource.UserId} - Role: {evt.Resource.Role}");
		}
	}
}
