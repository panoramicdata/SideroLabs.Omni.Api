namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Identity resource links an email address to a User
/// </summary>
public class Identity : OmniResource<IdentitySpec, IdentityStatus>
{
	/// <inheritdoc />
	public override string Kind => "Identity";

	/// <inheritdoc />
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Gets the resource type for Identities
	/// </summary>
	public const string ResourceType = "Identities.omni.sidero.dev";

	/// <summary>
	/// Gets the email address (same as resource ID)
	/// </summary>
	public string Email => Metadata.Id;

	/// <summary>
	/// Gets the associated user ID
	/// </summary>
	public string UserId => Spec.UserId;

	/// <summary>
	/// Label key for user ID association
	/// </summary>
	public const string LabelUserID = "identity.omni.sidero.dev/user-id";

	/// <summary>
	/// Label key for identity type
	/// </summary>
	public const string LabelType = "identity.omni.sidero.dev/type";

	/// <summary>
	/// Identity type value for regular users
	/// </summary>
	public const string TypeUser = "user";

	/// <summary>
	/// Identity type value for service accounts
	/// </summary>
	public const string TypeServiceAccount = "service-account";
}
