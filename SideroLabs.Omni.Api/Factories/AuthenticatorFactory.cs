using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Factories;

/// <summary>
/// Factory for creating OmniAuthenticator instances
/// </summary>
/// <param name="logger">Logger instance</param>
internal class AuthenticatorFactory(ILogger logger) : IAuthenticatorFactory
{
	private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <inheritdoc />
	public async Task<OmniAuthenticator?> CreateAuthenticatorAsync(OmniClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		try
		{
			// Method 1: Direct PGP key content
			if (!string.IsNullOrEmpty(options.Identity) && !string.IsNullOrEmpty(options.PgpPrivateKey))
			{
				return new OmniAuthenticator(options.Identity, options.PgpPrivateKey, _logger);
			}

			// Method 2: PGP key file path
			if (!string.IsNullOrEmpty(options.PgpKeyFilePath))
			{
				var keyFile = new FileInfo(options.PgpKeyFilePath);
				return await OmniAuthenticator.FromFileAsync(keyFile, _logger);
			}

			// Method 3: Auth token containing base64-encoded JSON with identity and pgp_key
			if (!string.IsNullOrEmpty(options.AuthToken))
			{
				var (identity, pgpKey) = TryDecodeAuthToken(options.AuthToken);
				if (!string.IsNullOrEmpty(identity) && !string.IsNullOrEmpty(pgpKey))
				{
					return new OmniAuthenticator(identity, pgpKey, _logger);
				}

				_logger.LogWarning("AuthToken provided but failed to extract valid identity and PGP key - falling back to unauthenticated mode");
				return null;
			}

			_logger.LogWarning("No authentication credentials provided - operating in unauthenticated mode");
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to initialize authenticator - continuing without authentication");
			return null;
		}
	}

	/// <summary>
	/// Attempts to decode the AuthToken
	/// </summary>
	private (string? identity, string? pgpKey) TryDecodeAuthToken(string authToken)
	{
		try
		{
			var decodedBytes = Convert.FromBase64String(authToken);
			var decodedJson = Encoding.UTF8.GetString(decodedBytes);

			using var jsonDoc = JsonDocument.Parse(decodedJson);
			var root = jsonDoc.RootElement;

			var identity = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
			var pgpKey = root.TryGetProperty("pgp_key", out var pgpKeyElement) ? pgpKeyElement.GetString() : null;

			logger.LogInformation("Successfully decoded AuthToken for identity: {Identity}", identity);
			return (identity, pgpKey);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Failed to decode AuthToken, falling back to placeholder credentials");
			return (null, null);
		}
	}
}
