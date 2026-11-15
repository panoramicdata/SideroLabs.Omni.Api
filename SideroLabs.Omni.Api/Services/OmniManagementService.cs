using Grpc.Net.Client;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Monolithic Omni Management Service facade that delegates to the focused management services
/// </summary>
internal class OmniManagementService : IManagementService, IDisposable
{
	private const int DefaultTailLines = 100;

	private readonly IKubeConfigService _kubeConfig;
	private readonly ITalosConfigService _talosConfig;
	private readonly IOmniConfigService _omniConfig;
	private readonly IServiceAccountService _serviceAccounts;
	private readonly IValidationService _validation;
	private readonly IKubernetesService _kubernetes;
	private readonly ISchematicService _schematics;
	private readonly IMachineService _machines;
	private readonly ISupportService _support;

	/// <summary>
	/// Initializes a new instance of the OmniManagementService class
	/// </summary>
	/// <param name="options">Client options</param>
	/// <param name="channel">gRPC channel</param>
	/// <param name="authenticator">Authentication provider</param>
	public OmniManagementService(
		OmniClientOptions options,
		GrpcChannel channel,
		OmniAuthenticator? authenticator)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(channel);

		var connection = new ManagementConnection(options, channel, authenticator);

		_kubeConfig = new KubeConfigService(connection);
		_talosConfig = new TalosConfigService(connection);
		_omniConfig = new OmniConfigService(connection);
		_serviceAccounts = new ServiceAccountService(connection);
		_validation = new ValidationService(connection);
		_kubernetes = new KubernetesService(connection);
		_schematics = new SchematicService(connection);
		_machines = new MachineService(connection);
		_support = new SupportService(connection);
	}

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(CancellationToken cancellationToken) =>
		GetKubeConfigAsync(false, null, null, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, null, null, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, TimeSpan? serviceAccountTtl, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, null, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(bool serviceAccount, TimeSpan? serviceAccountTtl, string? serviceAccountUser, CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, serviceAccountUser, null, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		CancellationToken cancellationToken) =>
		GetKubeConfigAsync(serviceAccount, serviceAccountTtl, serviceAccountUser, serviceAccountGroups, null, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetKubeConfigAsync(
		bool serviceAccount,
		TimeSpan? serviceAccountTtl,
		string? serviceAccountUser,
		string[]? serviceAccountGroups,
		string? grantType,
		bool breakGlass,
		CancellationToken cancellationToken) =>
		_kubeConfig.GetAsync(
			serviceAccount,
			serviceAccountTtl,
			serviceAccountUser,
			serviceAccountGroups,
			grantType,
			breakGlass,
			cancellationToken);

	/// <inheritdoc />
	public Task<string> GetTalosConfigAsync(CancellationToken cancellationToken) =>
		GetTalosConfigAsync(false, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetTalosConfigAsync(bool raw, CancellationToken cancellationToken) =>
		GetTalosConfigAsync(raw, false, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetTalosConfigAsync(bool raw, bool breakGlass, CancellationToken cancellationToken) =>
		_talosConfig.GetAsync(raw, breakGlass, cancellationToken);

	/// <inheritdoc />
	public Task<string> GetOmniConfigAsync(CancellationToken cancellationToken) =>
		_omniConfig.GetAsync(cancellationToken);

	/// <inheritdoc />
	public Task<string> CreateServiceAccountAsync(string armoredPgpPublicKey, CancellationToken cancellationToken) =>
		CreateServiceAccountAsync(armoredPgpPublicKey, false, null, cancellationToken);

	/// <inheritdoc />
	public Task<string> CreateServiceAccountAsync(string armoredPgpPublicKey, bool useUserRole, CancellationToken cancellationToken) =>
		CreateServiceAccountAsync(armoredPgpPublicKey, useUserRole, null, cancellationToken);

	/// <inheritdoc />
	public Task<string> CreateServiceAccountAsync(
		string armoredPgpPublicKey,
		bool useUserRole,
		string? role,
		CancellationToken cancellationToken) =>
		_serviceAccounts.CreateAsync(armoredPgpPublicKey, useUserRole, role, cancellationToken);

	/// <inheritdoc />
	public Task<IReadOnlyList<ServiceAccountInfo>> ListServiceAccountsAsync(CancellationToken cancellationToken) =>
		_serviceAccounts.ListAsync(cancellationToken);

	/// <inheritdoc />
	public Task<string> RenewServiceAccountAsync(
		string name,
		string armoredPgpPublicKey,
		CancellationToken cancellationToken) =>
		_serviceAccounts.RenewAsync(name, armoredPgpPublicKey, cancellationToken);

	/// <inheritdoc />
	public Task DestroyServiceAccountAsync(string name, CancellationToken cancellationToken) =>
		_serviceAccounts.DestroyAsync(name, cancellationToken);

	/// <inheritdoc />
	public Task ValidateConfigAsync(string config, CancellationToken cancellationToken) =>
		_validation.ValidateConfigAsync(config, cancellationToken);

	/// <inheritdoc />
	public Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		CancellationToken cancellationToken) =>
		_validation.ValidateJsonSchemaAsync(data, schema, cancellationToken);

	/// <inheritdoc />
	public Task<KubernetesUpgradePreCheckResult> KubernetesUpgradePreChecksAsync(
		string newVersion,
		CancellationToken cancellationToken) =>
		_kubernetes.UpgradePreChecksAsync(newVersion, cancellationToken);

	/// <inheritdoc />
	public Task<SchematicResult> CreateSchematicAsync(CancellationToken cancellationToken) =>
		CreateSchematicAsync(null, null, null, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<SchematicResult> CreateSchematicAsync(string[]? extensions, CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, null, null, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<SchematicResult> CreateSchematicAsync(string[]? extensions, string[]? extraKernelArgs, CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, extraKernelArgs, null, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<SchematicResult> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		CancellationToken cancellationToken) =>
		CreateSchematicAsync(extensions, extraKernelArgs, metaValues, null, null, false, SiderolinkGrpcTunnelMode.Auto, null, cancellationToken);

	/// <inheritdoc />
	public Task<SchematicResult> CreateSchematicAsync(
		string[]? extensions,
		string[]? extraKernelArgs,
		Dictionary<uint, string>? metaValues,
		string? talosVersion,
		string? mediaId,
		bool secureBoot,
		SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode,
		string? joinToken,
		CancellationToken cancellationToken) =>
		_schematics.CreateAsync(
			extensions,
			extraKernelArgs,
			metaValues,
			talosVersion,
			mediaId,
			secureBoot,
			siderolinkGrpcTunnelMode,
			joinToken,
			cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
		string cluster,
		CancellationToken cancellationToken) =>
		_support.GetSupportBundleAsync(cluster, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<byte[]> ReadAuditLogAsync(
		string startDate,
		string endDate,
		CancellationToken cancellationToken) =>
		_support.ReadAuditLogAsync(startDate, endDate, cancellationToken);

	/// <inheritdoc />
	public Task MaintenanceUpgradeAsync(
		string machineId,
		string version,
		CancellationToken cancellationToken) =>
		_machines.MaintenanceUpgradeAsync(machineId, version, cancellationToken);

	/// <inheritdoc />
	public Task<MachineJoinConfig> GetMachineJoinConfigAsync(
		bool useGrpcTunnel,
		string joinToken,
		CancellationToken cancellationToken) =>
		_machines.GetJoinConfigAsync(useGrpcTunnel, joinToken, cancellationToken);

	/// <inheritdoc />
	public Task<string> CreateJoinTokenAsync(
		string name,
		DateTimeOffset expirationTime,
		CancellationToken cancellationToken) =>
		_machines.CreateJoinTokenAsync(name, expirationTime, cancellationToken);

	/// <inheritdoc />
	public Task TearDownLockedClusterAsync(
		string clusterId,
		CancellationToken cancellationToken) =>
		_support.TearDownLockedClusterAsync(clusterId, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<byte[]> StreamMachineLogsAsync(string machineId, CancellationToken cancellationToken) =>
		StreamMachineLogsAsync(machineId, false, DefaultTailLines, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<byte[]> StreamMachineLogsAsync(string machineId, bool follow, CancellationToken cancellationToken) =>
		StreamMachineLogsAsync(machineId, follow, DefaultTailLines, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<byte[]> StreamMachineLogsAsync(
		string machineId,
		bool follow,
		int tailLines,
		CancellationToken cancellationToken) =>
		_machines.StreamLogsAsync(machineId, follow, tailLines, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(CancellationToken cancellationToken) =>
		StreamKubernetesSyncManifestsAsync(false, cancellationToken);

	/// <inheritdoc />
	public IAsyncEnumerable<KubernetesSyncResult> StreamKubernetesSyncManifestsAsync(
		bool dryRun,
		CancellationToken cancellationToken) =>
		_kubernetes.StreamSyncManifestsAsync(dryRun, cancellationToken);

	/// <inheritdoc />
	public void Dispose() =>
		GC.SuppressFinalize(this);
}
