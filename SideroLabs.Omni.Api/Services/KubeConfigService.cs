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
/// Service for retrieving Kubernetes configuration
/// </summary>
internal class KubeConfigService(ManagementConnection connection) : ManagementServiceBase(connection), IKubeConfigService
{
	public Task<string> GetAsync(CancellationToken cancellationToken = default) =>
		GetAsync(false, cancellationToken: cancellationToken);

	public async Task<string> GetAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl = null,
		string? serviceAccountUser = null,
		string[]? serviceAccountGroups = null,
		string? grantType = null,
		bool breakGlass = false,
		CancellationToken cancellationToken = default)
	{
		if (serviceAccount)
		{
			EnsureWriteOperationAllowed("create", "service account");
		}

		if (breakGlass)
		{
			Logger.LogWarning("Using break-glass access for kubeconfig");
		}

		var request = new KubeconfigRequest
		{
			ServiceAccount = serviceAccount,
			ServiceAccountUser = serviceAccountUser ?? "",
			GrantType = grantType ?? "",
			BreakGlass = breakGlass
		};

		if (serviceAccountTtl.HasValue)
		{
			request.ServiceAccountTtl = Duration.FromTimeSpan(serviceAccountTtl.Value);
		}

		if (serviceAccountGroups != null)
		{
			request.ServiceAccountGroups.AddRange(serviceAccountGroups);
		}

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.KubeconfigAsync,
			GrpcMethods.Kubeconfig,
			"kubeconfig retrieval",
			cancellationToken);

		return ResponseDecoder.DecodeConfigResponse(response.Kubeconfig);
	}
}
