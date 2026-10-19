# Working with IAsyncEnumerable in SideroLabs.Omni.Api

## Overview

The SideroLabs.Omni.Api library uses `IAsyncEnumerable<T>` for streaming operations because:

1. **Memory Efficiency** - Processes items one at a time instead of loading everything into memory
2. **Streaming Protocol** - gRPC streams data, so we stream it to you
3. **Large Datasets** - Handles 10,000+ resources without memory issues
4. **Standard .NET Pattern** - Same pattern used by Entity Framework Core, ASP.NET Core, and other modern libraries

## ⚡ Quick Reference

| Scenario | Recommended Approach | Reason |
|----------|---------------------|---------|
| **Small datasets (<100 items)** | `.ToListAsync()` | Convenient, no memory concerns |
| **Large datasets (1000+ items)** | `await foreach` | Memory efficient, streaming |
| **Unknown dataset size** | `await foreach` | Safe default |
| **Need first item only** | `.FirstOrDefaultAsync()` | Stops after first item |
| **Just counting** | `.CountAsync()` | Enumerates but doesn't store |
| **Infinite streams (Watch)** | `await foreach` | **Required** - stream never ends |

## 📚 Usage Patterns

### Pattern 1: Simple List (Small Datasets)

**When to use**: You have <100 items and want a simple list.

```csharp
using SideroLabs.Omni.Api.Extensions;

// ✅ Good for small datasets
var clusters = await client.Resources.ListAsync<Cluster>().ToListAsync();

foreach (var cluster in clusters)
{
    Console.WriteLine($"Cluster: {cluster.Metadata.Id}");
}
```

**⚠️ Warning**: This loads ALL items into memory before returning.

### Pattern 2: Streaming (Large Datasets)

**When to use**: You have 1000+ items or unknown size.

```csharp
// ✅ Memory efficient - processes one at a time
await foreach (var cluster in client.Resources.ListAsync<Cluster>())
{
    Console.WriteLine($"Cluster: {cluster.Metadata.Id}");
    await ProcessClusterAsync(cluster);
    // ✅ Previous cluster can be garbage collected
}
```

**Advantages**:
- Constant memory usage
- Starts processing immediately
- Can stop early without loading rest

### Pattern 3: Get First Item Only

```csharp
using SideroLabs.Omni.Api.Extensions;

// ✅ Efficient - stops after first item
var firstCluster = await client.Resources.ListAsync<Cluster>()
    .FirstOrDefaultAsync();

if (firstCluster != null)
{
    Console.WriteLine($"First cluster: {firstCluster.Metadata.Id}");
}
```

### Pattern 4: Take First N Items

```csharp
using SideroLabs.Omni.Api.Extensions;

// ✅ Memory efficient - only loads 10 items
var topClusters = await client.Resources.ListAsync<Cluster>()
    .TakeAsync(10)
    .ToListAsync();
```

### Pattern 5: Filter and Transform

```csharp
using SideroLabs.Omni.Api.Extensions;

// ✅ Memory efficient - filters during streaming
await foreach (var clusterId in client.Resources.ListAsync<Cluster>()
    .WhereAsync(c => c.Status?.Ready == true)
    .SelectAsync(c => c.Metadata.Id))
{
    Console.WriteLine($"Ready cluster: {clusterId}");
}
```

### Pattern 6: Watch for Changes (Infinite Stream)

```csharp
// ✅ REQUIRED pattern - stream never ends
await foreach (var evt in client.Resources.WatchAsync<Cluster>(cancellationToken: ct))
{
    Console.WriteLine($"{evt.Type}: {evt.Resource.Metadata.Id}");
    
    // Process event...
    
    // Stream continues indefinitely until cancelled
}
```

**❌ DON'T DO THIS** with Watch:
```csharp
// ❌ NEVER DO THIS - Will hang forever!
var events = await client.Resources.WatchAsync<Cluster>().ToListAsync();
```

## 📊 Performance Comparison

### Small Dataset (10 clusters)

