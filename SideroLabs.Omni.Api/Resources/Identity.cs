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
	public static string ResourceType { get; } = "Identities.omni.sidero.dev";

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
	public static string LabelUserID { get; } = "identity.omni.sidero.dev/user-id";

	/// <summary>
	/// Label key for identity type
	/// </summary>
	public static string LabelType { get; } = "identity.omni.sidero.dev/type";

	/// <summary>
	/// Identity type value for regular users
	/// </summary>
	public static string TypeUser { get; } = "user";

	/// <summary>
	/// Identity type value for service accounts
	/// </summary>
	public static string TypeServiceAccount { get; } = "service-account";
}
