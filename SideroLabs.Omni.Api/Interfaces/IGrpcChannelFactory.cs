using Grpc.Net.Client;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for creating gRPC channels
/// </summary>
internal interface IGrpcChannelFactory
{
	/// <summary>
	/// Creates a gRPC channel with the specified options
	/// </summary>
	/// <param name="options">The client options</param>
	/// <returns>A configured gRPC channel</returns>
	GrpcChannel CreateChannel(OmniClientOptions options);
}
