using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;

namespace SideroLabs.Omni.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Omni gRPC services
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds the OmniClient and its dependencies to the service collection
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configureOptions">Action to configure OmniClientOptions</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(
		this IServiceCollection services,
		Action<OmniClientOptions> configureOptions)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configureOptions);

		// Register OmniClientOptions
		services.Configure(configureOptions);

		// Register the OmniClient as both its interface and concrete type
		services.AddSingleton<IOmniClient>(serviceProvider =>
		{
			var logger = serviceProvider.GetService<ILogger<OmniClient>>() ??
						 serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OmniClient>() ??
						 Microsoft.Extensions.Logging.Abstractions.NullLogger<OmniClient>.Instance;

			var options = new OmniClientOptions();
			configureOptions(options);
			options.Logger = logger;

			return new OmniClient(options);
		});

		services.AddSingleton<OmniClient>(serviceProvider =>
			(OmniClient)serviceProvider.GetRequiredService<IOmniClient>());

		// Register individual services for direct injection
		services.AddTransient(serviceProvider =>
			serviceProvider.GetRequiredService<IOmniClient>().Management);

		return services;
	}

	/// <summary>
	/// Adds the OmniClient with options loaded from configuration
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="options">Pre-configured options</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(
		this IServiceCollection services,
		OmniClientOptions options) => services.AddOmniClient(_ =>
										   {
											   _.Endpoint = options.Endpoint;
											   _.Identity = options.Identity;
											   _.PgpPrivateKey = options.PgpPrivateKey;
											   _.PgpKeyFilePath = options.PgpKeyFilePath;
											   _.TimeoutSeconds = options.TimeoutSeconds;
											   _.UseTls = options.UseTls;
											   _.ValidateCertificate = options.ValidateCertificate;
											   _.IsReadOnly = options.IsReadOnly;
										   });
}

/// <summary>
/// Interface for the OmniClient to support dependency injection
/// </summary>
public interface IOmniClient : IDisposable
{
	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// </summary>
	IManagementService Management { get; }

	/// <summary>
	/// Gets the gRPC endpoint URL
	/// </summary>
	string Endpoint { get; }

	/// <summary>
	/// Gets whether TLS is enabled
	/// </summary>
	bool UseTls { get; }

	/// <summary>
	/// Gets whether the client is in read-only mode
	/// </summary>
	bool IsReadOnly { get; }

	/// <summary>
	/// Gets the authentication identity if available
	/// </summary>
	string? Identity { get; }
}
