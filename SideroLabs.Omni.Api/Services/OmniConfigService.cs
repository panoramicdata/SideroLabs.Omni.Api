using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Management;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Constants;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Utilities;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Service for retrieving Omni configuration
/// </summary>
internal class OmniConfigService(ManagementConnection connection) : ManagementServiceBase(connection), IOmniConfigService
{
	public async Task<string> GetAsync(CancellationToken cancellationToken = default)
	{
		var request = new Empty();

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.OmniconfigAsync,
			GrpcMethods.Omniconfig,
			"omniconfig retrieval",
			cancellationToken);

		return ResponseDecoder.DecodeConfigResponse(response.Omniconfig);
	}
}
