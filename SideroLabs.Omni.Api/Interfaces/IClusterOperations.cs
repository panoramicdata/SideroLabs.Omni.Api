using System;
using System.Threading;
using System.Threading.Tasks;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

public interface IClusterOperations
{
    /// <summary>
    /// Gets cluster status information. Returns an implementation-specific status object.
    /// </summary>
    Task<object> GetStatusAsync(
        string clusterName,
        TimeSpan? waitTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a cluster resource.
    /// </summary>
    Task<IOmniResource> CreateAsync(
        IOmniResource cluster,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a cluster by name
    /// </summary>
    Task DeleteAsync(
        string clusterName,
        bool force = false,
        CancellationToken cancellationToken = default);

    Task LockMachineAsync(
        string machineId,
        string clusterName,
        CancellationToken cancellationToken = default);

    Task UnlockMachineAsync(
        string machineId,
        string clusterName,
        CancellationToken cancellationToken = default);
}
