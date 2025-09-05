using Microsoft.Extensions.DependencyInjection;

namespace SideroLabs.Omni.Api.Extensions;

/// <summary>
/// Service collection extensions for configuring the Omni Client
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds the Omni Client to the service collection
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configure">Configuration action for the client options</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(this IServiceCollection services, Action<OmniClientOptions> configure)
	{
		var options = new OmniClientOptions();
		configure(options);

		services.AddSingleton(options);
		services.AddSingleton<OmniClient>();

		return services;
	}

	/// <summary>
	/// Adds the Omni Client to the service collection with options
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="options">Pre-configured client options</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddOmniClient(this IServiceCollection services, OmniClientOptions options)
	{
		services.AddSingleton(options);
		services.AddSingleton<OmniClient>();

		return services;
	}
}