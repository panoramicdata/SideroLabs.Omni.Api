using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
	/// The identity (username) for PGP authentication
	/// </summary>
	public string? Identity { get; set; }

	/// <summary>
	/// The PGP private key content (armored format)
	/// </summary>
	public string? PgpPrivateKey { get; set; }

	/// <summary>
	/// Path to the PGP private key file for authentication (alternative to PgpPrivateKey)
	/// This file should contain the base64-encoded JSON with 'name' and 'pgp_key' properties
	/// </summary>
	public string? PgpKeyFilePath { get; set; }

	/// <summary>
	/// Timeout for gRPC calls in seconds
	/// </summary>
	public int TimeoutSeconds { get; set; } = 30;

	/// <summary>
	/// Whether to use Transport Layer Security for the connection
	/// </summary>
	public bool UseTls { get; set; } = true;

	/// <summary>
	/// Whether to validate the server certificate
	/// Set to false only for testing with self-signed certificates
	/// </summary>
	public bool ValidateCertificate { get; set; } = true;

	/// <summary>
	/// Whether the client should operate in read-only mode
	/// When true, write operations (create, update, delete) will throw ReadOnlyModeException
	/// </summary>
	public bool IsReadOnly { get; set; } = false;

	/// <summary>
	/// The logger
	/// </summary>
	public ILogger Logger { get; set; } = NullLogger.Instance;
}
