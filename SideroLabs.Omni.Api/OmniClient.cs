using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Extensions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Services;

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

		// Create gRPC channel
		_channel = CreateGrpcChannel();

		// Initialize authenticator if credentials are provided
		_authenticator = CreateAuthenticator();

		_logger.LogDebug("Initialized Omni gRPC client for endpoint: {Endpoint}", _options.Endpoint);
	}

	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// This is the primary service interface provided by Omni
	/// </summary>
	public IManagementService Management => _managementService ??= new OmniManagementService(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the gRPC endpoint URL
	/// </summary>
	public string Endpoint => _options.Endpoint;

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
		if (string.IsNullOrWhiteSpace(_options.Endpoint))
		{
			throw new ArgumentException("Endpoint is required", nameof(_options.Endpoint));
		}

		if (!Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out _))
		{
			throw new ArgumentException("Endpoint must be a valid URI", nameof(_options.Endpoint));
		}

		if (_options.TimeoutSeconds <= 0)
		{
			throw new ArgumentException("TimeoutSeconds must be positive", nameof(_options.TimeoutSeconds));
		}

		_logger.LogDebug("OmniClient options validated successfully");
	}

	private GrpcChannel CreateGrpcChannel()
	{
		var channelOptions = new GrpcChannelOptions
		{
			MaxReceiveMessageSize = 64 * 1024 * 1024, // 64MB for large responses
			MaxSendMessageSize = 16 * 1024 * 1024,    // 16MB for large requests
		};

		// Configure HTTP handler for certificate validation
		if (!_options.ValidateCertificate)
		{
			var httpHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (_, _, _, _) => true
			};

			channelOptions.HttpHandler = httpHandler;
			_logger.LogWarning("Certificate validation is disabled");
		}

		var channel = GrpcChannel.ForAddress(_options.Endpoint, channelOptions);
		_logger.LogDebug("Created gRPC channel for {Endpoint}", _options.Endpoint);

		return channel;
	}

	private OmniAuthenticator? CreateAuthenticator()
	{
		// Try to create authenticator from provided credentials
		try
		{
			if (!string.IsNullOrEmpty(_options.Identity) && !string.IsNullOrEmpty(_options.PgpPrivateKey))
			{
				return new OmniAuthenticator(_options.Identity, _options.PgpPrivateKey, _logger);
			}

			if (!string.IsNullOrEmpty(_options.PgpKeyFilePath))
			{
				var keyFile = new FileInfo(_options.PgpKeyFilePath);
				return OmniAuthenticator.FromFileAsync(keyFile, _logger).GetAwaiter().GetResult();
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
