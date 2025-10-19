# SideroLabs Omni API Client

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/4f7bad7c811949dcb03008b3f799b12b)](https://app.codacy.com/gh/panoramicdata/SideroLabs.Omni.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![NuGet](https://img.shields.io/nuget/v/SideroLabs.Omni.Api.svg)](https://www.nuget.org/packages/SideroLabs.Omni.Api/)
[![NuGet](https://img.shields.io/nuget/dt/SideroLabs.Omni.Api.svg)](https://www.nuget.org/packages/SideroLabs.Omni.Api/)
![License](https://img.shields.io/github/license/panoramicdata/SideroLabs.Omni.Api.svg)
![GitHub repo size](https://img.shields.io/github/repo-size/panoramicdata/SideroLabs.Omni.Api)
![GitHub top language](https://img.shields.io/github/languages/top/panoramicdata/SideroLabs.Omni.Api)
![GitHub last commit](https://img.shields.io/github/last-commit/panoramicdata/SideroLabs.Omni.Api)
![GitHub Release](https://img.shields.io/github/v/release/panoramicdata/SideroLabs.Omni.Api)

A **.NET gRPC client library** for interacting with the **SideroLabs Omni Management API**.

This client provides strongly-typed C# interfaces for the Omni gRPC services, with built-in PGP-based authentication and enterprise-ready features.

> **🏗️ Inspired by**: This project is inspired by the official [SideroLabs Omni client](https://github.com/siderolabs/omni/tree/main/client) and follows the same patterns and authentication mechanisms.
>
> **📋 Proto Definitions**: The gRPC service definitions are based on the official [.proto files from the Omni project](https://github.com/siderolabs/omni/tree/main/client), which are the definitive source for the Omni API specification.

## Features

### 🔐 **Authentic gRPC Implementation**

- **Native gRPC client** - Direct implementation of Omni's actual gRPC services
- **PGP-based authentication** - Uses Omni's standard PGP signature authentication (compatible with [go-api-signature](https://github.com/siderolabs/go-api-signature))
- **Streaming support** - Real-time log streaming, manifest synchronization, and resource watching
- **Type-safe operations** - Generated from official Omni and COSI proto definitions

### 🎯 **Two Complete API Surfaces**

This library provides **full parity with omnictl** through two complementary gRPC services:

#### 1️⃣ **COSI State Service** (`/cosi.resource.State/*`)
**Resource operations** - Full CRUD for clusters, machines, users, and all Omni resources:

- `Resources.ListAsync<T>()` - List resources by type
- `Resources.GetAsync<T>()` - Get specific resources
- `Resources.CreateAsync<T>()` - Create new resources
- `Resources.UpdateAsync<T>()` - Update existing resources
- `Resources.DeleteAsync<T>()` - Delete resources
- `Resources.WatchAsync<T>()` - Watch resources for changes (streaming)

**High-level Operations:**
- `Clusters` - Cluster management operations
- `Users` - User and identity management
- `Templates` - Template rendering and synchronization

#### 2️⃣ **ManagementService** (`/management.ManagementService/*`)
**Administrative operations** - Configurations, service accounts, provisioning:

- `GetKubeConfigAsync()` / `GetTalosConfigAsync()` / `GetOmniConfigAsync()`
- `CreateServiceAccountAsync()` / `RenewServiceAccountAsync()` / `ListServiceAccountsAsync()`
- `StreamMachineLogsAsync()` / `StreamKubernetesSyncManifestsAsync()`
- `CreateSchematicAsync()` - Machine provisioning
- `ValidateConfigAsync()` / `KubernetesUpgradePreChecksAsync()`

> **🎉 Breakthrough Discovery**: After deep investigation of the omnictl source code, we discovered that it uses the COSI v1alpha1 State service (`/cosi.resource.State/*`), NOT the ResourceService (`/omni.resources.ResourceService/*`). This library now implements the correct service, providing full parity with omnictl! See [BREAKTHROUGH_COSI_STATE_SERVICE.md](BREAKTHROUGH_COSI_STATE_SERVICE.md) for details.

### 🛡️ **Enterprise Features**

- **Read-only mode** - Prevent accidental destructive operations
- **Comprehensive logging** - Structured logging with Microsoft.Extensions.Logging
- **Proper error handling** - gRPC status codes and meaningful error messages
- **Timeout management** - Configurable request timeouts
- **Connection pooling** - Efficient gRPC channel management

## ✅ **What This Library Provides**

This library provides **complete parity with omnictl** for both administrative and resource operations:

### COSI State Service (Resource Operations)
✅ **Full Resource CRUD** - Create, Read, Update, Delete for all Omni resources  
✅ **Resource Types** - Clusters, Machines, Users, Identities, Templates, and more  
✅ **Resource Watching** - Real-time change notifications via Watch API  
✅ **Label Queries** - Filter resources by labels and selectors  

### ManagementService (Administrative Operations)
✅ **Configuration Management** - kubeconfig, talosconfig, omniconfig retrieval  
✅ **Service Account Management** - Full lifecycle management  
✅ **Machine Provisioning** - Schematic creation and management  
✅ **Operational Tasks** - Log streaming, manifest sync, validation  
✅ **PGP Authentication** - Compatible with Omni's security model  
✅ **Type-Safe API** - Strongly-typed C# interfaces  

### Enterprise Features
✅ **Read-Only Mode** - Prevent accidental destructive operations  
✅ **Comprehensive Logging** - Structured logging throughout  
✅ **Proper Error Handling** - gRPC status codes and meaningful messages  

## 🎯 **Architecture & API Design**

```text
OmniClient
├── Resources (COSI State Service - /cosi.resource.State/*)
│   ├── ListAsync<T>()      - List resources
│   ├── GetAsync<T>()       - Get specific resource
│   ├── CreateAsync<T>()    - Create resource
│   ├── UpdateAsync<T>()    - Update resource
│   ├── DeleteAsync<T>()    - Delete resource
│   └── WatchAsync<T>()     - Watch for changes
│
├── Clusters (High-level cluster operations)
│   ├── GetStatusAsync()    - Get cluster status
│   ├── CreateAsync()       - Create cluster
│   ├── DeleteAsync()       - Delete cluster
│   ├── LockMachineAsync()  - Lock machine
│   └── UnlockMachineAsync()- Unlock machine
│
├── Users (User management)
│   ├── CreateAsync()       - Create user
│   ├── ListAsync()         - List users
│   ├── GetAsync()          - Get user
│   ├── DeleteAsync()       - Delete user
│   └── SetRoleAsync()      - Update user role
│
├── Templates (Template operations)
│   ├── LoadAsync()         - Load template
│   ├── RenderAsync()       - Render with variables
│   ├── SyncAsync()         - Sync to cluster
│   └── DiffAsync()         - Show differences
│
└── Management (Administrative operations - /management.ManagementService/*)
    ├── GetKubeConfigAsync()
    ├── GetTalosConfigAsync()
    ├── GetOmniConfigAsync()
    ├── CreateServiceAccountAsync()
    ├── StreamMachineLogsAsync()
    ├── CreateSchematicAsync()
    └── ... (16+ operations total)
```

**Key Insight**: This client uses the **same gRPC services as omnictl**, specifically:
- **COSI State API** (`/cosi.resource.State/*`) for resource operations
- **ManagementService API** (`/management.ManagementService/*`) for administrative tasks

See [BREAKTHROUGH_COSI_STATE_SERVICE.md](BREAKTHROUGH_COSI_STATE_SERVICE.md) for the technical discovery process.



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
    BaseUrl = new Uri("https://your-omni-instance.com"),
    Identity = "your-username",
    PgpPrivateKey = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n...",
    TimeoutSeconds = 30
};

// Create the client
using var client = new OmniClient(options);

// RESOURCE OPERATIONS (COSI State Service)

// List all clusters
await foreach (var cluster in client.Resources.ListAsync<Cluster>(cancellationToken: cancellationToken))
{
    Console.WriteLine($"Cluster: {cluster.Metadata.Id}");
}

// Get a specific cluster
var cluster = await client.Resources.GetAsync<Cluster>("my-cluster", "default", cancellationToken);

// Watch for cluster changes
await foreach (var evt in client.Resources.WatchAsync<Cluster>(cancellationToken: cancellationToken))
{
    Console.WriteLine($"{evt.Type}: {evt.Resource.Metadata.Id}");
}

// ADMINISTRATIVE OPERATIONS (ManagementService)

// Get kubeconfig for a cluster
var kubeconfig = await client.Management.GetKubeConfigAsync(
    serviceAccount: true,
    serviceAccountTtl: TimeSpan.FromHours(24),
    cancellationToken: cancellationToken);

// Stream machine logs
await foreach (var logEntry in client.Management.StreamMachineLogsAsync(
    "machine-id", 
    follow: true, 
    tailLines: 100, 
    cancellationToken))
{
    Console.WriteLine(Encoding.UTF8.GetString(logEntry));
}

// Create a service account
var publicKeyId = await client.Management.CreateServiceAccountAsync(
    armoredPgpPublicKey: pgpPublicKey,
    useUserRole: true,
    cancellationToken: cancellationToken);
```

### Authentication Setup

The client uses **PGP-based authentication** as required by Omni, implementing the same signature mechanism as the [official Go client](https://github.com/siderolabs/go-api-signature):

```csharp
var options = new OmniClientOptions
{
    BaseUrl = new Uri("https://omni.example.com"),
    Identity = "your-omni-username",
    PgpPrivateKey = File.ReadAllText("path/to/private-key.asc"),
    Logger = logger
};
```

Or load from a base64-encoded key file (compatible with Omni's key format):

```csharp
var options = new OmniClientOptions
{
    BaseUrl = new Uri("https://omni.example.com"),
    PgpKeyFilePath = "path/to/key-file.json" // Contains base64-encoded JSON
};
```

Or use an auth token (recommended for quick start):

```csharp
var options = new OmniClientOptions
{
    BaseUrl = new Uri("https://omni.example.com"),
    AuthToken = Environment.GetEnvironmentVariable("OMNI_AUTH_TOKEN")
};
```

See [QUICK_START_AUTH_TOKEN.md](QUICK_START_AUTH_TOKEN.md) for auth token setup.

### Read-Only Mode

Enable read-only mode for safe production use:

```csharp
var options = new OmniClientOptions
{
    BaseUrl = new Uri("https://omni.example.com"),
    Identity = "readonly-user", 
    PgpPrivateKey = "...",
    IsReadOnly = true // Prevents destructive operations
};
```

## Advanced Usage

### Resource Operations (COSI State Service)

```csharp
// List all clusters
var clusters = new List<Cluster>();
await foreach (var cluster in client.Resources.ListAsync<Cluster>(cancellationToken: cancellationToken))
{
    clusters.Add(cluster);
}

// Get a specific machine
var machine = await client.Resources.GetAsync<Machine>(
    id: "machine-uuid",
    @namespace: "default",
    cancellationToken: cancellationToken);

// Watch for resource changes
await foreach (var evt in client.Resources.WatchAsync<Cluster>(
    tailEvents: 10, // Replay last 10 events
    cancellationToken: cancellationToken))
{
    Console.WriteLine($"{evt.Type}: {evt.Resource.Metadata.Id}");
    if (evt.OldResource != null)
    {
        Console.WriteLine($"  Changed from version {evt.OldResource.Metadata.Version}");
    }
}

// Create a resource
var newCluster = new Cluster
{
    Metadata = new ResourceMetadata
    {
        Id = "my-new-cluster",
        Namespace = "default"
    },
    Spec = new ClusterSpec
    {
        // ... cluster configuration
    }
};
await client.Resources.CreateAsync(newCluster, cancellationToken);

// Update a resource
cluster.Spec.SomeProperty = "new-value";
await client.Resources.UpdateAsync(cluster, cancellationToken: cancellationToken);

// Delete a resource
await client.Resources.DeleteAsync<Cluster>(
    id: "cluster-to-delete",
    @namespace: "default",
    cancellationToken: cancellationToken);
```

### High-Level Cluster Operations

```csharp
// Get cluster status
var status = await client.Clusters.GetStatusAsync(
    clusterName: "production",
    waitTimeout: TimeSpan.FromMinutes(5),
    cancellationToken: cancellationToken);

// Lock a machine (prevent updates)
await client.Clusters.LockMachineAsync(
    machineId: "machine-uuid",
    clusterName: "production",
    cancellationToken: cancellationToken);

// Unlock a machine
await client.Clusters.UnlockMachineAsync(
    machineId: "machine-uuid",
    clusterName: "production",
    cancellationToken: cancellationToken);
```

### User Management

```csharp
// Create a new user
var (user, identity) = await client.Users.CreateAsync(
    email: "newuser@example.com",
    role: "Operator",
    cancellationToken: cancellationToken);

// List all users
var users = await client.Users.ListAsync(cancellationToken);
foreach (var userInfo in users)
{
    Console.WriteLine($"{userInfo.Email} - {userInfo.Role}");
}

// Update user role
await client.Users.SetRoleAsync(
    email: "user@example.com",
    role: "Admin",
    cancellationToken: cancellationToken);

// Delete a user
await client.Users.DeleteAsync(
    email: "olduser@example.com",
    cancellationToken: cancellationToken);
```

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
| `BaseUrl` | Omni gRPC endpoint URL | *Required* |
| `Identity` | Username for PGP authentication | *Optional* |
| `PgpPrivateKey` | PGP private key (armored format) | *Optional* |
| `PgpKeyFilePath` | Path to PGP key file | *Optional* |
| `AuthToken` | Authentication token (alternative to PGP) | *Optional* |
| `TimeoutSeconds` | Request timeout in seconds | `30` |
| `DefaultNamespace` | Default namespace for resources | `"default"` |
| `IsReadOnly` | Enable read-only mode | `false` |
| `Logger` | Microsoft.Extensions.Logging logger | `NullLogger` |

### Dependency Injection

```csharp
services.AddOmniClient(options =>
{
    options.BaseUrl = new Uri("https://omni.example.com");
    options.Identity = "api-user";
    options.PgpPrivateKey = Environment.GetEnvironmentVariable("OMNI_PGP_KEY");
    // OR use auth token
    options.AuthToken = Environment.GetEnvironmentVariable("OMNI_AUTH_TOKEN");
});

// Inject and use
public class MyService
{
    private readonly IOmniClient _omniClient;

    public MyService(IOmniClient omniClient)
    {
        _omniClient = omniClient;
    }

    public async Task<List<Cluster>> GetClustersAsync()
    {
        var clusters = new List<Cluster>();
        await foreach (var cluster in _omniClient.Resources.ListAsync<Cluster>())
        {
            clusters.Add(cluster);
        }
        return clusters;
    }
}
```

## Architecture

This client is built around the **actual Omni gRPC services** as used by omnictl:

```text
OmniClient
├── Resources (COSI State Service)
│   └── /cosi.resource.State/* endpoints
│       ├── Get, List, Create, Update, Delete
│       └── Watch (streaming)
│
├── Clusters (High-level operations on Resources)
├── Users (High-level operations on Resources)
├── Templates (High-level operations on Resources)
│
└── Management (ManagementService)
    └── /management.ManagementService/* endpoints
        ├── Configuration Operations (kubeconfig, talosconfig, omniconfig)
        ├── Service Account Management (create, renew, list, destroy)
        ├── Machine Operations (logs streaming)
        ├── Kubernetes Operations (upgrade checks, manifest sync)
        └── Provisioning (schematic creation)
```

**Key Discovery**: By analyzing the omnictl source code, we found that it uses:
- **COSI State Service** (`/cosi.resource.State/*`) for all resource operations
- **NOT** ResourceService (`/omni.resources.ResourceService/*`) which returns HTTP 405 on SaaS

This means we have **full parity with omnictl** for both resource and administrative operations!

See [BREAKTHROUGH_COSI_STATE_SERVICE.md](BREAKTHROUGH_COSI_STATE_SERVICE.md) for the complete technical discovery.

## Compatibility

This .NET client is designed to be fully compatible with:

- **Official Omni instances** - Works with any Omni deployment
- **Authentication system** - Uses the same PGP signature mechanism as the Go client
- **gRPC protocol** - Based on the same `.proto` definitions
- **API versioning** - Stays in sync with Omni's API evolution

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
    var result = await client.Management.GetKubeConfigAsync();
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

## Development & Publishing

### Building and Testing

```bash
# Build the solution
dotnet build

# Run unit tests
dotnet test

# Run integration tests (requires setup)
# See INTEGRATION_TEST_SETUP.md for configuration
dotnet test --filter "Category=Integration"

# Create NuGet package
dotnet pack --configuration Release
```

#### Integration Testing Setup

For integration testing against a real Omni instance:

1. See [INTEGRATION_TEST_SETUP.md](INTEGRATION_TEST_SETUP.md) for quick start
2. See [SideroLabs.Omni.Api.Tests/INTEGRATION_TESTING.md](SideroLabs.Omni.Api.Tests/INTEGRATION_TESTING.md) for detailed guide
3. Run helper script: `cd SideroLabs.Omni.Api.Tests && .\Create-ServiceAccount.ps1`

The setup includes:
- ✅ Service account creation with Admin role
- ✅ PGP key pair generation guidance
- ✅ appsettings.json template
- ✅ Security best practices
- ✅ CI/CD integration examples

### Automated Release Process

The project includes an automated release script that handles version management, testing, tagging, NuGet publishing with symbols, and GitHub release creation:

```bash
# Using PowerShell
.\Release.ps1

# Common options
.\Release.ps1 -Force              # Force release with uncommitted changes
.\Release.ps1 -SkipTests          # Skip unit tests (not recommended)
.\Release.ps1 -Publish            # Automatically publish to NuGet with symbols
.\Release.ps1 -Publish -SkipSymbols        # Publish without symbols
.\Release.ps1 -CreateGitHubRelease         # Create GitHub release with changelog
.\Release.ps1 -Publish -CreateGitHubRelease # Full release pipeline
```

**Setup for NuGet Publishing:**

1. Get your API key from [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys)
2. Create a file named `nuget_key.txt` in the solution root
3. Add your API key to the file (first line only)
4. Run `.\Release.ps1` - it will prompt whether to publish if tests pass

**Setup for GitHub Releases:**

1. Install GitHub CLI: `winget install GitHub.cli` or visit [cli.github.com](https://cli.github.com)
2. Authenticate: `gh auth login`
3. Run `.\Release.ps1 -CreateGitHubRelease` to create releases with automated changelogs

**The Release.ps1 script will:**

- ✅ Validate prerequisites and git repository status
- ✅ Get version using NerdBank GitVersioning (nbgv)
- ✅ Restore NuGet packages
- ✅ Build solution in Release mode
- ✅ Run unit tests (unless `-SkipTests` is specified)
- ✅ Create Git tag with current version
- ✅ Push tag to origin
- ✅ Optionally pack and publish NuGet package with symbols
- ✅ Generate changelog from git commits since last release
- ✅ Create GitHub release with automated release notes
- ✅ Provide next steps for manual tasks

**Version Management:**
The script uses NerdBank GitVersioning to automatically determine the version. It will:

- Try to use global `nbgv` tool
- Fall back to local `nbgv` tool
- Attempt to install `nbgv` if not found
- Use fallback versioning from `version.json` if `nbgv` is unavailable

**Example Usage:**

```bash
# Standard release process with symbols and GitHub release
.\Release.ps1 -CreateGitHubRelease

# Full automated release pipeline
.\Release.ps1 -Publish -CreateGitHubRelease

# Emergency release (skip tests and force with uncommitted changes)
.\Release.ps1 -Force -SkipTests -Publish
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

**Development Guidelines:**

- Follow the patterns established in the [official Omni client](https://github.com/siderolabs/omni/tree/main/client)
- Keep the `.proto` definitions in sync with the [upstream source](https://github.com/siderolabs/omni/tree/main/client)
- Maintain compatibility with the official authentication mechanisms

## License

MIT License - see LICENSE file for details.

## Links

- **🏠 Official Omni Project**: https://github.com/siderolabs/omni
- **📋 Official Proto Definitions**: https://github.com/siderolabs/omni/tree/main/client
- **🔐 Go API Signature Library**: https://github.com/siderolabs/go-api-signature
- **📖 Omni Documentation**: https://omni.siderolabs.com/
- **🏢 SideroLabs**: https://www.siderolabs.com/
- **🐧 Talos Linux**: https://talos.dev
