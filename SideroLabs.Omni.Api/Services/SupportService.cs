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
/// Service for support and audit operations
/// </summary>
internal class SupportService(ManagementConnection connection) : ManagementServiceBase(connection), ISupportService
{
	public async IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
		string cluster,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Starting support bundle generation for cluster: {Cluster}", cluster);

		var request = new GetSupportBundleRequest
		{
			Cluster = cluster
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.GetSupportBundle,
			GrpcMethods.GetSupportBundle,
			"support bundle generation");

		await foreach (var update in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			var progress = ToProgress(update);
			LogProgress(progress);

			yield return progress;
		}

		Logger.LogInformation("Support bundle generation completed for cluster: {Cluster}", cluster);
	}

	private static SupportBundleProgress ToProgress(GetSupportBundleResponse update)
	{
		var progress = update.Progress ?? new();
		return new SupportBundleProgress
		{
			Source = progress.Source,
			Error = progress.Error,
			State = progress.State,
			Total = progress.Total,
			Value = progress.Value,
			BundleData = update.BundleData.Length > 0 ? update.BundleData.ToByteArray() : null
		};
	}

	private void LogProgress(SupportBundleProgress progress)
	{
		if (progress.HasError)
		{
			Logger.LogWarning("Support bundle generation error from {Source}: {Error}", progress.Source, progress.Error);
		}
		else if (progress.HasBundleData)
		{
			Logger.LogInformation("Support bundle data received: {Size} bytes", progress.BundleData!.Length);
		}
		else if (!string.IsNullOrEmpty(progress.State))
		{
			Logger.LogDebug("Support bundle progress: {State} ({Value}/{Total})", progress.State, progress.Value, progress.Total);
		}
	}

	public async IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Logger.LogInformation("Reading audit logs from {StartDate} to {EndDate}", startDate, endDate);

		var request = new ReadAuditLogRequest
		{
			StartTime = startDate,
			EndTime = endDate
		};

		using var call = CallHelper.ExecuteStreamingCall(
			request,
			GrpcClient.ReadAuditLog,
			GrpcMethods.ReadAuditLog,
			"audit log reading");

		var totalBytesRead = 0L;
		var chunkCount = 0;

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
		{
			var logData = response.AuditLog.ToByteArray();
			totalBytesRead += logData.Length;
			chunkCount++;

			Logger.LogDebug("Received audit log chunk {ChunkNumber}: {Size} bytes", chunkCount, logData.Length);

			yield return logData;
		}

		Logger.LogInformation("Audit log reading completed: {TotalChunks} chunks, {TotalBytes} total bytes", chunkCount, totalBytesRead);
	}

	[IsWriteAction(WriteActionType.Delete, Description = "Tears down a locked cluster")]
	public async Task TearDownLockedClusterAsync(
		string clusterId,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("tear down", "locked cluster");

		Logger.LogWarning("Tearing down locked cluster: {ClusterId} - this is a destructive operation", clusterId);

		var request = new TearDownLockedClusterRequest
		{
			ClusterId = clusterId
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.TearDownLockedClusterAsync,
			GrpcMethods.TearDownLockedCluster,
			"locked cluster tear down",
			cancellationToken);

		Logger.LogInformation("Locked cluster {ClusterId} torn down successfully", clusterId);
	}
}
