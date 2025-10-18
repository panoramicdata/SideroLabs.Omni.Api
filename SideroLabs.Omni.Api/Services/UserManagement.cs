using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Service for managing users via COSI resources
/// </summary>
internal class UserManagement : IUserManagement
{
	private readonly IOmniResourceClient _resources;
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="UserManagement"/> class
	/// </summary>
	/// <param name="resources">Resource client</param>
	/// <param name="logger">Logger</param>
	public UserManagement(IOmniResourceClient resources, ILogger logger)
	{
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc />
	public async Task<(User User, Identity Identity)> CreateAsync(
		string email,
		string role,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email is required", nameof(email));
		
		if (string.IsNullOrWhiteSpace(role))
			throw new ArgumentException("Role is required", nameof(role));

		_logger.LogInformation("Creating user with email {Email} and role {Role}", email, role);

		// Create user resource
		var user = new User
		{
			Metadata = new ResourceMetadata
			{
				Namespace = "default",
				Id = Guid.NewGuid().ToString()
			},
			Spec = new UserSpec { Role = role }
		};

		// Create identity resource
		var identity = new Identity
		{
			Metadata = new ResourceMetadata
			{
				Namespace = "default",
				Id = email,
				Labels =
				{
					[Identity.LabelUserID] = user.Metadata.Id,
					[Identity.LabelType] = Identity.TypeUser
				}
			},
			Spec = new IdentitySpec { UserId = user.Metadata.Id }
		};

		// Create both resources
		await _resources.CreateAsync(user, cancellationToken);
		_logger.LogDebug("Created user resource with ID {UserId}", user.UserId);

		await _resources.CreateAsync(identity, cancellationToken);
		_logger.LogDebug("Created identity resource for email {Email}", email);

		return (user, identity);
	}

	/// <inheritdoc />
	public async Task<List<UserInfo>> ListAsync(CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Listing all users");

		var users = new List<UserInfo>();

		// List identities (excluding service accounts)
		var identities = _resources.ListAsync<Identity>(
			selector: $"{Identity.LabelType}={Identity.TypeUser}",
			cancellationToken: cancellationToken);

		await foreach (var identity in identities)
		{
			try
			{
				// Get corresponding user
				var user = await _resources.GetAsync<User>(
					identity.Spec.UserId,
					cancellationToken: cancellationToken);

				users.Add(new UserInfo
				{
					UserId = user.UserId,
					Email = identity.Email,
					Role = user.Spec.Role,
					User = user,
					Identity = identity
				});
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to get user for identity {Email}", identity.Email);
			}
		}

		_logger.LogInformation("Found {Count} users", users.Count);
		return users;
	}

	/// <inheritdoc />
	public async Task<UserInfo> GetAsync(string email, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email is required", nameof(email));

		_logger.LogInformation("Getting user with email {Email}", email);

		var identity = await _resources.GetAsync<Identity>(email, cancellationToken: cancellationToken);
		var user = await _resources.GetAsync<User>(identity.Spec.UserId, cancellationToken: cancellationToken);

		return new UserInfo
		{
			UserId = user.UserId,
			Email = identity.Email,
			Role = user.Spec.Role,
			User = user,
			Identity = identity
		};
	}

	/// <inheritdoc />
	public async Task DeleteAsync(string email, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email is required", nameof(email));

		_logger.LogInformation("Deleting user with email {Email}", email);

		// Get identity to find user ID
		var identity = await _resources.GetAsync<Identity>(email, cancellationToken: cancellationToken);
		var userId = identity.Spec.UserId;

		// Delete both resources
		await _resources.DeleteAsync<Identity>(email, cancellationToken: cancellationToken);
		_logger.LogDebug("Deleted identity for email {Email}", email);

		await _resources.DeleteAsync<User>(userId, cancellationToken: cancellationToken);
		_logger.LogDebug("Deleted user with ID {UserId}", userId);
	}

	/// <inheritdoc />
	public async Task<User> SetRoleAsync(string email, string role, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email is required", nameof(email));
		
		if (string.IsNullOrWhiteSpace(role))
			throw new ArgumentException("Role is required", nameof(role));

		_logger.LogInformation("Setting role {Role} for user {Email}", role, email);

		// Get identity to find user ID
		var identity = await _resources.GetAsync<Identity>(email, cancellationToken: cancellationToken);
		
		// Get and update user
		var user = await _resources.GetAsync<User>(identity.Spec.UserId, cancellationToken: cancellationToken);
		user.Spec.Role = role;

		// Update user resource
		var updatedUser = await _resources.UpdateAsync(user, cancellationToken: cancellationToken);
		_logger.LogDebug("Updated role for user {UserId} to {Role}", user.UserId, role);

		return updatedUser;
	}
}