```csharp
// Option 1: ToListAsync() - ✅ Fine
var clusters = await client.Resources.ListAsync<Cluster>().ToListAsync();
// Memory: ~10KB
// Time: ~100ms

// Option 2: await foreach - ✅ Also fine
await foreach (var cluster in client.Resources.ListAsync<Cluster>())
{
    // ...
}
// Memory: ~1KB at a time
// Time: ~100ms (same)
```

**Winner**: **Either works** - use `.ToListAsync()` for convenience.

### Large Dataset (10,000 machines)

```csharp
// Option 1: ToListAsync() - ❌ Problematic
var machines = await client.Resources.ListAsync<Machine>().ToListAsync();
// Memory: ~100MB all at once
// Time: ~10 seconds before you can start processing
// Risk: OutOfMemoryException, GC pressure

// Option 2: await foreach - ✅ Recommended
await foreach (var machine in client.Resources.ListAsync<Machine>())
{
    await ProcessMachineAsync(machine);
}
// Memory: ~10KB at a time (constant)
// Time: Starts processing immediately
// Benefit: Can stop early if needed
```

**Winner**: **await foreach** - memory efficient and responsive.

## 🛠️ Available Extension Methods

We provide convenient extension methods for common scenarios:

```csharp
using SideroLabs.Omni.Api.Extensions;

// Convert to list (loads all into memory)
List<T> list = await source.ToListAsync();

// Convert to array (loads all into memory)
T[] array = await source.ToArrayAsync();

// Get first item or null
T? first = await source.FirstOrDefaultAsync();

// Count items (enumerates all)
int count = await source.CountAsync();

// Check if any items exist
bool any = await source.AnyAsync();

// Take first N items (memory efficient)
IAsyncEnumerable<T> top10 = source.TakeAsync(10);

// Skip first N items
IAsyncEnumerable<T> rest = source.SkipAsync(10);

// Filter (memory efficient)
IAsyncEnumerable<T> filtered = source.WhereAsync(predicate);

// Transform (memory efficient)
IAsyncEnumerable<R> transformed = source.SelectAsync(selector);
```

## 🎯 Decision Guide

Use this flowchart to decide which approach to use:

```text
┌──────────────────────────────────┐
│ Is it a Watch operation?         │
└───────────┬──────────────────────┘
            │
    Yes ────┴──► Use await foreach (REQUIRED)
            │
            No
            │
┌───────────▼──────────────────────┐
│ Do you know the dataset size?     │
└───────────┬──────────────────────┘
            │
    No ─────┴──► Use await foreach (SAFE DEFAULT)
            │
            Yes
            │
┌───────────▼──────────────────────┐
│ Is it < 100 items?                 │
└───────────┬──────────────────────┘
            │
    Yes ────┴──► .ToListAsync() OK (CONVENIENT)
            │
            No
            │
┌───────────▼──────────────────────┐
│ Is it > 1000 items?                │
└───────────┬──────────────────────┘
            │
    Yes ────┴──► Use await foreach (EFFICIENT)
            │
            No
            │
└──────────► Either approach works (YOUR PREFERENCE)
```

## 💡 Best Practices

### ✅ DO

```csharp
// ✅ Use ToListAsync for known small datasets
var clusters = await client.Resources.ListAsync<Cluster>()
    .ToListAsync();

// ✅ Use await foreach for unknown or large datasets
await foreach (var machine in client.Resources.ListAsync<Machine>())
{
    await ProcessAsync(machine);
}

// ✅ Use FirstOrDefaultAsync when you need just one
var cluster = await client.Resources.ListAsync<Cluster>()
    .FirstOrDefaultAsync();

// ✅ Use TakeAsync for top-N scenarios
var top10 = await client.Resources.ListAsync<Cluster>()
    .TakeAsync(10)
    .ToListAsync();

// ✅ Cancel watch operations
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
await foreach (var evt in client.Resources.WatchAsync<Cluster>(cancellationToken: cts.Token))
{
    // Process events...
}
```

