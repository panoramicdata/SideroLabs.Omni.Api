using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Constants;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Base class for management services that wraps common gRPC functionality
/// </summary>
internal abstract class ManagementServiceBase : OmniServiceBase
{
	protected readonly Management.ManagementService.ManagementServiceClient GrpcClient;
	protected readonly GrpcCallHelper CallHelper;

	protected ManagementServiceBase(
		OmniClientOptions options,
		GrpcChannel channel,
		OmniAuthenticator? authenticator)
		: base(options, channel, authenticator)
	{
		GrpcClient = new Management.ManagementService.ManagementServiceClient(channel);
		CallHelper = new GrpcCallHelper(options.Logger, CreateCallOptions);
	}
}
