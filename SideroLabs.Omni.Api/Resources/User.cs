namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// User resource represents a user in the Omni system
/// </summary>
public class User : OmniResource<UserSpec, UserStatus>
{
	/// <inheritdoc />
	public override string Kind => "User";

	/// <inheritdoc />
	public override string ApiVersion => "omni.sidero.dev/v1alpha1";

	/// <summary>
	/// Gets the resource type for Users
	/// </summary>
	public const string ResourceType = "Users.omni.sidero.dev";

	/// <summary>
	/// Gets the user ID (same as resource ID)
	/// </summary>
	public string UserId => Metadata.Id;

	/// <summary>
	/// Gets the user role
	/// </summary>
	public string Role => Spec.Role;
}
