using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SideroLabs.Omni.Api.Extensions;

/// <summary>
/// Service collection extensions for configuring the Omni Client
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds the Omni Client to the service collection using IOptions pattern
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(this IServiceCollection services)
	{
		services.AddSingleton(provider =>
		{
			var options = provider.GetRequiredService<IOptions<OmniClientOptions>>().Value;
			var logger = provider.GetService<Microsoft.Extensions.Logging.ILogger<OmniClient>>();
			return new OmniClient(options);
		});

		return services;
	}

	/// <summary>
	/// Adds the Omni Client to the service collection
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configure">Configuration action for the client options</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(this IServiceCollection services, Action<OmniClientOptions> configure)
	{
		services.Configure(configure);
		return services.AddOmniClient();
	}

	/// <summary>
	/// Adds the Omni Client to the service collection with options
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="options">Pre-configured client options</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(this IServiceCollection services, OmniClientOptions options)
	{
		services.Configure<OmniClientOptions>(opts =>
		{
			opts.Endpoint = options.Endpoint;
			opts.Identity = options.Identity;
			opts.PgpPrivateKey = options.PgpPrivateKey;
			opts.PgpKeyFilePath = options.PgpKeyFilePath;
			opts.TimeoutSeconds = options.TimeoutSeconds;
			opts.UseTls = options.UseTls;
			opts.ValidateCertificate = options.ValidateCertificate;
			opts.IsReadOnly = options.IsReadOnly;
			opts.Logger = options.Logger;
		});
		return services.AddOmniClient();
	}
}
