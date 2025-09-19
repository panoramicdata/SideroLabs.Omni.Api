using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;

namespace SideroLabs.Omni.Api.Factories;

/// <summary>
/// Factory for creating gRPC channels
/// </summary>
/// <param name="logger">Logger instance</param>
internal class GrpcChannelFactory(ILogger logger) : IGrpcChannelFactory
{
	private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <inheritdoc />
	public GrpcChannel CreateChannel(OmniClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		var channelOptions = new GrpcChannelOptions
		{
			MaxReceiveMessageSize = 64 * 1024 * 1024, // 64MB for large responses
			MaxSendMessageSize = 16 * 1024 * 1024,    // 16MB for large requests
		};

		// Configure HTTP handler for certificate validation
		if (!options.ValidateCertificate)
		{
			var httpHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (_, _, _, _) => true
			};

			channelOptions.HttpHandler = httpHandler;
			_logger.LogWarning("Certificate validation is disabled");
		}

		var channel = GrpcChannel.ForAddress(options.Endpoint, channelOptions);
		_logger.LogDebug("Created gRPC channel for {Endpoint}", options.Endpoint);

		return channel;
	}
}
