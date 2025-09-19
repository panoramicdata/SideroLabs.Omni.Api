namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Service account information
/// </summary>
public class ServiceAccountInfo
{
	public string Name { get; set; } = "";
	public IReadOnlyList<PgpPublicKeyInfo> PgpPublicKeys { get; set; } = [];
	public string Role { get; set; } = "";
}
