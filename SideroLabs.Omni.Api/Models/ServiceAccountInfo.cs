namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Service account information
/// </summary>
public class ServiceAccountInfo
{
	/// <summary>
	/// Gets or sets the Omni service account name.
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// Gets or sets the PGP public keys currently associated with the service account.
	/// </summary>
	public IReadOnlyList<PgpPublicKeyInfo> PgpPublicKeys { get; set; } = [];

	/// <summary>
	/// Gets or sets the effective Omni role assigned to the service account.
	/// </summary>
	public string Role { get; set; } = "";
}
