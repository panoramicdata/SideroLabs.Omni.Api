using System.Text.Json;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Factories;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Services;
using SideroLabs.Omni.Api.Validation;

namespace SideroLabs.Omni.Api;

/// <summary>
/// Client for interacting with the SideroLabs Omni gRPC API
/// This client implements both ManagementService and COSI State service for full Omni functionality
/// </summary>
/// <remarks>
/// ✅ ManagementService - Administrative and operational tasks (configs, service accounts, schematics, etc.)
/// ✅ COSI State Service - Resource operations (clusters, machines, users, etc.) via /cosi.resource.State/*
/// ❌ ResourceService - NOT available on Omni SaaS (returns HTTP 405) - use State service instead
/// See BREAKTHROUGH_COSI_STATE_SERVICE.md for detailed analysis.
/// </remarks>
public class OmniClient : IOmniClient
{
	private readonly OmniClientOptions _options;
	private readonly ILogger _logger;
	private readonly GrpcChannel _channel;
	private readonly OmniAuthenticator? _authenticator;

	// Lazy-loaded services
	private IManagementService? _managementService;
	private IOmniResourceClient? _resourceClient;

	// Resource-specific operations
	private IClusterOperations? _clusterOperations;
	private IMachineOperations? _machineOperations;
	private IClusterMachineOperations? _clusterMachineOperations;
	private IMachineSetOperations? _machineSetOperations;
	private IMachineSetNodeOperations? _machineSetNodeOperations;
	private IMachineClassOperations? _machineClassOperations;
	private IConfigPatchOperations? _configPatchOperations;
	private IExtensionsConfigurationOperations? _extensionsConfigurationOperations;
	private ITalosConfigOperations? _talosConfigOperations;
	private ILoadBalancerOperations? _loadBalancerOperations;
	private IControlPlaneOperations? _controlPlaneOperations;
	private IKubernetesNodeOperations? _kubernetesNodeOperations;
	private IIdentityOperations? _identityOperations;
	private IUserManagement? _userManagement;
	private ITemplateOperations? _templateOperations;

	// Management services
	private IKubeConfigService? _kubeConfigService;
	private ITalosConfigService? _talosConfigService;
	private IOmniConfigService? _omniConfigService;
	private IServiceAccountService? _serviceAccountService;
	private IValidationService? _validationService;
	private IKubernetesService? _kubernetesService;
	private ISchematicService? _schematicService;
	private IMachineService? _machineService;
	private ISupportService? _supportService;

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

		// Initialize resource type registry for COSI State service
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

	// === Low-Level Resource Access ===

	/// <summary>
	/// Gets the Resource Client for COSI resource operations
	/// Now uses the COSI v1alpha1 State service which works on Omni SaaS!
	/// </summary>
	public IOmniResourceClient Resources => _resourceClient ??= new CosiStateClientService(_channel, _logger, _options.IsReadOnly, _options, _authenticator);

	// === Resource-Specific Operations ===

	public IClusterOperations Clusters => _clusterOperations ??= new ClusterOperations(Resources, _options);
	public IMachineOperations Machines => _machineOperations ??= new MachineOperations(Resources, _options);
	public IClusterMachineOperations ClusterMachines => _clusterMachineOperations ??= new ClusterMachineOperations(Resources, _options);
	public IMachineSetOperations MachineSets => _machineSetOperations ??= new MachineSetOperations(Resources, _options);
	public IMachineSetNodeOperations MachineSetNodes => _machineSetNodeOperations ??= new MachineSetNodeOperations(Resources, _options);
	public IMachineClassOperations MachineClasses => _machineClassOperations ??= new MachineClassOperations(Resources, _options);
	public IConfigPatchOperations ConfigPatches => _configPatchOperations ??= new ConfigPatchOperations(Resources, _options);
	public IExtensionsConfigurationOperations ExtensionsConfigurations => _extensionsConfigurationOperations ??= new ExtensionsConfigurationOperations(Resources, _options);
	public ITalosConfigOperations TalosConfigs => _talosConfigOperations ??= new TalosConfigOperations(Resources, _options);
	public ILoadBalancerOperations LoadBalancers => _loadBalancerOperations ??= new LoadBalancerOperations(Resources, _options);
	public IControlPlaneOperations ControlPlanes => _controlPlaneOperations ??= new ControlPlaneOperations(Resources, _options);
	public IKubernetesNodeOperations KubernetesNodes => _kubernetesNodeOperations ??= new KubernetesNodeOperations(Resources, _options);
	public IIdentityOperations Identities => _identityOperations ??= new IdentityOperations(Resources, _options);
	public IUserManagement Users => _userManagement ??= new UserManagement(Resources, _logger);
	public ITemplateOperations Templates => _templateOperations ??= new TemplateOperations(Resources, _logger);

	// === Management Services ===

	public IKubeConfigService KubeConfig => _kubeConfigService ??= new KubeConfigService(_options, _channel, _authenticator);
	public ITalosConfigService TalosConfig => _talosConfigService ??= new TalosConfigService(_options, _channel, _authenticator);
	public IOmniConfigService OmniConfig => _omniConfigService ??= new OmniConfigService(_options, _channel, _authenticator);
	public IServiceAccountService ServiceAccounts => _serviceAccountService ??= new ServiceAccountService(_options, _channel, _authenticator);
	public IValidationService Validation => _validationService ??= new ValidationService(_options, _channel, _authenticator);
	public IKubernetesService Kubernetes => _kubernetesService ??= new KubernetesService(_options, _channel, _authenticator);
	public ISchematicService Schematics => _schematicService ??= new SchematicService(_options, _channel, _authenticator);
	public IMachineService MachineManagement => _machineService ??= new MachineService(_options, _channel, _authenticator);
	public ISupportService Support => _supportService ??= new SupportService(_options, _channel, _authenticator);

	// === Legacy (Deprecated) ===

	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// </summary>
	/// <remarks>
	/// ⚠️ DEPRECATED: Use specific services like KubeConfig, ServiceAccounts, etc. instead.
	/// This property is maintained for backward compatibility but will be removed in a future version.
	/// </remarks>
	[Obsolete("Use specific services like KubeConfig, ServiceAccounts, Validation, etc. instead of the monolithic Management service.")]
	public IManagementService Management => _managementService ??= new OmniManagementService(_options, _channel, _authenticator);

	// === Client Properties ===

	public Uri BaseUrl => _options.BaseUrl;
	public bool UseTls => _options.UseTls;
	public bool IsReadOnly => _options.IsReadOnly;
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


