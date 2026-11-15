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
/// Service for Kubernetes operations
/// </summary>
internal class KubernetesService(ManagementConnection connection) : ManagementServiceBase(connection), IKubernetesService
{
	public async Task<KubernetesUpgradePreCheckResult> UpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken = default)
	{
		var request = new KubernetesUpgradePreChecksRequest
		{
			NewVersion = newVersion
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.KubernetesUpgradePreChecksAsync,
			GrpcMethods.KubernetesUpgradePreChecks,
			"Kubernetes upgrade pre-checks",
			cancellationToken);

		return response.Ok
			? KubernetesUpgradePreCheckResult.Success(response.Reason)
			: KubernetesUpgradePreCheckResult.Failure(response.Reason);
	}

	[IsWriteAction(WriteActionType.Update, Description = "Synchronizes Kubernetes manifests")]
	public async IAsyncEnumerable<KubernetesSyncResult> StreamSyncManifestsAsync(
		bool dryRun = false,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (!dryRun)
		{
			EnsureWriteOperationAllowed("update", "kubernetes manifests");
		}

		var request = new KubernetesSyncManifestRequest
		{
			DryRun = dryRun
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.KubernetesSyncManifests,
			GrpcMethods.KubernetesSyncManifests,
			"Kubernetes manifest sync streaming");

		await foreach (var syncResult in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			yield return new KubernetesSyncResult
			{
				ResponseType = (SyncType)(int)syncResult.ResponseType,
				Path = syncResult.Path,
				Object = syncResult.Object.ToByteArray(),
				Diff = syncResult.Diff,
				Skipped = syncResult.Skipped
			};
		}
	}
}
