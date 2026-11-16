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
/// Service for managing service accounts
/// </summary>
internal class ServiceAccountService(ManagementConnection connection) : ManagementServiceBase(connection), IServiceAccountService
{
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new service account")]
	public async Task<string> CreateAsync(
		string armoredPgpPublicKey,
		bool useUserRole = false,
		string? role = null,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("create", "service account");

		var request = new CreateServiceAccountRequest
		{
			ArmoredPgpPublicKey = armoredPgpPublicKey,
			UseUserRole = useUserRole,
			Role = role ?? "",
			Name = $"service-account-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.CreateServiceAccountAsync,
			GrpcMethods.CreateServiceAccount,
			"service account creation",
			cancellationToken);

		return response.PublicKeyId;
	}

	public async Task<IReadOnlyList<ServiceAccountInfo>> ListAsync(CancellationToken cancellationToken = default)
	{
		var request = new Empty();

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.ListServiceAccountsAsync,
			GrpcMethods.ListServiceAccounts,
			"service accounts listing",
			cancellationToken);

		var serviceAccounts = response.ServiceAccounts.Select(sa => new ServiceAccountInfo
		{
			Name = sa.Name,
			Role = sa.Role,
			PgpPublicKeys = [.. sa.PgpPublicKeys.Select(key => new PgpPublicKeyInfo
			{
				Id = key.Id,
				Armored = key.Armored,
				Expiration = key.Expiration.ToDateTimeOffset()
			})]
		}).ToList();

		return serviceAccounts;
	}

	[IsWriteAction(WriteActionType.Update, Description = "Renews a service account with new credentials")]
	public async Task<string> RenewAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("update", "service account");

		var request = new RenewServiceAccountRequest
		{
			Name = name,
			ArmoredPgpPublicKey = armoredPgpPublicKey
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.RenewServiceAccountAsync,
			GrpcMethods.RenewServiceAccount,
			"service account renewal",
			cancellationToken);

		return response.PublicKeyId;
	}

	[IsWriteAction(WriteActionType.Delete, Description = "Destroys a service account")]
	public async Task DestroyAsync(string name, CancellationToken cancellationToken = default)
	{
		EnsureWriteOperationAllowed("delete", "service account");

		var request = new DestroyServiceAccountRequest
		{
			Name = name
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.DestroyServiceAccountAsync,
			GrpcMethods.DestroyServiceAccount,
			"service account destruction",
			cancellationToken);
	}
}
