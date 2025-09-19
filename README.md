# SideroLabs Omni API Client

A **.NET gRPC client library** for interacting with the **SideroLabs Omni Management API**.

This client provides strongly-typed C# interfaces for the Omni gRPC services, with built-in PGP-based authentication and enterprise-ready features.

## Features

### 🔐 **Authentic gRPC Implementation**
- **Native gRPC client** - Direct implementation of Omni's actual gRPC services
- **PGP-based authentication** - Uses Omni's standard PGP signature authentication
- **Streaming support** - Real-time log streaming and manifest synchronization
- **Type-safe operations** - Generated from official Omni proto definitions

### 🛠️ **Omni ManagementService Operations**
Based on the actual `management.proto` from the Omni project:

**Configuration Management:**
- `GetKubeconfigAsync()` - Retrieve kubeconfig for clusters
- `GetTalosconfigAsync()` - Retrieve talosconfig for Talos clusters  
- `GetOmniconfigAsync()` - Retrieve omnictl client configuration

**Service Account Management:**
- `CreateServiceAccountAsync()` - Create new service accounts
- `RenewServiceAccountAsync()` - Renew service account credentials
- `ListServiceAccountsAsync()` - List all service accounts
- `DestroyServiceAccountAsync()` - Delete service accounts

**Operational Tasks:**
- `StreamMachineLogsAsync()` - Stream logs from machines in real-time
- `ValidateConfigAsync()` - Validate configuration files
- `KubernetesUpgradePreChecksAsync()` - Check if K8s upgrade is safe
- `StreamKubernetesSyncManifestsAsync()` - Sync Kubernetes manifests
- `CreateSchematicAsync()` - Create schematics for machine provisioning

### 🛡️ **Enterprise Features**
- **Read-only mode** - Prevent accidental destructive operations
- **Comprehensive logging** - Structured logging with Microsoft.Extensions.Logging
- **Proper error handling** - gRPC status codes and meaningful error messages
- **Timeout management** - Configurable request timeouts
- **Connection pooling** - Efficient gRPC channel management

## Quick Start

### Installation

```bash
dotnet add package SideroLabs.Omni.Api
```

### Basic Usage

```csharp
using SideroLabs.Omni.Api;

// Configure the client
var options = new OmniClientOptions
{
    Endpoint = "https://your-omni-instance.com",
    Identity = "your-username",
    PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...",
    UseTls = true,
    ValidateCertificate = true
};

// Create the client
using var client = new OmniClient(options);

// Get kubeconfig for a cluster
var kubeconfig = await client.Management.GetKubeconfigAsync(
    serviceAccount: true,
    serviceAccountTtl: TimeSpan.FromHours(24),
    cancellationToken: cancellationToken);

// List service accounts
var serviceAccounts = await client.Management.ListServiceAccountsAsync(cancellationToken);

// Stream machine logs
await foreach (var logEntry in client.Management.StreamMachineLogsAsync(
    "machine-id", 
    follow: true, 
    tailLines: 100, 
    cancellationToken))
{
    Console.WriteLine(Encoding.UTF8.GetString(logEntry));
}
```

### Authentication Setup

The client uses **PGP-based authentication** as required by Omni:

```csharp
var options = new OmniClientOptions
{
    Endpoint = "https://omni.example.com",
    Identity = "your-omni-username",
    PgpPrivateKey = File.ReadAllText("path/to/private-key.asc"),
    Logger = logger
};
```

Or load from a base64-encoded key file:

```csharp
var options = new OmniClientOptions
{
    Endpoint = "https://omni.example.com",
    PgpKeyFilePath = "path/to/key-file.json" // Contains base64-encoded JSON
};
```

### Read-Only Mode

Enable read-only mode for safe production use:

```csharp
var options = new OmniClientOptions
{
    Endpoint = "https://omni.example.com",
    Identity = "readonly-user", 
    PgpPrivateKey = "...",
    IsReadOnly = true // Prevents destructive operations
};
```

## Advanced Usage

### Service Account Management

