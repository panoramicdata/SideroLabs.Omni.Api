using System.Text;
using System.Text.Json;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.Encoders;

namespace SideroLabs.Omni.Api.Security;

/// <summary>
/// Provides Omni authentication by signing gRPC requests using PGP private keys
/// This implements the Sidero Labs authentication mechanism as documented in:
/// https://github.com/siderolabs/go-api-signature
/// </summary>
public class OmniAuthenticator
{
	private const string SignatureVersion = "siderov1";
	private const string TimestampHeaderKey = "x-sidero-timestamp";
	private const string PayloadHeaderKey = "x-sidero-payload";
	private const string SignatureHeaderKey = "x-sidero-signature";

	private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	// Headers that are included in the signed payload (from go-api-signature)
	private static readonly string[] IncludedHeaders = [
		TimestampHeaderKey,
		"nodes",
		"selectors",
		"fieldSelectors",
		"runtime",
		"context",
		"cluster",
		"namespace",
		"uid",
		"authorization"
	];

	private readonly ILogger _logger;
	private readonly PgpSecretKey _secretKey;

	/// <summary>
	/// Creates a new OmniAuthenticator with PGP key content
	/// </summary>
	/// <param name="identity">The user identity</param>
	/// <param name="pgpPrivateKey">The PGP private key in armored format</param>
	/// <param name="logger">Logger instance</param>
	public OmniAuthenticator(string identity, string pgpPrivateKey, ILogger logger)
	{
		ArgumentNullException.ThrowIfNull(identity, nameof(identity));
		ArgumentNullException.ThrowIfNull(pgpPrivateKey, nameof(pgpPrivateKey));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		Identity = identity;
		_logger = logger;

		if (string.IsNullOrEmpty(pgpPrivateKey))
		{
			throw new ArgumentException("PGP private key cannot be null or empty", nameof(pgpPrivateKey));
		}

		(_secretKey, KeyFingerprint) = ParsePgpKey(pgpPrivateKey);

		_logger.LogDebug("Initialized Omni authenticator with identity: {Identity}, key fingerprint: {Fingerprint}",
			Identity, KeyFingerprint);
	}

	/// <summary>
	/// Creates a new OmniAuthenticator from a PGP key file
	/// </summary>
	/// <param name="pgpKeyFile">File containing base64-encoded JSON with name and pgp_key</param>
	/// <param name="logger">Logger instance</param>
	/// <returns>Configured OmniAuthenticator</returns>
	public static async Task<OmniAuthenticator> FromFileAsync(FileInfo pgpKeyFile, ILogger logger, CancellationToken cancellationToken = default)
	{
		if (!pgpKeyFile.Exists)
		{
			throw new FileNotFoundException($"PGP key file not found: {pgpKeyFile.FullName}");
		}

		var fileContents = await File.ReadAllTextAsync(pgpKeyFile.FullName, cancellationToken);

		byte[] decodedBytes;
		try
		{
			decodedBytes = Convert.FromBase64String(fileContents);
		}
		catch (FormatException ex)
		{
			logger.LogError(ex, "Failed to decode Base64 content from file");
			throw new InvalidOperationException("Invalid Base64 content in the file", ex);
		}

		var decodedString = Encoding.UTF8.GetString(decodedBytes);

		using var jsonDoc = JsonDocument.Parse(decodedString);
		var root = jsonDoc.RootElement;

		if (!root.TryGetProperty("name", out var nameElement))
		{
			throw new InvalidOperationException("Missing 'name' property in JSON content");
		}

		if (!root.TryGetProperty("pgp_key", out var pgpKeyElement))
		{
			throw new InvalidOperationException("Missing 'pgp_key' property in JSON content");
		}

		var identity = nameElement.GetString() ?? throw new InvalidOperationException("Name is null");
		var pgpPrivateKeyContent = pgpKeyElement.GetString() ?? throw new InvalidOperationException("PGP key value is null");

		return new OmniAuthenticator(identity, pgpPrivateKeyContent, logger);
	}

	/// <summary>
	/// Gets the identity from the PGP key
	/// </summary>
	public string Identity { get; }

	/// <summary>
	/// Gets the PGP key fingerprint
	/// </summary>
	public string KeyFingerprint { get; }

