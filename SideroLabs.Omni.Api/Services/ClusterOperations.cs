using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Services;

internal class ClusterOperations : IClusterOperations
{
	private readonly IOmniResourceClient _resources;
	private readonly OmniClientOptions _options;

	// Internal constructor used for wiring from OmniClient
	internal ClusterOperations(IOmniResourceClient resources, OmniClientOptions options)
	{
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}

	public async IAsyncEnumerable<Cluster> ListAsync(
		string? @namespace = "default",
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var cluster in _resources.ListAsync<Cluster>(@namespace, cancellationToken: cancellationToken))
		{
			yield return cluster;
		}
	}

	public async Task<Cluster> GetAsync(
		string clusterId,
		string? @namespace = "default",
		CancellationToken cancellationToken = default)
	{
		return await _resources.GetAsync<Cluster>(clusterId, @namespace ?? _options.DefaultNamespace, cancellationToken);
	}

	public async Task<object> GetStatusAsync(string clusterName, TimeSpan? waitTimeout = null, CancellationToken cancellationToken = default)
	{
		// Retrieve cluster resource and return its status object
		var cluster = await _resources.GetAsync<Cluster>(clusterName, _options.DefaultNamespace, cancellationToken);
		return cluster.Status ?? new() { Ready = false };
	}

	public async Task<IOmniResource> CreateAsync(IOmniResource cluster, CancellationToken cancellationToken = default)
	{
		if (cluster is Cluster c)
		{
			return await _resources.ApplyAsync(c, false, cancellationToken);
		}

		// Try to cast via serialization fallback
		var json = Serialization.ResourceSerializer.ToJson(cluster);
		var created = Serialization.ResourceSerializer.FromJson<Cluster>(json) ?? throw new InvalidOperationException("Failed to convert resource to Cluster");
		return await _resources.ApplyAsync(created, false, cancellationToken);
	}

	public async Task DeleteAsync(string clusterName, bool force = false, CancellationToken cancellationToken = default)
	{
		// Currently ignore force and simply delete cluster resource
		await _resources.DeleteAsync<Cluster>(clusterName, _options.DefaultNamespace, cancellationToken);
	}

	public async Task LockMachineAsync(string machineId, string clusterName, CancellationToken cancellationToken = default)
	{
		var machine = await _resources.GetAsync<Resources.Machine>(machineId, _options.DefaultNamespace, cancellationToken)
			?? throw new InvalidOperationException("Machine not found");
		machine.Status ??= new MachineStatus();
		machine.Status.Locked = true;

		await _resources.UpdateAsync(machine, null, cancellationToken);
	}

	public async Task UnlockMachineAsync(string machineId, string clusterName, CancellationToken cancellationToken = default)
	{
		var machine = await _resources.GetAsync<Resources.Machine>(machineId, _options.DefaultNamespace, cancellationToken) ?? throw new InvalidOperationException("Machine not found");
		machine.Status ??= new MachineStatus();
		machine.Status.Locked = false;

		await _resources.UpdateAsync(machine, null, cancellationToken);
	}
}
