namespace SideroLabs.Omni.Api;

/// <summary>
/// Configuration options for the Omni Client
/// </summary>
public class OmniClientOptions
{
	/// <summary>
	/// The gRPC endpoint URL for the Omni Management API
	/// </summary>
	public string Endpoint { get; set; } = string.Empty;

	/// <summary>
	/// Authentication token for the API
	/// </summary>
	public string? AuthToken { get; set; }

	/// <summary>
	/// Timeout for gRPC calls in seconds
	/// </summary>
	public int TimeoutSeconds { get; set; } = 30;

	/// <summary>
	/// Whether to use TLS for the connection
	/// </summary>
	public bool UseTls { get; set; } = true;

	/// <summary>
	/// Whether to validate the server certificate
	/// </summary>
	public bool ValidateCertificate { get; set; } = true;
}
