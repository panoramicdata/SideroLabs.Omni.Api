using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SideroLabs.Omni.Api.Services;

internal class TemplateOperations(
	IOmniResourceClient resources,
	ILogger logger) : ITemplateOperations
{
	public async Task<object> LoadAsync(string filePath, CancellationToken cancellationToken)
	{
		var yaml = await File.ReadAllTextAsync(filePath, cancellationToken);
		return yaml;
	}

	public Task<List<IOmniResource>> RenderAsync(object template, Dictionary<string, object> variables, CancellationToken cancellationToken)
	{
		var yaml = template as string ?? template?.ToString() ?? string.Empty;
		var docs = SplitYamlDocuments(yaml);

		var list = new List<IOmniResource>();

		foreach (var doc in docs)
		{
			if (string.IsNullOrWhiteSpace(doc)) continue;

			// Inspect kind
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();

			var map = deserializer.Deserialize<Dictionary<string, object>>(doc);
			if (map == null) continue;

			map.TryGetValue("kind", out var kindObj);
			var kind = (kindObj as string) ?? string.Empty;

			try
			{
				switch (kind)
				{
					case "Cluster":
						var cluster = ResourceSerializer.FromYaml<Cluster>(doc);
						if (cluster != null) list.Add(cluster);
						break;
					case "Machine":
						var machine = ResourceSerializer.FromYaml<Machine>(doc);
						if (machine != null) list.Add(machine);
						break;
					default:
						throw new NotSupportedException($"Kind {kind} not supported.");
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Failed to parse template document of kind {Kind}", kind);
			}
		}

		return Task.FromResult(list);
	}

	public Task ValidateAsync(object template, CancellationToken cancellationToken)
	{
		// Validation not implemented yet - placeholder
		return Task.CompletedTask;
	}

	public async IAsyncEnumerable<SyncResult> SyncAsync(
		object template,
		Dictionary<string, object> variables,
		bool dryRun,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var resources = await RenderAsync(template, variables, cancellationToken);

		foreach (var res in resources)
		{
			if (cancellationToken.IsCancellationRequested) yield break;

			SyncResult result;
			try
			{
				var applied = await InvokeApplyAsync(res, dryRun, cancellationToken).ConfigureAwait(false);
				result = new() { Action = dryRun ? "DryRunApply" : "Applied", Resource = applied ?? (object)res };
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error applying resource {ResourceId}", res.Metadata.Id);
				result = new() { Action = "Error", Resource = res, Error = ex.Message };
			}

			yield return result;
		}
	}

	public Task<object> ExportAsync(string clusterName, CancellationToken cancellationToken)
	{
		// Export not implemented yet
		throw new NotImplementedException();
	}

	public Task<List<object>> DiffAsync(
		object template,
		Dictionary<string, object> variables,
		CancellationToken cancellationToken)
	{
		// Diff not implemented yet
		return Task.FromResult(new List<object>());
	}

	/// <summary>
	/// Splits a YAML string into separate documents based on YAML document delimiters.
	/// </summary>
	/// <remarks>YAML documents are separated by lines starting with '---' according to the YAML specification.
	/// Leading and trailing whitespace is not removed from individual documents.</remarks>
	/// <param name="yaml">The YAML string to split. Cannot be null.</param>
	/// <returns>An array of strings, each containing a single YAML document. The array will be empty if the input contains no
	/// documents.</returns>
	private static string[] SplitYamlDocuments(string yaml) =>
		yaml.Split(["\n---", "\r\n---"], StringSplitOptions.RemoveEmptyEntries);

	private async Task<object?> InvokeApplyAsync(IOmniResource resource, bool dryRun, CancellationToken cancellationToken)
	{
		var method = resources.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
			.FirstOrDefault(m => m.Name == "ApplyAsync" && m.IsGenericMethod && m.GetGenericArguments().Length == 1) ?? throw new InvalidOperationException("ApplyAsync method not found on IOmniResourceClient implementation");
		var generic = method.MakeGenericMethod(resource.GetType());

		var taskObj = generic.Invoke(resources, new object?[] { resource, dryRun, cancellationToken });
		if (taskObj == null) return null;

		var asTask = taskObj as Task ?? throw new InvalidOperationException("ApplyAsync did not return a Task");
		await asTask.ConfigureAwait(false);

		var resultProp = taskObj.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
		return resultProp?.GetValue(taskObj);
	}
}
