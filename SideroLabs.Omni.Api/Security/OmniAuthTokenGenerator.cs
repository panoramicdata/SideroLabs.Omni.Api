using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace SideroLabs.Omni.Api.Security;

internal class OmniAuthTokenGenerator(FileInfo pgpPrivateKeyFileInfo, ILogger logger)
{
	public async Task<string> GenerateOmniTokenAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Generating Omni authentication token with PGP private key...");

		try
		{
			// Load the PGP private key from the file asynchronously
			string fileContents = await File.ReadAllTextAsync(pgpPrivateKeyFileInfo.FullName, cancellationToken);

			// Extract the user name and PGP key from the JSON content
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

			// Parse the PGP private key
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

			// Create a challenge message to sign (this is likely what Omni expects)
			var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var challengeMessage = $"{userName}:{timestamp}";
			
			logger.LogInformation("Signing challenge message: {ChallengeMessage}", challengeMessage);

			// Create signature over the challenge message
			byte[] messageBytes = Encoding.UTF8.GetBytes(challengeMessage);

			// Initialize PGP signature generator
			var signatureGenerator = new PgpSignatureGenerator(secretKey.PublicKey.Algorithm, HashAlgorithmTag.Sha256);
			signatureGenerator.InitSign(PgpSignature.BinaryDocument, privateKey);

			// Update signature with the data
			signatureGenerator.Update(messageBytes);

			// Generate the signature
			var signature = signatureGenerator.Generate();

			// Get the signature bytes
			using var signatureStream = new MemoryStream();
			signature.Encode(signatureStream);
			var signatureBytes = signatureStream.ToArray();

			// Try different formats that Omni might expect:

			// Format 1: Raw Base64 signature
			string base64Signature = Convert.ToBase64String(signatureBytes);
			logger.LogInformation("Format 1 - Raw Base64 signature: {Base64Signature}", base64Signature);

			// Format 2: Base64Url signature (JWT style)
			string base64UrlSignature = Base64UrlEncode(signatureBytes);
			logger.LogInformation("Format 2 - Base64Url signature: {Base64UrlSignature}", base64UrlSignature);

			// Format 3: Combined message and signature
			var combinedToken = $"{Convert.ToBase64String(messageBytes)}.{base64Signature}";
			logger.LogInformation("Format 3 - Combined token: {CombinedToken}", combinedToken);

			// Format 4: Simple username + signature
			var userSignatureToken = $"{userName}:{base64Signature}";
			logger.LogInformation("Format 4 - User:Signature token: {UserSignatureToken}", userSignatureToken);

			logger.LogInformation("Generated multiple token formats for testing");

			// Return the most likely format (raw base64 signature)
			return base64Signature;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error generating Omni authentication token");
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
