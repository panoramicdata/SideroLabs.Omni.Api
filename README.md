# SideroLabs.Omni.Api

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/YOUR_PROJECT_ID_HERE)](https://app.codacy.com/gh/panoramicdata/SideroLabs.Omni.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/YOUR_PROJECT_ID_HERE)](https://app.codacy.com/gh/panoramicdata/SideroLabs.Omni.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)
[![NuGet](https://img.shields.io/nuget/v/SideroLabs.Omni.Api.svg)](https://www.nuget.org/packages/SideroLabs.Omni.Api/)
[![NuGet](https://img.shields.io/nuget/dt/SideroLabs.Omni.Api.svg)](https://www.nuget.org/packages/SideroLabs.Omni.Api/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET client library for interacting with the [SideroLabs Omni](https://omni.siderolabs.com/) Management API using gRPC with proper PGP-based authentication.

## Features

- **Complete API Coverage** - Support for all cluster and machine management operations
- **gRPC-based** - High-performance communication with the Omni Management API
- **PGP Authentication** - Implements Sidero Labs' official gRPC request signing mechanism
- **Async/Await** - Modern asynchronous programming patterns throughout
- **Cancellation Support** - Proper cancellation token support for all operations
- **Dependency Injection** - Built-in support for .NET dependency injection
- **Type Safety** - Strongly typed models for all API operations
- **TLS Configuration** - Flexible TLS and certificate validation options
- **Safety Features** - Built-in safeguards against accidental destructive operations

## Installation

Install the package via NuGet:

```bash
dotnet add package SideroLabs.Omni.Api
```

Or via the Package Manager Console:

```powershell
Install-Package SideroLabs.Omni.Api
```

## Authentication

Omni uses a custom PGP-based authentication mechanism that signs each gRPC request. This library implements the official [go-api-signature](https://github.com/siderolabs/go-api-signature) specification.

### PGP Key Requirements

You need a PGP private key from Sidero Labs in one of these formats:
- **Ed25519** (recommended)
- **RSA**
- **ECDSA**

### Authentication Format

The authentication is handled automatically by signing each gRPC request with three headers:
- `x-sidero-timestamp`: Request timestamp
- `x-sidero-payload`: JSON payload containing method and selected headers
- `x-sidero-signature`: `siderov1 {identity} {fingerprint} {base64_signature}`

## Quick Start

### Method 1: Direct PGP Key Content (Recommended)

```csharp
using SideroLabs.Omni.Api;
using SideroLabs.Omni.Api.Models;

// Configure the client with PGP authentication
var options = new OmniClientOptions
{
    Endpoint = "https://your-omni-instance.com:8443",
    Identity = "your-username",
    PgpPrivateKey = @"-----BEGIN PGP PRIVATE KEY BLOCK-----
lQHYBGU7... your PGP private key content ...
-----END PGP PRIVATE KEY BLOCK-----",
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

### Method 2: PGP Key File

If you have a PGP key file from Sidero Labs (base64-encoded JSON format):

```csharp
var options = new OmniClientOptions
{
    Endpoint = "https://your-omni-instance.com:8443",
    PgpKeyFilePath = "/path/to/your/pgp-key-file.txt",
    TimeoutSeconds = 30
};
```

### Dependency Injection

In your `Program.cs` or `Startup.cs`:

```csharp
using SideroLabs.Omni.Api.Extensions;

// Configure from appsettings.json
builder.Services.Configure<OmniClientOptions>(
    builder.Configuration.GetSection("Omni"));

// Add the Omni client to DI
builder.Services.AddOmniClient();

// Or configure directly
builder.Services.AddOmniClient(options =>
{
    options.Endpoint = builder.Configuration["Omni:Endpoint"];
    options.Identity = builder.Configuration["Omni:Identity"];
    options.PgpPrivateKey = builder.Configuration["Omni:PgpPrivateKey"];
    options.TimeoutSeconds = 30;
});
```

Configuration in `appsettings.json`:

```json
{
  "Omni": {
    "Endpoint": "https://your-omni-instance.com:8443",
    "Identity": "your-username",
    "PgpPrivateKey": "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...\n-----END PGP PRIVATE KEY BLOCK-----",
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
| `Identity` | `string?` | `null` | Your username/identity for PGP authentication |
| `PgpPrivateKey` | `string?` | `null` | PGP private key content (armored format) |
| `PgpKeyFilePath` | `string?` | `null` | Alternative: path to PGP key file |
| `TimeoutSeconds` | `int` | `30` | Timeout for gRPC calls in seconds |
| `UseTls` | `bool` | `true` | Whether to use Transport Layer Security for the connection |
| `ValidateCertificate` | `bool` | `true` | Whether to validate the server certificate |

## Safety Features

The library includes built-in safety features to prevent accidental destructive operations:

- **Production Credential Protection**: Destructive operations (create, update, delete) are blocked when using production credentials during development
- **Non-destructive Testing**: Read operations (list, get, status) work safely with any credentials
- **Graceful Authentication Failures**: Invalid PGP keys don't crash the client but log warnings

## Authentication Technical Details

This library implements the Sidero Labs authentication mechanism exactly as specified in [go-api-signature](https://github.com/siderolabs/go-api-signature):

1. **Request Timestamping**: Each request gets a Unix timestamp
2. **Payload Construction**: Creates a JSON payload with the gRPC method and selected headers
3. **PGP Signing**: Signs the payload using your PGP private key
4. **Header Injection**: Adds authentication headers to the gRPC metadata

### Supported PGP Key Types

- **Ed25519**: Modern elliptic curve (recommended by Sidero Labs)
- **RSA**: Traditional RSA keys (RSA-SHA256 signature)
- **ECDSA**: Elliptic Curve DSA (ES256 signature)

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
catch (InvalidOperationException ex) when (ex.Message.Contains("PGP"))
{
    // Handle PGP authentication errors
    Console.WriteLine($"Authentication Error: {ex.Message}");
}
catch (Exception ex)
{
    // Handle other errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Development Status

This library is currently in active development:

- ✅ **Phase 1 Complete**: PGP-based authentication mechanism
- 🔄 **Phase 2 In Progress**: gRPC client integration
- 📋 **Phase 3 Planned**: Complete API implementation with real gRPC calls

The authentication mechanism is production-ready and follows Sidero Labs' official specification.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For questions and support, please open an issue on the [GitHub repository](https://github.com/panoramicdata/SideroLabs.Omni.Api/issues).

---

**Copyright © Panoramic Data Limited 2025**