	/// <summary>
	/// Signs a gRPC request by adding authentication headers to the metadata
	/// This is the primary authentication method that Omni expects
	/// </summary>
	/// <param name="metadata">The gRPC metadata to sign</param>
	/// <param name="method">The gRPC method name (e.g., "/omni.management.ManagementService/ListClusters")</param>
	public void SignRequest(Metadata metadata, string method)
	{
		// Add timestamp
		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		metadata.Add(TimestampHeaderKey, timestamp.ToString());

		// Remove any existing signature/payload headers (for re-signing)
		RemoveHeader(metadata, PayloadHeaderKey);
		RemoveHeader(metadata, SignatureHeaderKey);

		// Build payload from metadata and method
		var payload = BuildPayload(metadata, method);
		var payloadJson = JsonSerializer.Serialize(payload, _jsonSerializerOptions);

		_logger.LogDebug("Signing payload: {Payload}", payloadJson);

		// Sign the payload
		var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
		var signatureBytes = SignData(payloadBytes);
		var signatureBase64 = Convert.ToBase64String(signatureBytes);

		// Add payload and signature headers
		metadata.Add(PayloadHeaderKey, payloadJson);
		metadata.Add(SignatureHeaderKey, $"{SignatureVersion} {Identity} {KeyFingerprint} {signatureBase64}");

		_logger.LogDebug("Signed gRPC request for method: {Method}", method);
	}

	/// <summary>
	/// Gets the authentication identity and key fingerprint information
	/// Used for testing and debugging purposes
	/// </summary>
	/// <returns>A string containing identity and fingerprint info</returns>
	public string GetAuthenticationInfo() => $"Identity: {Identity}, Fingerprint: {KeyFingerprint}";

	private (PgpSecretKey secretKey, string fingerprint) ParsePgpKey(string pgpPrivateKeyContent)
	{
		try
		{
			using var pgpKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(pgpPrivateKeyContent));
			var armoredInputStream = PgpUtilities.GetDecoderStream(pgpKeyStream);
			var pgpKeyRing = new PgpSecretKeyRing(armoredInputStream);

			// Find the signing key
			foreach (PgpSecretKey key in pgpKeyRing.GetSecretKeys())
			{
				if (key.IsSigningKey)
				{
					var fingerprint = Hex.ToHexString(key.PublicKey.GetFingerprint()).ToLowerInvariant();
					return (key, fingerprint);
				}
			}

			throw new InvalidOperationException("No suitable signing key found in PGP key ring");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to parse PGP key");
			throw new InvalidOperationException("Invalid PGP private key format", ex);
		}
	}

	private byte[] SignData(byte[] data)
	{
		// Extract the private key (assuming no passphrase)
		var privateKey = _secretKey.ExtractPrivateKey(null)
			?? throw new InvalidOperationException("Failed to extract private key from PGP secret key");

		// Create PGP signature
		var signatureGenerator = new PgpSignatureGenerator(_secretKey.PublicKey.Algorithm, HashAlgorithmTag.Sha256);
		signatureGenerator.InitSign(PgpSignature.BinaryDocument, privateKey);
		signatureGenerator.Update(data);

		var signature = signatureGenerator.Generate();

		// Encode the signature
		using var signatureStream = new MemoryStream();
		signature.Encode(signatureStream);

		return signatureStream.ToArray();
	}

	private static object BuildPayload(Metadata metadata, string method)
	{
		var headers = new Dictionary<string, string[]>();

		// Include only the headers that Omni expects to be signed
		foreach (var headerName in IncludedHeaders)
		{
			var values = metadata.Where(entry =>
				string.Equals(entry.Key, headerName, StringComparison.OrdinalIgnoreCase))
				.Select(entry => entry.Value)
				.ToArray();

			if (values.Length > 0)
			{
				headers[headerName] = values;
			}
		}

		return new
		{
			headers,
			method
		};
	}

	private static void RemoveHeader(Metadata metadata, string headerName)
	{
		var toRemove = metadata.Where(entry =>
			string.Equals(entry.Key, headerName, StringComparison.OrdinalIgnoreCase))
			.ToList();

		foreach (var entry in toRemove)
		{
			metadata.Remove(entry);
		}
	}
}
