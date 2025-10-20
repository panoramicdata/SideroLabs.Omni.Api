using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Operations for MachineSet resources
/// </summary>
public interface IMachineSetOperations
{
	IAsyncEnumerable<MachineSet> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<MachineSet> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<MachineSet> CreateAsync(MachineSet machineSet, CancellationToken cancellationToken = default);
	Task<MachineSet> UpdateAsync(MachineSet machineSet, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<MachineSet>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<MachineSet> ApplyAsync(MachineSet machineSet, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for MachineSetNode resources
/// </summary>
public interface IMachineSetNodeOperations
{
	IAsyncEnumerable<MachineSetNode> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<MachineSetNode> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<MachineSetNode> CreateAsync(MachineSetNode node, CancellationToken cancellationToken = default);
	Task<MachineSetNode> UpdateAsync(MachineSetNode node, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<MachineSetNode>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<MachineSetNode> ApplyAsync(MachineSetNode node, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for ConfigPatch resources
/// </summary>
public interface IConfigPatchOperations
{
	IAsyncEnumerable<ConfigPatch> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<ConfigPatch> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<ConfigPatch> CreateAsync(ConfigPatch configPatch, CancellationToken cancellationToken = default);
	Task<ConfigPatch> UpdateAsync(ConfigPatch configPatch, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<ConfigPatch>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<ConfigPatch> ApplyAsync(ConfigPatch configPatch, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for ExtensionsConfiguration resources
/// </summary>
public interface IExtensionsConfigurationOperations
{
	IAsyncEnumerable<ExtensionsConfiguration> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<ExtensionsConfiguration> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<ExtensionsConfiguration> CreateAsync(ExtensionsConfiguration config, CancellationToken cancellationToken = default);
	Task<ExtensionsConfiguration> UpdateAsync(ExtensionsConfiguration config, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<ExtensionsConfiguration>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<ExtensionsConfiguration> ApplyAsync(ExtensionsConfiguration config, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for Identity resources
/// </summary>
public interface IIdentityOperations
{
	IAsyncEnumerable<Identity> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<Identity> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<Identity> CreateAsync(Identity identity, CancellationToken cancellationToken = default);
	Task<Identity> UpdateAsync(Identity identity, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<Identity>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<Identity> ApplyAsync(Identity identity, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for ControlPlane resources
/// </summary>
public interface IControlPlaneOperations
{
	IAsyncEnumerable<ControlPlane> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<ControlPlane> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<ControlPlane> CreateAsync(ControlPlane controlPlane, CancellationToken cancellationToken = default);
	Task<ControlPlane> UpdateAsync(ControlPlane controlPlane, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<ControlPlane>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<ControlPlane> ApplyAsync(ControlPlane controlPlane, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for LoadBalancerConfig resources
/// </summary>
public interface ILoadBalancerOperations
{
	IAsyncEnumerable<LoadBalancerConfig> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<LoadBalancerConfig> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<LoadBalancerConfig> CreateAsync(LoadBalancerConfig config, CancellationToken cancellationToken = default);
	Task<LoadBalancerConfig> UpdateAsync(LoadBalancerConfig config, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<LoadBalancerConfig>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<LoadBalancerConfig> ApplyAsync(LoadBalancerConfig config, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for TalosConfig resources
/// </summary>
public interface ITalosConfigOperations
{
	IAsyncEnumerable<TalosConfig> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<TalosConfig> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<TalosConfig> CreateAsync(TalosConfig config, CancellationToken cancellationToken = default);
	Task<TalosConfig> UpdateAsync(TalosConfig config, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<TalosConfig>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<TalosConfig> ApplyAsync(TalosConfig config, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for KubernetesNode resources
/// </summary>
public interface IKubernetesNodeOperations
{
	IAsyncEnumerable<KubernetesNode> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<KubernetesNode> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<KubernetesNode> CreateAsync(KubernetesNode node, CancellationToken cancellationToken = default);
	Task<KubernetesNode> UpdateAsync(KubernetesNode node, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<KubernetesNode>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<KubernetesNode> ApplyAsync(KubernetesNode node, bool dryRun = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Operations for MachineClass resources
/// </summary>
public interface IMachineClassOperations
{
	IAsyncEnumerable<MachineClass> ListAsync(string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<MachineClass> GetAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	Task<MachineClass> CreateAsync(MachineClass machineClass, CancellationToken cancellationToken = default);
	Task<MachineClass> UpdateAsync(MachineClass machineClass, string? currentVersion = null, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default);
	IAsyncEnumerable<ResourceEvent<MachineClass>> WatchAsync(string? @namespace = "default", string? id = null, CancellationToken cancellationToken = default);
	Task<MachineClass> ApplyAsync(MachineClass machineClass, bool dryRun = false, CancellationToken cancellationToken = default);
}
