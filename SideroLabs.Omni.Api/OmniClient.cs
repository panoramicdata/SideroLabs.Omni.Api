using System.Text.Json;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Factories;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Services;
using SideroLabs.Omni.Api.Validation;
using static Omni.Resources.ResourceService;

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
	private IOmniResourceClient? _resourceClient;
	private IClusterOperations? _clusterOperations;
	private ITemplateOperations? _templateOperations;
	private IUserManagement? _userManagement;

	// JSON handling
	internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	/// <summary>
	/// Initializes a new instance of the OmniClient class
	/// </summary>
	/// <param name="options">Configuration options for the client</param>
	public OmniClient(OmniClientOptions options)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = _options.Logger;

		// Initialize resource type registry
		ResourceTypes.Initialize();

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
	/// Gets the Resource Client for COSI resource operations
	/// Provides access to get, list, watch, create, update, and delete resources
	/// </summary>
	public IOmniResourceClient Resources => _resourceClient ??= new ResourceClientService(new ResourceServiceClient(_channel), _logger, _options.IsReadOnly, _options);

	/// <summary>
	/// Gets cluster operations (placeholder until implemented)
	/// </summary>
	public IClusterOperations Clusters => _clusterOperations ??= new ClusterOperations(Resources, _options);

	/// <summary>
	/// Gets template operations
	/// </summary>
	public ITemplateOperations Templates => _templateOperations ??= new TemplateOperations(Resources, _logger);


	/// <summary>
	/// Gets user management operations (placeholder until implemented)
	/// </summary>
	public IUserManagement Users => _userManagement ??= new UserManagement();

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
