namespace SideroLabs.Omni.Api.Examples.Infrastructure;

/// <summary>
/// Factory for creating common example configurations
/// </summary>
public static class ExampleConfigurationFactory
{
	/// <summary>
	/// Creates standard configuration for examples
	/// </summary>
	/// <returns>Configured options with default settings</returns>
	public static OmniClientOptions CreateStandardOptions() => CreateStandardOptions("your-username", 30);

	/// <summary>
	/// Creates standard configuration for examples
	/// </summary>
	/// <param name="identity">User identity</param>
	/// <returns>Configured options with default timeout</returns>
	public static OmniClientOptions CreateStandardOptions(string identity) => CreateStandardOptions(identity, 30);

	/// <summary>
	/// Creates standard configuration for examples
	/// </summary>
	/// <param name="identity">User identity</param>
	/// <param name="timeoutSeconds">Timeout in seconds</param>
	/// <returns>Configured options</returns>
	public static OmniClientOptions CreateStandardOptions(string identity, int timeoutSeconds) => new()
	{
		Endpoint = "https://your-omni-instance.example.com",
		Identity = identity,
		PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
		TimeoutSeconds = timeoutSeconds,
		UseTls = true,
		ValidateCertificate = true
	};

	/// <summary>
	/// Creates read-only configuration for examples
	/// </summary>
	/// <returns>Configured read-only options with default identity</returns>
	public static OmniClientOptions CreateReadOnlyOptions() => CreateReadOnlyOptions("readonly-user");

	/// <summary>
	/// Creates read-only configuration for examples
	/// </summary>
	/// <param name="identity">User identity</param>
	/// <returns>Configured read-only options</returns>
	public static OmniClientOptions CreateReadOnlyOptions(string identity) => new()
	{
		Endpoint = "https://your-omni-instance.example.com",
		Identity = identity,
		PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
		TimeoutSeconds = 30,
		UseTls = true,
		ValidateCertificate = true,
		IsReadOnly = true
	};

	/// <summary>
	/// Creates configuration for streaming operations
	/// </summary>
	/// <returns>Configured options with longer timeout and default identity</returns>
	public static OmniClientOptions CreateStreamingOptions() => CreateStreamingOptions("streaming-user");

	/// <summary>
	/// Creates configuration for streaming operations
	/// </summary>
	/// <param name="identity">User identity</param>
	/// <returns>Configured options with longer timeout</returns>
	public static OmniClientOptions CreateStreamingOptions(string identity) => new()
	{
		Endpoint = "https://your-omni-instance.example.com",
		Identity = identity,
		PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
		TimeoutSeconds = 60,
		UseTls = true,
		ValidateCertificate = true
	};
}
