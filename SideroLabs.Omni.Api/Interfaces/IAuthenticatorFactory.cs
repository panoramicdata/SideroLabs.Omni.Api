using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for creating authenticators
/// </summary>
internal interface IAuthenticatorFactory
{
	/// <summary>
	/// Creates an authenticator from the specified options
	/// </summary>
	/// <param name="options">The client options</param>
	/// <returns>An authenticator instance, or null if no credentials are provided</returns>
	Task<OmniAuthenticator?> CreateAuthenticatorAsync(OmniClientOptions options);
}