```csharp
// Create a service account
var publicKeyId = await client.Management.CreateServiceAccountAsync(
    armoredPgpPublicKey: pgpPublicKey,
    useUserRole: true,
    cancellationToken: cancellationToken);

// Renew service account
await client.Management.RenewServiceAccountAsync(
    name: "service-account-name",
    armoredPgpPublicKey: newPgpPublicKey,
    cancellationToken: cancellationToken);
```

### Kubernetes Operations

```csharp
// Check if Kubernetes upgrade is safe
var (canUpgrade, reason) = await client.Management.KubernetesUpgradePreChecksAsync(
    newVersion: "v1.29.0",
    cancellationToken: cancellationToken);

// Sync Kubernetes manifests
await foreach (var syncResult in client.Management.StreamKubernetesSyncManifestsAsync(
    dryRun: true,
    cancellationToken: cancellationToken))
{
    Console.WriteLine($"Synced: {syncResult.Path}");
}
```

### Machine Provisioning

```csharp
// Create a schematic for machine provisioning
var (schematicId, pxeUrl) = await client.Management.CreateSchematicAsync(
    extensions: new[] { "siderolabs/iscsi-tools", "siderolabs/util-linux-tools" },
    extraKernelArgs: new[] { "console=ttyS0" },
    metaValues: new Dictionary<uint, string> { { 0x0a, "rack-1" } },
    cancellationToken: cancellationToken);
```

## Configuration

### OmniClientOptions

| Property | Description | Default |
|----------|-------------|---------|
| `Endpoint` | Omni gRPC endpoint URL | *Required* |
| `Identity` | Username for PGP authentication | *Required* |
| `PgpPrivateKey` | PGP private key (armored format) | *Optional* |
| `PgpKeyFilePath` | Path to PGP key file | *Optional* |
| `TimeoutSeconds` | Request timeout in seconds | `30` |
| `UseTls` | Enable TLS encryption | `true` |
| `ValidateCertificate` | Validate server certificates | `true` |
| `IsReadOnly` | Enable read-only mode | `false` |
| `Logger` | Microsoft.Extensions.Logging logger | `NullLogger` |

### Dependency Injection

```csharp
services.AddOmniClient(options =>
{
    options.Endpoint = "https://omni.example.com";
    options.Identity = "api-user";
    options.PgpPrivateKey = Environment.GetEnvironmentVariable("OMNI_PGP_KEY");
});

// Inject and use
public class MyService
{
    private readonly IOmniClient _omniClient;

    public MyService(IOmniClient omniClient)
    {
        _omniClient = omniClient;
    }

    public async Task<byte[]> GetKubeconfigAsync()
    {
        return await _omniClient.Management.GetKubeconfigAsync();
    }
}
```

## Architecture

This client is built around the **actual Omni gRPC services**:

```
OmniClient
└── Management (IManagementService)
    ├── Configuration Operations (kubeconfig, talosconfig, omniconfig)
    ├── Service Account Management (create, renew, list, destroy)
    ├── Machine Operations (logs streaming)
    ├── Kubernetes Operations (upgrade checks, manifest sync)
    └── Provisioning (schematic creation)
```

**Note**: This client implements **only the gRPC services that Omni actually provides**. It does not include cluster CRUD operations, as these are handled through different mechanisms in the Omni architecture.

## Requirements

- **.NET 9.0** or later
- **gRPC support** (included)
- **Valid Omni instance** with gRPC endpoint
- **PGP key pair** for authentication

## Error Handling

The client provides comprehensive error handling:

```csharp
try
{
    var result = await client.Management.GetKubeconfigAsync();
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated)
{
    // Handle authentication errors
    logger.LogError("Authentication failed: {Message}", ex.Message);
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
{
    // Handle authorization errors
    logger.LogError("Permission denied: {Message}", ex.Message);
}
catch (ReadOnlyModeException ex)
{
    // Handle read-only mode violations
    logger.LogWarning("Operation blocked in read-only mode: {Operation}", ex.Operation);
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

MIT License - see LICENSE file for details.

## Links

- **Omni Documentation**: https://omni.siderolabs.com/
- **SideroLabs**: https://www.siderolabs.com/
- **Talos Linux**: https://talos.dev/
