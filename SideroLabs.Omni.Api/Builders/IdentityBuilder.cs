using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Builders;

/// <summary>
/// Builder for Identity resources
/// </summary>
public class IdentityBuilder
{
	private readonly Identity _identity;

	/// <summary>
	/// Initializes a new instance of the <see cref="IdentityBuilder"/> class
	/// </summary>
	/// <param name="email">Email address for the identity</param>
	public IdentityBuilder(string email)
	{
		_identity = new Identity
		{
			Metadata = new ResourceMetadata
			{
				Namespace = "default",
				Id = email
			},
			Spec = new IdentitySpec()
		};
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IdentityBuilder"/> class with an existing identity
	/// </summary>
	/// <param name="identity">Existing identity to build from</param>
	public IdentityBuilder(Identity identity)
	{
		_identity = identity ?? throw new ArgumentNullException(nameof(identity));
	}

	/// <summary>
	/// Sets the email address
	/// </summary>
	/// <param name="email">Email address</param>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder WithEmail(string email)
	{
		_identity.Metadata.Id = email;
		return this;
	}

	/// <summary>
	/// Sets the namespace
	/// </summary>
	/// <param name="namespace">Namespace</param>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder WithNamespace(string @namespace)
	{
		_identity.Metadata.Namespace = @namespace;
		return this;
	}

	/// <summary>
	/// Sets the associated user ID
	/// </summary>
	/// <param name="userId">User ID to associate with</param>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder ForUser(string userId)
	{
		_identity.Spec.UserId = userId;
		_identity.Metadata.Labels[Identity.LabelUserID] = userId;
		return this;
	}

	/// <summary>
	/// Sets the associated user
	/// </summary>
	/// <param name="user">User to associate with</param>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder ForUser(User user)
	{
		if (user == null)
			throw new ArgumentNullException(nameof(user));

		return ForUser(user.UserId);
	}

	/// <summary>
	/// Marks this identity as a regular user (not a service account)
	/// </summary>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder AsUserType()
	{
		_identity.Metadata.Labels[Identity.LabelType] = Identity.TypeUser;
		return this;
	}

	/// <summary>
	/// Marks this identity as a service account
	/// </summary>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder AsServiceAccount()
	{
		_identity.Metadata.Labels[Identity.LabelType] = Identity.TypeServiceAccount;
		return this;
	}

	/// <summary>
	/// Adds a label to the identity
	/// </summary>
	/// <param name="key">Label key</param>
	/// <param name="value">Label value</param>
	/// <returns>Builder for method chaining</returns>
	public IdentityBuilder WithLabel(string key, string value)
	{
		_identity.Metadata.Labels[key] = value;
		return this;
	}

	/// <summary>
	/// Builds the Identity resource
	/// </summary>
	/// <returns>Configured Identity resource</returns>
	public Identity Build() => _identity;
}
