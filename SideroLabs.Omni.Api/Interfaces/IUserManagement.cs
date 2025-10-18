using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for user management operations
/// Provides high-level operations for managing users via COSI resources
/// </summary>
public interface IUserManagement
{
	/// <summary>
	/// Creates a new user with the specified email and role
	/// </summary>
	/// <param name="email">User email address</param>
	/// <param name="role">User role (Admin, Operator, Reader, None)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The created user and identity resources</returns>
	Task<(User User, Identity Identity)> CreateAsync(
		string email,
		string role,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists all users (excluding service accounts)
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>List of user information</returns>
	Task<List<UserInfo>> ListAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a user by email address
	/// </summary>
	/// <param name="email">User email address</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>User information</returns>
	Task<UserInfo> GetAsync(string email, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a user by email address
	/// </summary>
	/// <param name="email">User email address</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task DeleteAsync(string email, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates a user's role
	/// </summary>
	/// <param name="email">User email address</param>
	/// <param name="role">New role (Admin, Operator, Reader, None)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Updated user</returns>
	Task<User> SetRoleAsync(string email, string role, CancellationToken cancellationToken = default);
}

/// <summary>
/// User information combining User and Identity data
/// </summary>
public class UserInfo
{
	/// <summary>
	/// Gets or sets the user ID
	/// </summary>
	public string UserId { get; set; } = "";

	/// <summary>
	/// Gets or sets the email address
	/// </summary>
	public string Email { get; set; } = "";

	/// <summary>
	/// Gets or sets the user role
	/// </summary>
	public string Role { get; set; } = "";

	/// <summary>
	/// Gets or sets the user resource
	/// </summary>
	public User? User { get; set; }

	/// <summary>
	/// Gets or sets the identity resource
	/// </summary>
	public Identity? Identity { get; set; }
}
