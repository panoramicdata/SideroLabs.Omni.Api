using System.Reflection;
using Grpc.Core;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Tests.Infrastructure;

/// <summary>
/// In-memory <see cref="IOmniResourceClient"/> built on DispatchProxy for offline tests
/// </summary>
internal class FakeResourceClient : DispatchProxy
{
	private readonly Dictionary<(Type Type, string Id), IOmniResource> _store = [];

	public List<string> Calls { get; } = [];

	public static (IOmniResourceClient Client, FakeResourceClient Fake) Create()
	{
		var client = Create<IOmniResourceClient, FakeResourceClient>();
		return (client, (FakeResourceClient)(object)client);
	}

	public void Seed(IOmniResource resource) => _store[(resource.GetType(), resource.Metadata.Id)] = resource;

	public bool Contains<T>(string id) where T : IOmniResource => _store.ContainsKey((typeof(T), id));

	public T Stored<T>(string id) where T : IOmniResource => (T)_store[(typeof(T), id)];

	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		var method = targetMethod!;
		var arguments = args ?? [];
		Calls.Add(method.Name);

		return method.Name switch
		{
			"CreateAsync" or "UpdateAsync" or "ApplyAsync" => Save(method, (IOmniResource)arguments[0]!),
			"GetAsync" => Load(method, (string)arguments[0]!),
			"DeleteAsync" => Remove(method, (string)arguments[0]!),
			"ListAsync" => Enumerate(method),
			_ => throw new NotSupportedException(method.Name)
		};
	}

	private object Save(MethodInfo method, IOmniResource resource)
	{
		Seed(resource);
		return TaskOf(method.ReturnType, resource);
	}

	private object Load(MethodInfo method, string id)
	{
		var type = method.GetGenericArguments()[0];
		if (!_store.TryGetValue((type, id), out var resource))
		{
			throw new RpcException(new Status(StatusCode.NotFound, "not found"));
		}

		return TaskOf(method.ReturnType, resource);
	}

	private Task Remove(MethodInfo method, string id)
	{
		_store.Remove((method.GetGenericArguments()[0], id));
		return Task.CompletedTask;
	}

	private object Enumerate(MethodInfo method)
	{
		var type = method.GetGenericArguments()[0];
		var items = _store.Where(pair => pair.Key.Type == type).Select(pair => pair.Value).ToList();
		return typeof(FakeResourceClient)
			.GetMethod(nameof(AsAsync), BindingFlags.Static | BindingFlags.NonPublic)!
			.MakeGenericMethod(type)
			.Invoke(null, [items])!;
	}

	private static object TaskOf(Type taskType, object value) => typeof(Task)
		.GetMethod(nameof(Task.FromResult))!
		.MakeGenericMethod(taskType.GetGenericArguments()[0])
		.Invoke(null, [value])!;

	private static async IAsyncEnumerable<T> AsAsync<T>(List<IOmniResource> items)
	{
		foreach (var item in items.OfType<T>())
		{
			yield return item;
		}

		await Task.CompletedTask;
	}
}
