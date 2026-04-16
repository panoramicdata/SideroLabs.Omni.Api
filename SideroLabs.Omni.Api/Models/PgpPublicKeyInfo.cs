namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// PGP public key information
/// </summary>
public class PgpPublicKeyInfo
{
	/// <summary>
	/// Gets or sets the Omni-assigned identifier for the public key.
	/// </summary>
	public string Id { get; set; } = "";

	/// <summary>
	/// Gets or sets the ASCII-armored OpenPGP public key material.
	/// </summary>
	public string Armored { get; set; } = "";

	/// <summary>
	/// Gets or sets the public key expiration timestamp in UTC offset form.
	/// </summary>
	public DateTimeOffset Expiration { get; set; }
}
