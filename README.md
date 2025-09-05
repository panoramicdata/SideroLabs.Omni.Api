# SideroLabs.Omni.Api

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/YOUR_PROJECT_ID_HERE)](https://app.codacy.com/gh/panoramicdata/SideroLabs.Omni.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/YOUR_PROJECT_ID_HERE)](https://app.codacy.com/gh/panoramicdata/SideroLabs.Omni.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)
[![NuGet](https://img.shields.io/nuget/v/SideroLabs.Omni.Api.svg)](https://www.nuget.org/packages/SideroLabs.Omni.Api/)
[![NuGet](https://img.shields.io/nuget/dt/SideroLabs.Omni.Api.svg)](https://www.nuget.org/packages/SideroLabs.Omni.Api/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET client library for interacting with the [SideroLabs Omni](https://omni.siderolabs.com/) Management API using gRPC.

## Features

- **Complete API Coverage** - Support for all cluster and machine management operations
- **gRPC-based** - High-performance communication with the Omni Management API
- **Async/Await** - Modern asynchronous programming patterns throughout
- **Cancellation Support** - Proper cancellation token support for all operations
- **Dependency Injection** - Built-in support for .NET dependency injection
- **Type Safety** - Strongly typed models for all API operations
- **Authentication** - Bearer token authentication support
- **TLS Configuration** - Flexible TLS and certificate validation options

## Installation

Install the package via NuGet:

```bash
dotnet add package SideroLabs.Omni.Api
```

Or via the Package Manager Console:

```powershell
Install-Package SideroLabs.Omni.Api
```

## Quick Start

### Basic Usage

```csharp
using SideroLabs.Omni.Api;
using SideroLabs.Omni.Api.Models;

// Configure the client
var options = new OmniClientOptions
{
    Endpoint = "https://your-omni-instance.com:8443",
    AuthToken = "your-auth-token",
    TimeoutSeconds = 30
};

// Create the client
using var client = new OmniClient(options);
using var cts = new CancellationTokenSource();

// Get service status
var status = await client.GetStatusAsync(cts.Token);
Console.WriteLine($"Omni v{status.Version} - Ready: {status.Ready}");

// List clusters
var clusters = await client.ListClustersAsync(cts.Token);
foreach (var cluster in clusters.Clusters)
{
    Console.WriteLine($"Cluster: {cluster.Name} ({cluster.Status.Phase})");
}
```

### Dependency Injection

In your `Program.cs` or `Startup.cs`:

```csharp
using SideroLabs.Omni.Api.Extensions;

// Add the Omni client to DI
builder.Services.AddOmniClient(options =>
{
    options.Endpoint = builder.Configuration["Omni:Endpoint"];
    options.AuthToken = builder.Configuration["Omni:AuthToken"];
    options.TimeoutSeconds = 30;
});

// Or from configuration section
builder.Services.Configure<OmniClientOptions>(
    builder.Configuration.GetSection("Omni"));
builder.Services.AddSingleton<OmniClient>();
```

Configuration in `appsettings.json`:

```json
{
  "Omni": {
    "Endpoint": "https://your-omni-instance.com:8443",
    "AuthToken": "your-auth-token",
    "TimeoutSeconds": 30,
    "UseTls": true,
    "ValidateCertificate": true
  }
}
```

## API Operations

### Cluster Management

```csharp
// Create a cluster
var spec = new ClusterSpec
{
    KubernetesVersion = "v1.28.0",
    TalosVersion = "v1.5.0",
    Features = new List<string> { "embedded-discovery-service" }
};

var createResponse = await client.CreateClusterAsync("my-cluster", spec, cancellationToken);

// Get a specific cluster
var cluster = await client.GetClusterAsync("cluster-id", cancellationToken);

// Update a cluster
var updateResponse = await client.UpdateClusterAsync("cluster-id", updatedSpec, cancellationToken);

// Delete a cluster
await client.DeleteClusterAsync("cluster-id", cancellationToken);
```

### Machine Management

```csharp
// List machines in a cluster
var machines = await client.ListMachinesAsync("cluster-id", cancellationToken);

// Get a specific machine
var machine = await client.GetMachineAsync("machine-id", cancellationToken);

// Update machine labels
var machineSpec = new MachineSpec
{
    Role = "worker",
    Labels = new Dictionary<string, string>
    {
        { "environment", "production" },
        { "zone", "us-west-2a" }
    }
};

await client.UpdateMachineAsync("machine-id", machineSpec, cancellationToken);
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Endpoint` | `string` | Required | The gRPC endpoint URL for the Omni Management API |
| `AuthToken` | `string?` | `null` | Bearer token for authentication |
| `TimeoutSeconds` | `int` | `30` | Timeout for gRPC calls in seconds |
| `UseTls` | `bool` | `true` | Whether to use TLS for the connection |
| `ValidateCertificate` | `bool` | `true` | Whether to validate the server certificate |

## Error Handling

The client throws standard .NET exceptions for error conditions:

```csharp
try
{
    var clusters = await client.ListClustersAsync(cancellationToken);
}
catch (OperationCanceledException)
{
    // Handle cancellation
}
catch (RpcException ex)
{
    // Handle gRPC-specific errors
    Console.WriteLine($"gRPC Error: {ex.Status.Detail}");
}
catch (Exception ex)
{
    // Handle other errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Development Status

This library is currently in development. The API surface is stable, but the underlying implementation is transitioning from mock responses to real gRPC calls to the Omni Management API.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For questions and support, please open an issue on the [GitHub repository](https://github.com/panoramicdata/SideroLabs.Omni.Api/issues).

---

**Copyright © Panoramic Data Limited 2025**
