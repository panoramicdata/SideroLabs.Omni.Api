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

		services.AddSingleton(serviceProvider =>
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
		OmniClientOptions options) => services.AddOmniClient(target => CopyOptions(options, target));

	/// <summary>
	/// Copies options from source to target to avoid duplication
	/// </summary>
	/// <param name="source">Source options</param>
	/// <param name="target">Target options</param>
	private static void CopyOptions(OmniClientOptions source, OmniClientOptions target)
	{
		target.BaseUrl = source.BaseUrl;
		target.Identity = source.Identity;
		target.PgpPrivateKey = source.PgpPrivateKey;
		target.PgpKeyFilePath = source.PgpKeyFilePath;
		target.TimeoutSeconds = source.TimeoutSeconds;
		target.UseTls = source.UseTls;
		target.ValidateCertificate = source.ValidateCertificate;
		target.IsReadOnly = source.IsReadOnly;
		target.Logger = source.Logger;
	}
}
