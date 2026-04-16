using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Provides templating operations equivalent to omnictl template workflows.
/// </summary>
public interface ITemplateOperations
{
	/// <summary>
	/// Loads a template document from disk.
	/// </summary>
	/// <param name="filePath">The absolute or relative path to the template file.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>A parsed template representation understood by the implementation.</returns>
	Task<object> LoadAsync(
		string filePath,
		CancellationToken cancellationToken);

	/// <summary>
	/// Renders a template using the supplied variable map.
	/// </summary>
	/// <param name="template">The loaded template instance.</param>
	/// <param name="variables">Template variables keyed by placeholder name.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>A concrete list of Omni resources produced from the template.</returns>
	Task<List<IOmniResource>> RenderAsync(
		object template,
		Dictionary<string, object> variables,
		CancellationToken cancellationToken);

	/// <summary>
	/// Validates template structure and semantic compatibility before render or sync.
	/// </summary>
	/// <param name="template">The template instance to validate.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	Task ValidateAsync(
		object template,
		CancellationToken cancellationToken);

	/// <summary>
	/// Synchronizes rendered resources with Omni state and returns per-resource outcomes.
	/// </summary>
	/// <param name="template">The template instance to synchronize.</param>
	/// <param name="variables">Template variables keyed by placeholder name.</param>
	/// <param name="dryRun"><see langword="true"/> to simulate writes without persisting changes.</param>
	/// <param name="cancellationToken">A token that can cancel the asynchronous stream.</param>
	/// <returns>A stream of sync outcomes for each rendered resource.</returns>
	IAsyncEnumerable<SyncResult> SyncAsync(
		object template,
		Dictionary<string, object> variables,
		bool dryRun,
		CancellationToken cancellationToken);

	/// <summary>
	/// Exports a cluster template representation from an existing Omni cluster.
	/// </summary>
	/// <param name="clusterName">The cluster name to export.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>An implementation-specific template object.</returns>
	Task<object> ExportAsync(
		string clusterName,
		CancellationToken cancellationToken);

	/// <summary>
	/// Computes the difference between template-rendered resources and current Omni state.
	/// </summary>
	/// <param name="template">The template instance to compare.</param>
	/// <param name="variables">Template variables keyed by placeholder name.</param>
	/// <param name="cancellationToken">A token that can cancel the operation.</param>
	/// <returns>A list of implementation-specific diff objects.</returns>
	Task<List<object>> DiffAsync(
		object template,
		Dictionary<string, object> variables,
		CancellationToken cancellationToken);
}
