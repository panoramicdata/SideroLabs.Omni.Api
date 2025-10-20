using System.Runtime.CompilerServices;

namespace SideroLabs.Omni.Api.Extensions;

/// <summary>
/// Extension methods for IAsyncEnumerable to provide convenient list operations
/// WARNING: These methods load all items into memory. Use sparingly for large datasets.
/// </summary>
public static class AsyncEnumerableExtensions
{
	/// <summary>
	/// Materializes the async enumerable into a List
	/// WARNING: This loads ALL items into memory. Only use for small datasets.
	/// For large datasets, use await foreach instead.
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A list containing all items</returns>
	/// <example>
	/// <code>
	/// // For small datasets (recommended):
	/// var clusters = await client.Resources.ListAsync&lt;Cluster&gt;().ToListAsync();
	///
	/// // For large datasets (more efficient):
	/// await foreach (var cluster in client.Resources.ListAsync&lt;Cluster&gt;())
	/// {
	///     ProcessCluster(cluster);
	/// }
	/// </code>
	/// </example>
	public static async Task<List<T>> ToListAsync<T>(
		this IAsyncEnumerable<T> source,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);

		var list = new List<T>();

		await foreach (var item in source.WithCancellation(cancellationToken))
		{
			list.Add(item);
		}

		return list;
	}

	/// <summary>
	/// Materializes the async enumerable into an Array
	/// WARNING: This loads ALL items into memory. Only use for small datasets.
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An array containing all items</returns>
	public static async Task<T[]> ToArrayAsync<T>(
		this IAsyncEnumerable<T> source,
		CancellationToken cancellationToken = default)
	{
		var list = await source.ToListAsync(cancellationToken);
		return [.. list];
	}

	/// <summary>
	/// Gets the first item or null if empty
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The first item or null</returns>
	public static async Task<T?> FirstOrDefaultAsync<T>(
		this IAsyncEnumerable<T> source,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);

		await foreach (var item in source.WithCancellation(cancellationToken))
		{
			return item;
		}

		return default;
	}

	/// <summary>
	/// Counts the number of items
	/// WARNING: This enumerates all items. Only use when you need just the count.
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The number of items</returns>
	public static async Task<int> CountAsync<T>(
		this IAsyncEnumerable<T> source,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);

		var count = 0;

		await foreach (var _ in source.WithCancellation(cancellationToken))
		{
			count++;
		}

		return count;
	}

	/// <summary>
	/// Checks if any items exist
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if at least one item exists</returns>
	public static async Task<bool> AnyAsync<T>(
		this IAsyncEnumerable<T> source,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);

		await foreach (var _ in source.WithCancellation(cancellationToken))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Takes only the first N items
	/// This is memory efficient as it doesn't load everything
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="count">Number of items to take</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An async enumerable with at most count items</returns>
	public static async IAsyncEnumerable<T> TakeAsync<T>(
		this IAsyncEnumerable<T> source,
		int count,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);

		if (count <= 0)
		{
			yield break;
		}

		var taken = 0;

		await foreach (var item in source.WithCancellation(cancellationToken))
		{
			yield return item;
			taken++;

			if (taken >= count)
			{
				break;
			}
		}
	}

	/// <summary>
	/// Skips the first N items
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="count">Number of items to skip</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An async enumerable skipping the first count items</returns>
	public static async IAsyncEnumerable<T> SkipAsync<T>(
		this IAsyncEnumerable<T> source,
		int count,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);

		var skipped = 0;

		await foreach (var item in source.WithCancellation(cancellationToken))
		{
			if (skipped < count)
			{
				skipped++;
				continue;
			}

			yield return item;
		}
	}

	/// <summary>
	/// Filters items based on a predicate
	/// </summary>
	/// <typeparam name="T">The type of items</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="predicate">The filter predicate</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An async enumerable of filtered items</returns>
	public static async IAsyncEnumerable<T> WhereAsync<T>(
		this IAsyncEnumerable<T> source,
		Func<T, bool> predicate,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(predicate);

		await foreach (var item in source.WithCancellation(cancellationToken))
		{
			if (predicate(item))
			{
				yield return item;
			}
		}
	}

	/// <summary>
	/// Projects each item to a new form
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TResult">The result type</typeparam>
	/// <param name="source">The async enumerable source</param>
	/// <param name="selector">The projection function</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An async enumerable of projected items</returns>
	public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(
		this IAsyncEnumerable<TSource> source,
		Func<TSource, TResult> selector,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(selector);

		await foreach (var item in source.WithCancellation(cancellationToken))
		{
			yield return selector(item);
		}
	}
}
