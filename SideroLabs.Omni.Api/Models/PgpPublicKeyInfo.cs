namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// PGP public key information
/// </summary>
public class PgpPublicKeyInfo
{
	public string Id { get; set; } = "";
	public string Armored { get; set; } = "";
	public DateTimeOffset Expiration { get; set; }
}
