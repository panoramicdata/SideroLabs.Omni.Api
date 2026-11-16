using Grpc.Net.Client;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// The shared connection details every management service is created from
/// </summary>
internal sealed record ManagementConnection(
	OmniClientOptions Options,
	GrpcChannel Channel,
	OmniAuthenticator? Authenticator);