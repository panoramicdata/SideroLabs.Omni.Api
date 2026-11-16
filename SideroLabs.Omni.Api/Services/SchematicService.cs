using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Management;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Constants;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Service for machine schematic operations
/// </summary>
internal class SchematicService(ManagementConnection connection) : ManagementServiceBase(connection), ISchematicService
{
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new schematic for machine provisioning")]
	public async Task<SchematicResult> CreateAsync(CreateSchematicOptions? options = null, CancellationToken cancellationToken = default)
	{
		options ??= new CreateSchematicOptions();
		EnsureWriteOperationAllowed("create", "schematic");

		var request = new CreateSchematicRequest
		{
			TalosVersion = options.TalosVersion ?? "",
			MediaId = options.MediaId ?? "",
			SecureBoot = options.SecureBoot,
			SiderolinkGrpcTunnelMode = (CreateSchematicRequest.Types.SiderolinkGRPCTunnelMode)(int)options.SiderolinkGrpcTunnelMode,
			JoinToken = options.JoinToken ?? ""
		};

		if (options.Extensions != null)
		{
			request.Extensions.AddRange(options.Extensions);
		}

		if (options.ExtraKernelArgs != null)
		{
			request.ExtraKernelArgs.AddRange(options.ExtraKernelArgs);
		}

		if (options.MetaValues != null)
		{
			foreach (var kvp in options.MetaValues)
			{
				request.MetaValues[kvp.Key] = kvp.Value;
			}
		}

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.CreateSchematicAsync,
			GrpcMethods.CreateSchematic,
			"schematic creation",
			cancellationToken);

		return new SchematicResult
		{
			SchematicId = response.SchematicId,
			PxeUrl = response.PxeUrl,
			GrpcTunnelEnabled = response.GrpcTunnelEnabled
		};
	}
}
