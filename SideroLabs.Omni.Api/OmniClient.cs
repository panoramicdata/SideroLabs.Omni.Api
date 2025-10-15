using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Factories;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Services;
using SideroLabs.Omni.Api.Validation;

namespace SideroLabs.Omni.Api;

/// <summary>
/// Client for interacting with the SideroLabs Omni gRPC API
/// This is a gRPC-only client that implements the actual Omni services
/// </summary>
public class OmniClient : IOmniClient
{
	private readonly OmniClientOptions _options;
	private readonly ILogger _logger;
	private readonly GrpcChannel _channel;
	private readonly OmniAuthenticator? _authenticator;

	// Lazy-loaded services
	private IManagementService? _managementService;

	/// <summary>
	/// Initializes a new instance of the OmniClient class
	/// </summary>
	/// <param name="options">Configuration options for the client</param>
	public OmniClient(OmniClientOptions options)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = _options.Logger;

		ValidateOptions();

		// Create gRPC channel using factory
		var channelFactory = new GrpcChannelFactory(_logger);
		_channel = channelFactory.CreateChannel(_options);

		// Initialize authenticator using factory
		var authenticatorFactory = new AuthenticatorFactory(_logger);
		_authenticator = authenticatorFactory.CreateAuthenticatorAsync(_options).GetAwaiter().GetResult();

		_logger.LogDebug("Initialized Omni gRPC client for endpoint: {BaseUrl}", _options.BaseUrl);
	}

	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// This is the primary service interface provided by Omni
	/// </summary>
	public IManagementService Management => _managementService ??= new OmniManagementService(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the gRPC endpoint URL
	/// </summary>
	public Uri BaseUrl => _options.BaseUrl;

	/// <summary>
	/// Gets whether TLS is enabled
	/// </summary>
	public bool UseTls => _options.UseTls;

	/// <summary>
	/// Gets whether the client is in read-only mode
	/// </summary>
	public bool IsReadOnly => _options.IsReadOnly;

	/// <summary>
	/// Gets the authentication identity if available
	/// </summary>
	public string? Identity => _authenticator?.Identity;

	private void ValidateOptions()
	{
		var validator = new OmniClientOptionsValidator();
		var validationResult = validator.Validate(_options);
		validationResult.ThrowIfInvalid();

		_logger.LogDebug("OmniClient options validated successfully");
	}

	/// <summary>
	/// Disposes the OmniClient and releases all resources
	/// </summary>
	public void Dispose()
	{
		_logger.LogDebug("Disposing OmniClient");

		try
		{
			(_managementService as IDisposable)?.Dispose();
			_channel?.Dispose();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error disposing OmniClient resources");
		}

		GC.SuppressFinalize(this);
	}
}
