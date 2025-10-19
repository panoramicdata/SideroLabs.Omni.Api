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

		Console.WriteLine("═══════════════════════════════════════════════════════════");
		Console.WriteLine("🔧 Creating gRPC Channel");
		Console.WriteLine($"  BaseUrl: {options.BaseUrl}");
		Console.WriteLine($"  Scheme: {options.BaseUrl.Scheme}");
		Console.WriteLine($"  Host: {options.BaseUrl.Host}");
		Console.WriteLine($"  Port: {options.BaseUrl.Port}");
		Console.WriteLine("═══════════════════════════════════════════════════════════");

		_logger.LogInformation("═══════════════════════════════════════════════════════════");
		_logger.LogInformation("🔧 Creating gRPC Channel");
		_logger.LogInformation("  BaseUrl: {BaseUrl}", options.BaseUrl);
		_logger.LogInformation("  Scheme: {Scheme}", options.BaseUrl.Scheme);
		_logger.LogInformation("  Host: {Host}", options.BaseUrl.Host);
		_logger.LogInformation("  Port: {Port}", options.BaseUrl.Port);
		_logger.LogInformation("  AbsoluteUri: {AbsoluteUri}", options.BaseUrl.AbsoluteUri);

		// Create a logging handler to capture all HTTP traffic
		var loggingHandler = new LoggingHttpMessageHandler(_logger);

		// Create HttpClient with HTTP/2 explicitly configured
		var httpHandler = new SocketsHttpHandler
		{
			// Explicitly set HTTP/2
			EnableMultipleHttp2Connections = true,
			KeepAlivePingDelay = TimeSpan.FromSeconds(60),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
			PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
		};

		// Configure certificate validation
		if (!options.ValidateCertificate)
		{
			httpHandler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
			_logger.LogWarning("Certificate validation is disabled");
		}

		// Chain the logging handler with the actual handler
		loggingHandler.InnerHandler = httpHandler;

		var httpClient = new HttpClient(loggingHandler)
		{
			Timeout = Timeout.InfiniteTimeSpan, // gRPC manages timeouts
			DefaultRequestVersion = new Version(2, 0), // Force HTTP/2
			DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact // Must be HTTP/2
		};

		var channelOptions = new GrpcChannelOptions
		{
			HttpClient = httpClient,
			MaxReceiveMessageSize = 64 * 1024 * 1024, // 64MB for large responses
			MaxSendMessageSize = 16 * 1024 * 1024,    // 16MB for large requests
		};

		var channel = GrpcChannel.ForAddress(options.BaseUrl, channelOptions);
		
		_logger.LogInformation("✅ gRPC channel created successfully");
		_logger.LogInformation("  Channel Target: {Target}", channel.Target);
		_logger.LogInformation("═══════════════════════════════════════════════════════════");

		return channel;
	}
}

/// <summary>
/// HTTP message handler that logs all HTTP requests and responses
/// </summary>
internal class LoggingHttpMessageHandler : DelegatingHandler
{
	private readonly ILogger _logger;

	public LoggingHttpMessageHandler(ILogger logger)
	{
		_logger = logger;
		_logger.LogWarning("🔥🔥🔥 LoggingHttpMessageHandler CONSTRUCTED 🔥🔥🔥");
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("═══════════════════════════════════════════════════════════");
		_logger.LogInformation("🌐 HTTP Request:");
		_logger.LogInformation("  Method: {Method}", request.Method);
		_logger.LogInformation("  URI: {Uri}", request.RequestUri);
		_logger.LogInformation("  Version: HTTP/{Version}", request.Version);
		
		_logger.LogInformation("  Headers:");
		foreach (var header in request.Headers)
		{
			foreach (var value in header.Value)
			{
				if (header.Key.Contains("signature", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogInformation("    {Key}: [REDACTED]", header.Key);
				}
				else
				{
					_logger.LogInformation("    {Key}: {Value}", header.Key, value);
				}
			}
		}

		if (request.Content != null)
		{
			_logger.LogInformation("  Content Headers:");
			foreach (var header in request.Content.Headers)
			{
				foreach (var value in header.Value)
				{
					_logger.LogInformation("    {Key}: {Value}", header.Key, value);
				}
			}
		}

		try
		{
			var response = await base.SendAsync(request, cancellationToken);

			_logger.LogInformation("───────────────────────────────────────────────────────────");
			_logger.LogInformation("📥 HTTP Response:");
			_logger.LogInformation("  Status: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
			_logger.LogInformation("  Version: HTTP/{Version}", response.Version);
			
			_logger.LogInformation("  Headers:");
			foreach (var header in response.Headers)
			{
				foreach (var value in header.Value)
				{
					_logger.LogInformation("    {Key}: {Value}", header.Key, value);
				}
			}

			if (response.Content != null)
			{
				_logger.LogInformation("  Content Headers:");
				foreach (var header in response.Content.Headers)
				{
					foreach (var value in header.Value)
					{
						_logger.LogInformation("    {Key}: {Value}", header.Key, value);
					}
				}

				// Try to read the response body
				try
				{
					var content = await response.Content.ReadAsStringAsync(cancellationToken);
					if (!string.IsNullOrEmpty(content))
					{
						var preview = content.Length > 500 ? content[..500] + "..." : content;
						_logger.LogInformation("  Content Body: {Body}", preview);
					}
				}
				catch
				{
					// If we can't read the body, that's okay
				}
			}

			_logger.LogInformation("═══════════════════════════════════════════════════════════");

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ HTTP Request Failed: {Message}", ex.Message);
			_logger.LogInformation("═══════════════════════════════════════════════════════════");
			throw;
		}
	}
}



