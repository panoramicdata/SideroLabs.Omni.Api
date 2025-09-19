using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for service status operations
/// </summary>
public interface IServiceStatus
{
	/// <summary>
	/// Gets the basic status of the Omni service
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetStatusResponse> GetStatusAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets enhanced status of the Omni service with detailed health information
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetEnhancedStatusResponse> GetEnhancedStatusAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets health check status for all components
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetHealthCheckResponse> GetHealthCheckAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets health check status for a specific component
	/// </summary>
	/// <param name="component">Component name to check</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetHealthCheckResponse> GetHealthCheckAsync(string component, CancellationToken cancellationToken);
}
