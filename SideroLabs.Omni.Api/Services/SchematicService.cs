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
	public async Task<SchematicResult> CreateAsync(
		string[]? extensions = null,
		string[]? extraKernelArgs = null,
		Dictionary<uint, string>? metaValues = null,
		string? talosVersion = null,
		string? mediaId = null,
		bool secureBoot = false,
		SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode = SiderolinkGrpcTunnelMode.Auto,
		string? joinToken = null,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("create", "schematic");

		var request = new CreateSchematicRequest
		{
			TalosVersion = talosVersion ?? "",
			MediaId = mediaId ?? "",
			SecureBoot = secureBoot,
			SiderolinkGrpcTunnelMode = (CreateSchematicRequest.Types.SiderolinkGRPCTunnelMode)(int)siderolinkGrpcTunnelMode,
			JoinToken = joinToken ?? ""
		};

		if (extensions != null)
		{
			request.Extensions.AddRange(extensions);
		}

		if (extraKernelArgs != null)
		{
			request.ExtraKernelArgs.AddRange(extraKernelArgs);
		}

		if (metaValues != null)
		{
			foreach (var kvp in metaValues)
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
