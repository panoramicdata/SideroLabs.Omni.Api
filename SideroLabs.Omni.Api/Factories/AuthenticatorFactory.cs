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

			_logger.LogWarning("No authentication credentials provided - operating in unauthenticated mode");
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to initialize authenticator - continuing without authentication");
			return null;
		}
	}
}
