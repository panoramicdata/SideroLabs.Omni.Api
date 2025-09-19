using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for log management operations
/// </summary>
public interface ILogManagement
{
	/// <summary>
	/// Gets available log streams for a cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetLogStreamsResponse> GetLogStreamsAsync(string clusterId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets logs from a specific source
	/// </summary>
	/// <param name="source">Log source specification</param>
	/// <param name="spec">Log retrieval specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetLogsResponse> GetLogsAsync(LogSource source, LogStreamSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Starts a log stream for real-time log monitoring
	/// </summary>
	/// <param name="source">Log source specification</param>
	/// <param name="spec">Log stream specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<StartLogStreamResponse> StartLogStreamAsync(LogSource source, LogStreamSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Stops a log stream
	/// </summary>
	/// <param name="streamId">ID of the stream to stop</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<StopLogStreamResponse> StopLogStreamAsync(string streamId, CancellationToken cancellationToken);
}
