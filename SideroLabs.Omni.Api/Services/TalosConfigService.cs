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
/// Service for retrieving Talos configuration
/// </summary>
internal class TalosConfigService(ManagementConnection connection) : ManagementServiceBase(connection), ITalosConfigService
{
	public async Task<string> GetAsync(
		bool raw = false,
		bool breakGlass = false,
		CancellationToken cancellationToken = default)
	{
		if (breakGlass)
		{
			Logger.LogWarning("Using break-glass access for talosconfig");
		}

		var request = new TalosconfigRequest
		{
			Raw = raw,
			BreakGlass = breakGlass
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.TalosconfigAsync,
			GrpcMethods.Talosconfig,
			"talosconfig retrieval",
			cancellationToken);

		return ResponseDecoder.DecodeConfigResponse(response.Talosconfig);
	}
}
