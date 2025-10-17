using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Interfaces;

public interface ITemplateOperations
{
	Task<object> LoadAsync(
		string filePath,
		CancellationToken cancellationToken);

	Task<List<IOmniResource>> RenderAsync(
		object template,
		Dictionary<string, object> variables,
		CancellationToken cancellationToken);

	Task ValidateAsync(
		object template,
		CancellationToken cancellationToken);

	IAsyncEnumerable<SyncResult> SyncAsync(
		object template,
		Dictionary<string, object> variables,
		bool dryRun,
		CancellationToken cancellationToken);

	Task<object> ExportAsync(
		string clusterName,
		CancellationToken cancellationToken);

	Task<List<object>> DiffAsync(
		object template,
		Dictionary<string, object> variables,
		CancellationToken cancellationToken);
}