### ❌ DON'T

```csharp
// ❌ DON'T use ToListAsync on Watch (infinite stream)
var events = await client.Resources.WatchAsync<Cluster>().ToListAsync();

// ❌ DON'T use ToListAsync for large datasets without considering memory
var allMachines = await client.Resources.ListAsync<Machine>().ToListAsync();
// Could load 10,000+ machines into memory!

// ❌ DON'T load everything just to count
var allClusters = await client.Resources.ListAsync<Cluster>().ToListAsync();
var count = allClusters.Count; // ❌ Inefficient

// ✅ DO THIS instead
var count = await client.Resources.ListAsync<Cluster>().CountAsync(); // ✅ Efficient
```

## 🔍 Real-World Examples

### Example 1: Dashboard (Small Dataset)

**Scenario**: Dashboard showing cluster summary (< 50 clusters)

```csharp
using SideroLabs.Omni.Api.Extensions;

// ✅ ToListAsync is fine here
var clusters = await client.Resources.ListAsync<Cluster>().ToListAsync();

return new DashboardModel
{
    TotalClusters = clusters.Count,
    ReadyClusters = clusters.Count(c => c.Status?.Ready == true),
    Clusters = clusters.Select(c => new ClusterSummary
    {
        Name = c.Metadata.Id,
        Status = c.Status?.Phase.ToString()
    }).ToList()
};
```

### Example 2: Bulk Processing (Large Dataset)

**Scenario**: Processing all machines for inventory (10,000+ machines)

```csharp
// ✅ await foreach is efficient
var processed = 0;
await foreach (var machine in client.Resources.ListAsync<Machine>())
{
    await UpdateInventoryAsync(machine);
    processed++;
    
    if (processed % 100 == 0)
    {
        _logger.LogInformation("Processed {Count} machines", processed);
    }
}
```

### Example 3: Search (Unknown Size)

**Scenario**: Search for clusters by name pattern

```csharp
using SideroLabs.Omni.Api.Extensions;

// ✅ Filter during streaming, then collect results
var results = await client.Resources.ListAsync<Cluster>()
    .WhereAsync(c => c.Metadata.Id.Contains(searchTerm))
    .TakeAsync(20) // Limit results
    .ToListAsync();
```

### Example 4: Real-Time Monitoring (Infinite Stream)

**Scenario**: Monitor cluster events in real-time

```csharp
// ✅ Must use await foreach for Watch
using var cts = new CancellationTokenSource();

await foreach (var evt in client.Resources.WatchAsync<Cluster>(
    tailEvents: 10,
    cancellationToken: cts.Token))
{
    switch (evt.Type)
    {
        case ResourceEventType.Created:
            _logger.LogInformation("Cluster created: {Id}", evt.Resource.Metadata.Id);
            await NotifyAdminsAsync($"New cluster: {evt.Resource.Metadata.Id}");
            break;
            
        case ResourceEventType.Updated:
            if (evt.OldResource?.Status?.Ready != true && evt.Resource.Status?.Ready == true)
            {
                _logger.LogInformation("Cluster became ready: {Id}", evt.Resource.Metadata.Id);
            }
            break;
            
        case ResourceEventType.Destroyed:
            _logger.LogInformation("Cluster deleted: {Id}", evt.Resource.Metadata.Id);
            break;
    }
}
```

## 📖 Further Reading

- [IAsyncEnumerable in C#](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#asynchronous-streams)
- [gRPC Streaming](https://grpc.io/docs/what-is-grpc/core-concepts/#server-streaming-rpc)
- [Memory Management in .NET](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)

## 🤝 Feedback

If you find the `IAsyncEnumerable<T>` pattern confusing or have suggestions for additional convenience methods, please [open an issue](https://github.com/panoramicdata/SideroLabs.Omni.Api/issues).

We're committed to providing both:
- ✅ **Performance** (streaming for large datasets)
- ✅ **Convenience** (simple methods for common cases)
