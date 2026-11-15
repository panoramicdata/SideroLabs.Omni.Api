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

	protected ManagementServiceBase(ManagementConnection connection)
		: base(connection.Options, connection.Channel, connection.Authenticator)
	{
		GrpcClient = new Management.ManagementService.ManagementServiceClient(connection.Channel);
		CallHelper = new GrpcCallHelper(connection.Options.Logger, CreateCallOptions);
	}
}
