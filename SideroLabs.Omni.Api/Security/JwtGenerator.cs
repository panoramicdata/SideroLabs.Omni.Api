using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace SideroLabs.Omni.Api.Security;

internal class JwtGenerator(FileInfo pgpPrivateKeyFileInfo, ILogger logger)
{
	public async Task<string> GenerateAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Generating JWT and signing with PGP private key...");

		try
		{
			// Load the PGP private key from the file asynchronously
			string fileContents = await File.ReadAllTextAsync(pgpPrivateKeyFileInfo.FullName, cancellationToken);

			// Extract the user name and PGP key from the JSON content
			// First, base64 decode the content
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

			string userName;
			string pgpPrivateKeyContent;

			using (var jsonDoc = JsonDocument.Parse(decodedString))
			{
				var root = jsonDoc.RootElement;

				if (!root.TryGetProperty("name", out var nameElement))
				{
					throw new InvalidOperationException("Missing 'name' property in JSON content");
				}

				if (!root.TryGetProperty("pgp_key", out var pgpKeyElement))
				{
					throw new InvalidOperationException("Missing 'pgp_key' property in JSON content");
				}

				userName = nameElement.GetString() ?? "default-user";
				pgpPrivateKeyContent = pgpKeyElement.GetString() ?? throw new InvalidOperationException("PGP key value is null");
			}

			logger.LogDebug("User Name: {UserName}", userName);

			// --- JWT Creation Logic ---
			// Create minimal JWT for Omni API (may not need all standard claims)
			var issuedAt = DateTimeOffset.UtcNow;
			var expires = issuedAt.AddHours(1); // 1 hour expiration

			var claims = new Dictionary<string, object>
			{
				["sub"] = userName,                                   // Subject (user identity) - primary claim
				["iat"] = issuedAt.ToUnixTimeSeconds(),              // Issued at
				["exp"] = expires.ToUnixTimeSeconds(),               // Expiration
			};

			// --- PGP Signing Logic ---
			// Parse the PGP private key first to determine the algorithm
			PgpSecretKey? secretKey = null;
			try
			{
				using var pgpKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(pgpPrivateKeyContent));
				var armoredInputStream = PgpUtilities.GetDecoderStream(pgpKeyStream);
				var pgpKeyRing = new PgpSecretKeyRing(armoredInputStream);

				// Get the first secret key suitable for signing
				foreach (PgpSecretKey key in pgpKeyRing.GetSecretKeys())
				{
					if (key.IsSigningKey)
					{
						secretKey = key;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to parse PGP key as secret key ring");
				throw new InvalidOperationException("Invalid PGP private key format", ex);
			}

			if (secretKey == null)
			{
				throw new InvalidOperationException("No suitable signing key found in PGP key ring");
			}

			// Extract the private key (assuming no passphrase)
			var privateKey = secretKey.ExtractPrivateKey(null);
			if (privateKey == null)
			{
				throw new InvalidOperationException("Failed to extract private key from PGP secret key");
			}

			// Determine the algorithm based on the key type
			var keyParams = privateKey.Key;
			string algorithm;
			byte[] signatureBytes;

			logger.LogInformation("PGP Key Type: {KeyType}", keyParams.GetType().Name);

			// Create unsigned JWT parts first
			var header = new Dictionary<string, object>
			{
				["typ"] = "JWT"
			};

			byte[] unsignedBytes;

			switch (keyParams)
			{
				case RsaPrivateCrtKeyParameters rsaParams:
					algorithm = "RS256";
					header["alg"] = algorithm;

					var headerJson = JsonSerializer.Serialize(header, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
					var payloadJson = JsonSerializer.Serialize(claims, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

					var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
					var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
					var unsignedJwtString = $"{headerBase64}.{payloadBase64}";

					unsignedBytes = Encoding.UTF8.GetBytes(unsignedJwtString);

					var rsaSigner = new RsaDigestSigner(new Sha256Digest());
					rsaSigner.Init(true, rsaParams);
					rsaSigner.BlockUpdate(unsignedBytes, 0, unsignedBytes.Length);
					signatureBytes = rsaSigner.GenerateSignature();

					logger.LogInformation("Using RSA-SHA256 signature");
					break;

				case ECPrivateKeyParameters ecParams:
					algorithm = "ES256";
					header["alg"] = algorithm;

					headerJson = JsonSerializer.Serialize(header, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
					payloadJson = JsonSerializer.Serialize(claims, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

					headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
					payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
					unsignedJwtString = $"{headerBase64}.{payloadBase64}";

					unsignedBytes = Encoding.UTF8.GetBytes(unsignedJwtString);

					// For ECDSA, we need to hash the data ourselves and handle the signature format
					using (var sha256 = System.Security.Cryptography.SHA256.Create())
					{
						var hash = sha256.ComputeHash(unsignedBytes);
						var ecdsaSigner = new ECDsaSigner();
						ecdsaSigner.Init(true, ecParams);
						var signature = ecdsaSigner.GenerateSignature(hash);

						// Convert BigInteger array to byte array for JWT
						// ECDSA signature consists of two BigIntegers (r, s)
						var r = signature[0];
						var s = signature[1];

						// Convert to byte arrays and concatenate (IEEE P1363 format)
						var rBytes = r.ToByteArrayUnsigned();
						var sBytes = s.ToByteArrayUnsigned();

						// Ensure each component is the right length (32 bytes for P-256)
						var keySize = 32; // P-256 uses 32-byte components
						var rPadded = new byte[keySize];
						var sPadded = new byte[keySize];

						Array.Copy(rBytes, 0, rPadded, keySize - rBytes.Length, rBytes.Length);
						Array.Copy(sBytes, 0, sPadded, keySize - sBytes.Length, sBytes.Length);

						signatureBytes = new byte[keySize * 2];
						Array.Copy(rPadded, 0, signatureBytes, 0, keySize);
						Array.Copy(sPadded, 0, signatureBytes, keySize, keySize);
					}

					logger.LogInformation("Using ECDSA-SHA256 signature");
					break;

				case Ed25519PrivateKeyParameters ed25519Params:
					algorithm = "EdDSA";
					header["alg"] = algorithm;

					headerJson = JsonSerializer.Serialize(header, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
					payloadJson = JsonSerializer.Serialize(claims, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

					headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
					payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
					unsignedJwtString = $"{headerBase64}.{payloadBase64}";

					unsignedBytes = Encoding.UTF8.GetBytes(unsignedJwtString);

					var ed25519Signer = new Ed25519Signer();
					ed25519Signer.Init(true, ed25519Params);
					ed25519Signer.BlockUpdate(unsignedBytes, 0, unsignedBytes.Length);
					signatureBytes = ed25519Signer.GenerateSignature();

					logger.LogInformation("Using Ed25519 signature");
					break;

				default:
					logger.LogError("Unsupported key type: {KeyType}", keyParams.GetType().Name);
					throw new InvalidOperationException($"Unsupported PGP key type: {keyParams.GetType().Name}. Supported types: RSA, ECDSA, Ed25519");
			}

			// Get the final unsigned JWT string from the switch
			header["alg"] = algorithm;
			var finalHeaderJson = JsonSerializer.Serialize(header, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
			var finalPayloadJson = JsonSerializer.Serialize(claims, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

			var finalHeaderBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(finalHeaderJson));
			var finalPayloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(finalPayloadJson));
			var finalUnsignedJwtString = $"{finalHeaderBase64}.{finalPayloadBase64}";

			logger.LogDebug("Unsigned JWT: {UnsignedJwtString}", finalUnsignedJwtString);
			logger.LogDebug("JWT Header: {JwtHeader}", finalHeaderJson);
			logger.LogDebug("JWT Payload: {JwtPayload}", finalPayloadJson);

			// Encode the signature in Base64Url format
			string base64UrlSignature = Base64UrlEncode(signatureBytes);

			// Construct the final signed JWT
			string finalSignedJwt = $"{finalUnsignedJwtString}.{base64UrlSignature}";

			logger.LogInformation("JWT successfully signed with {Algorithm} signature", algorithm);
			logger.LogDebug("Final JWT length: {JwtLength}", finalSignedJwt.Length);

			return finalSignedJwt;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error generating or signing JWT");
			throw;
		}
	}

	/// <summary>
	/// Base64Url encode without padding (RFC 7515)
	/// </summary>
	private static string Base64UrlEncode(byte[] input)
	{
		return Convert.ToBase64String(input)
			.Replace('+', '-')
			.Replace('/', '_')
			.TrimEnd('=');
	}
}
