using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of Machine operations
/// </summary>
internal class MachineOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<Machine>(resources, options), IMachineOperations
{
	public async Task LockAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default)
	{
		var machine = await GetAsync(id, @namespace, cancellationToken);
		machine.Status ??= new MachineStatus();
		machine.Status.Locked = true;
		await UpdateAsync(machine, cancellationToken: cancellationToken);
	}

	public async Task UnlockAsync(string id, string? @namespace = "default", CancellationToken cancellationToken = default)
	{
		var machine = await GetAsync(id, @namespace, cancellationToken);
		machine.Status ??= new MachineStatus();
		machine.Status.Locked = false;
		await UpdateAsync(machine, cancellationToken: cancellationToken);
	}
}

/// <summary>
/// Simple resource operations implementations
/// </summary>
internal class ClusterMachineOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<ClusterMachine>(resources, options), IClusterMachineOperations
{
}

internal class MachineSetOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<MachineSet>(resources, options), IMachineSetOperations
{
}

internal class MachineSetNodeOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<MachineSetNode>(resources, options), IMachineSetNodeOperations
{
}

internal class ConfigPatchOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<ConfigPatch>(resources, options), IConfigPatchOperations
{
}

internal class ExtensionsConfigurationOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<ExtensionsConfiguration>(resources, options), IExtensionsConfigurationOperations
{
}

internal class IdentityOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<Identity>(resources, options), IIdentityOperations
{
}

internal class ControlPlaneOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<ControlPlane>(resources, options), IControlPlaneOperations
{
}

internal class LoadBalancerOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<LoadBalancerConfig>(resources, options), ILoadBalancerOperations
{
}

internal class TalosConfigOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<TalosConfig>(resources, options), ITalosConfigOperations
{
}

internal class KubernetesNodeOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<KubernetesNode>(resources, options), IKubernetesNodeOperations
{
}

internal class MachineClassOperations(IOmniResourceClient resources, OmniClientOptions options) : ResourceOperationsBase<MachineClass>(resources, options), IMachineClassOperations
{
}
