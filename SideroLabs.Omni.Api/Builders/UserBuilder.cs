using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Builders;

/// <summary>
/// Builder for User resources
/// </summary>
public class UserBuilder
{
	private readonly User _user;

	/// <summary>
	/// Initializes a new instance of the <see cref="UserBuilder"/> class
	/// </summary>
	public UserBuilder()
	{
		_user = new User
		{
			Metadata = new ResourceMetadata
			{
				Namespace = "default",
				Id = Guid.NewGuid().ToString()
			},
			Spec = new UserSpec()
		};
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UserBuilder"/> class with an existing user
	/// </summary>
	/// <param name="user">Existing user to build from</param>
	public UserBuilder(User user)
	{
		_user = user ?? throw new ArgumentNullException(nameof(user));
	}

	/// <summary>
	/// Sets the user ID
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder WithUserId(string userId)
	{
		_user.Metadata.Id = userId;
		return this;
	}

	/// <summary>
	/// Sets the namespace
	/// </summary>
	/// <param name="namespace">Namespace</param>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder WithNamespace(string @namespace)
	{
		_user.Metadata.Namespace = @namespace;
		return this;
	}

	/// <summary>
	/// Sets the user role
	/// </summary>
	/// <param name="role">Role (e.g., Admin, Operator, Reader, None)</param>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder WithRole(string role)
	{
		_user.Spec.Role = role;
		return this;
	}

	/// <summary>
	/// Sets the user as Admin role
	/// </summary>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder AsAdmin()
	{
		_user.Spec.Role = "Admin";
		return this;
	}

	/// <summary>
	/// Sets the user as Operator role
	/// </summary>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder AsOperator()
	{
		_user.Spec.Role = "Operator";
		return this;
	}

	/// <summary>
	/// Sets the user as Reader role
	/// </summary>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder AsReader()
	{
		_user.Spec.Role = "Reader";
		return this;
	}

	/// <summary>
	/// Adds a label to the user
	/// </summary>
	/// <param name="key">Label key</param>
	/// <param name="value">Label value</param>
	/// <returns>Builder for method chaining</returns>
	public UserBuilder WithLabel(string key, string value)
	{
		_user.Metadata.Labels[key] = value;
		return this;
	}

	/// <summary>
	/// Builds the User resource
	/// </summary>
	/// <returns>Configured User resource</returns>
	public User Build() => _user;
}
