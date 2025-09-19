using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Services;

namespace SideroLabs.Omni.Api;

/// <summary>
/// An Omni Client for interacting with the Sidero Labs Omni Management API
/// </summary>
/// <param name="omniClientOptions">Configuration options for the client</param>
/// <param name="logger">Logger instance</param>
public class OmniClient(OmniClientOptions omniClientOptions) : IDisposable
{
	private readonly OmniClientOptions _options = omniClientOptions ?? throw new ArgumentNullException(nameof(omniClientOptions));
	private readonly GrpcChannel _channel = CreateChannel(omniClientOptions);
	private readonly OmniAuthenticator? _authenticator = CreateAuthenticator(omniClientOptions);
	private bool _disposed;

	// Service instances - lazy initialization
	private IClusterManagement? _clusterManagement;
	private IMachineManagement? _machineManagement;
	private IWorkspaceManagement? _workspaceManagement;
	private IBackupOperations? _backupOperations;
	private IRestoreOperations? _restoreOperations;
	private ILogManagement? _logManagement;
	private INetworkManagement? _networkManagement;
	private IConfigurationTemplateManagement? _configurationTemplateManagement;
	private IKubernetesIntegration? _kubernetesIntegration;
	private IServiceStatus? _serviceStatus;

	/// <summary>
	/// Gets the configured endpoint
	/// </summary>
	public string Endpoint => _options.Endpoint;

	/// <summary>
	/// Gets whether TLS is enabled
	/// </summary>
	public bool UseTls => _options.UseTls;

	/// <summary>
	/// Gets the cluster management service
	/// </summary>
	public IClusterManagement Clusters => _clusterManagement ??= new ClusterManagement(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the machine management service
	/// </summary>
	public IMachineManagement Machines => _machineManagement ??= new MachineManagement(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the workspace management service
	/// </summary>
	public IWorkspaceManagement Workspaces => _workspaceManagement ??= new WorkspaceManagement(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the backup operations service
	/// </summary>
	public IBackupOperations Backups => _backupOperations ??= new BackupOperations(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the restore operations service
	/// </summary>
	public IRestoreOperations RestoreOperations => _restoreOperations ??= new RestoreOperations(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the log management service
	/// </summary>
	public ILogManagement Logs => _logManagement ??= new LogManagement(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the network management service
	/// </summary>
	public INetworkManagement Networks => _networkManagement ??= new NetworkManagement(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the configuration template management service
	/// </summary>
	public IConfigurationTemplateManagement ConfigurationTemplates => _configurationTemplateManagement ??= new ConfigurationTemplateManagement(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the Kubernetes integration service
	/// </summary>
	public IKubernetesIntegration Kubernetes => _kubernetesIntegration ??= new KubernetesIntegration(_options, _channel, _authenticator);

	/// <summary>
	/// Gets the service status service
	/// </summary>
	public IServiceStatus Status => _serviceStatus ??= new ServiceStatus(_options, _channel, _authenticator);

	/// <summary>
	/// Creates a gRPC channel with the specified options
	/// </summary>
	private static GrpcChannel CreateChannel(OmniClientOptions options)
	{
		var channelOptions = new GrpcChannelOptions
		{
			HttpHandler = CreateHttpHandler(options)
		};

		return GrpcChannel.ForAddress(options.Endpoint, channelOptions);
	}

	/// <summary>
	/// Creates an HTTP handler with the specified security options
	/// </summary>
	private static HttpClientHandler CreateHttpHandler(OmniClientOptions options)
	{
		var handler = new HttpClientHandler();

		if (!options.ValidateCertificate)
		{
			handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
		}

		return handler;
	}

	/// <summary>
	/// Creates an authenticator based on the options
	/// </summary>
	private static OmniAuthenticator? CreateAuthenticator(OmniClientOptions options)
	{
		try
		{
			// Prioritize direct PGP key content
			if (!string.IsNullOrEmpty(options.Identity) && !string.IsNullOrEmpty(options.PgpPrivateKey))
			{
				return new OmniAuthenticator(options.Identity, options.PgpPrivateKey, options.Logger);
			}

			// Fallback to file path
			if (!string.IsNullOrEmpty(options.PgpKeyFilePath))
			{
				var keyFile = new FileInfo(options.PgpKeyFilePath);
				// Note: This will be async in real usage, but for now we'll return null
				// and handle authentication setup later
				options.Logger.LogWarning("PGP key file path specified but async initialization not yet implemented: {FilePath}", options.PgpKeyFilePath);
				return null;
			}

			options.Logger.LogWarning("No PGP authentication configured. Some operations may fail.");
			return null;
		}
		catch (Exception ex)
		{
			options.Logger.LogWarning(ex, "Failed to initialize PGP authenticator. Operations will continue without authentication.");
			return null;
		}
	}

	#region IDisposable

	/// <summary>
	/// Disposes the client and releases resources
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_channel?.Dispose();
			_disposed = true;
		}

		GC.SuppressFinalize(this);
	}

	#endregion
}
