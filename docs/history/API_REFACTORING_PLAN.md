# API Refactoring Plan - Resource-Specific Operations & Interface Segregation

**Date**: January 19, 2025  
**Status**: 📋 **PLANNING**  
**Estimated Effort**: 6-8 hours  
**Breaking Changes**: ✅ YES (Acceptable - library not yet in production use)

---

## 🎯 **Objectives**

1. **Resource-Specific Operations**: Create convenience interfaces for each resource type (similar to `IClusterOperations`)
2. **Interface Segregation**: Break up `IManagementService` into focused, single-responsibility interfaces
3. **Improved Discoverability**: Make API more intuitive with `client.Clusters`, `client.Machines`, `client.Users`, etc.
4. **Better Organization**: Group related operations together logically

---

## 📊 **Current State Analysis**

### Existing Resource Types (14 total)

| Resource Type | Current Access | Proposed Interface |
|--------------|----------------|-------------------|
| **Cluster** | ✅ `client.Clusters` | `IClusterOperations` (exists) |
| **Machine** | ❌ `client.Resources.ListAsync<Machine>()` | `IMachineOperations` |
| **ClusterMachine** | ❌ `client.Resources` | `IClusterMachineOperations` |
| **MachineSet** | ❌ `client.Resources` | `IMachineSetOperations` |
| **MachineSetNode** | ❌ `client.Resources` | `IMachineSetNodeOperations` |
| **ConfigPatch** | ❌ `client.Resources` | `IConfigPatchOperations` |
| **ExtensionsConfiguration** | ❌ `client.Resources` | `IExtensionsConfigurationOperations` |
| **User** | ✅ `client.Users` | `IUserManagement` (exists) |
| **Identity** | ❌ `client.Resources` | `IIdentityOperations` |
| **ControlPlane** | ❌ `client.Resources` | `IControlPlaneOperations` |
| **LoadBalancerConfig** | ❌ `client.Resources` | `ILoadBalancerOperations` |
| **TalosConfig** | ❌ `client.Resources` | `ITalosConfigOperations` |
| **KubernetesNode** | ❌ `client.Resources` | `IKubernetesNodeOperations` |
| **MachineClass** | ❌ `client.Resources` | `IMachineClassOperations` |

### Current IManagementService (Monolithic - 30+ methods)

**Configuration Methods** (9 overloads):
- `GetKubeConfigAsync()` (7 overloads)
- `GetTalosConfigAsync()` (3 overloads)  
- `GetOmniConfigAsync()` (1 method)

**Service Account Methods** (5 methods):
- `CreateServiceAccountAsync()` (3 overloads)
- `ListServiceAccountsAsync()`
- `RenewServiceAccountAsync()`
- `DestroyServiceAccountAsync()`

**Validation Methods** (2 methods):
- `ValidateConfigAsync()`
- `ValidateJsonSchemaAsync()`

**Kubernetes Methods** (3 methods):
- `KubernetesUpgradePreChecksAsync()`
- `StreamKubernetesSyncManifestsAsync()` (2 overloads)

**Machine Provisioning Methods** (7 methods):
- `CreateSchematicAsync()` (5 overloads)
- `MaintenanceUpgradeAsync()`
- `GetMachineJoinConfigAsync()`
- `CreateJoinTokenAsync()`
- `StreamMachineLogsAsync()` (3 overloads)

**Support & Audit Methods** (3 methods):
- `GetSupportBundleAsync()`
- `ReadAuditLogAsync()`
- `TearDownLockedClusterAsync()`

---

## 🏗️ **Proposed Architecture**

### Part 1: Resource-Specific Operations (14 new interfaces)

Each resource type gets its own operations interface with standard CRUD + conveniences:

```csharp
// Pattern for each resource type
public interface I{ResourceType}Operations
{
    // List
    IAsyncEnumerable<{ResourceType}> ListAsync(
        string? @namespace = "default",
        CancellationToken cancellationToken = default);
    
    // Get
    Task<{ResourceType}> GetAsync(
        string id,
        string? @namespace = "default",
        CancellationToken cancellationToken = default);
    
    // Create
    Task<{ResourceType}> CreateAsync(
        {ResourceType} resource,
        CancellationToken cancellationToken = default);
    
    // Update
    Task<{ResourceType}> UpdateAsync(
        {ResourceType} resource,
        CancellationToken cancellationToken = default);
    
    // Delete
    Task DeleteAsync(
        string id,
        string? @namespace = "default",
        CancellationToken cancellationToken = default);
    
    // Watch
    IAsyncEnumerable<ResourceEvent<{ResourceType}>> WatchAsync(
        string? @namespace = "default",
        CancellationToken cancellationToken = default);
    
    // Resource-specific methods as needed
}
```

### Part 2: Break Up IManagementService (7 new interfaces)

#### 1. IKubeConfigService
```csharp
public interface IKubeConfigService
{
    Task<string> GetKubeConfigAsync(CancellationToken cancellationToken = default);
    
    Task<string> GetKubeConfigAsync(
        bool serviceAccount,
        TimeSpan? serviceAccountTtl = null,
        string? serviceAccountUser = null,
        string[]? serviceAccountGroups = null,
        string? grantType = null,
        bool breakGlass = false,
        CancellationToken cancellationToken = default);
}
```

#### 2. ITalosConfigService
```csharp
public interface ITalosConfigService
{
    Task<string> GetTalosConfigAsync(
        bool raw = false,
        bool breakGlass = false,
        CancellationToken cancellationToken = default);
}
```

#### 3. IOmniConfigService
```csharp
public interface IOmniConfigService
{
    Task<string> GetOmniConfigAsync(CancellationToken cancellationToken = default);
}
```

#### 4. IServiceAccountService
```csharp
public interface IServiceAccountService
{
    Task<string> CreateServiceAccountAsync(
        string armoredPgpPublicKey,
        bool useUserRole = false,
        string? role = null,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<ServiceAccountInfo>> ListServiceAccountsAsync(
        CancellationToken cancellationToken = default);
    
    Task<string> RenewServiceAccountAsync(
        string name,
        string armoredPgpPublicKey,
        CancellationToken cancellationToken = default);
    
    Task DestroyServiceAccountAsync(
        string name,
        CancellationToken cancellationToken = default);
}
```

#### 5. IValidationService
```csharp
public interface IValidationService
{
    Task ValidateConfigAsync(
        string config,
        CancellationToken cancellationToken = default);
    
    Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
        string data,
        string schema,
        CancellationToken cancellationToken = default);
}
```

#### 6. IKubernetesService
```csharp
public interface IKubernetesService
{
    Task<KubernetesUpgradePreCheckResult> UpgradePreChecksAsync(
        string newVersion,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<KubernetesSyncResult> StreamSyncManifestsAsync(
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}
```

#### 7. ISchematicService
```csharp
public interface ISchematicService
{
    Task<SchematicResult> CreateSchematicAsync(
        string[]? extensions = null,
        string[]? extraKernelArgs = null,
        Dictionary<uint, string>? metaValues = null,
        string? talosVersion = null,
        string? mediaId = null,
        bool secureBoot = false,
        SiderolinkGrpcTunnelMode siderolinkGrpcTunnelMode = SiderolinkGrpcTunnelMode.Auto,
        string? joinToken = null,
        CancellationToken cancellationToken = default);
}
```

#### 8. IMachineService (for machine-specific management operations)
```csharp
public interface IMachineService
{
    IAsyncEnumerable<byte[]> StreamLogsAsync(
        string machineId,
        bool follow = false,
        int tailLines = 0,
        CancellationToken cancellationToken = default);
    
    Task MaintenanceUpgradeAsync(
        string machineId,
        string version,
        CancellationToken cancellationToken = default);
    
    Task<MachineJoinConfig> GetJoinConfigAsync(
        bool useGrpcTunnel,
        string joinToken,
        CancellationToken cancellationToken = default);
    
    Task<string> CreateJoinTokenAsync(
        string name,
        DateTimeOffset expirationTime,
        CancellationToken cancellationToken = default);
}
```

#### 9. ISupportService
```csharp
public interface ISupportService
{
    IAsyncEnumerable<SupportBundleProgress> GetSupportBundleAsync(
        string cluster,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<byte[]> ReadAuditLogAsync(
        string startDate,
        string endDate,
        CancellationToken cancellationToken = default);
    
    Task TearDownLockedClusterAsync(
        string clusterId,
        CancellationToken cancellationToken = default);
}
```

### Part 3: Updated IOmniClient

```csharp
public interface IOmniClient : IDisposable
{
    // Low-level generic resource access
    IOmniResourceClient Resources { get; }
    
    // === Resource-Specific Operations (14 properties) ===
    
    // Core Infrastructure
    IClusterOperations Clusters { get; }
    IMachineOperations Machines { get; }
    IClusterMachineOperations ClusterMachines { get; }
    IMachineSetOperations MachineSets { get; }
    IMachineSetNodeOperations MachineSetNodes { get; }
    IMachineClassOperations MachineClasses { get; }
    
    // Configuration
    IConfigPatchOperations ConfigPatches { get; }
    IExtensionsConfigurationOperations ExtensionsConfigurations { get; }
    ITalosConfigOperations TalosConfigs { get; }
    ILoadBalancerOperations LoadBalancers { get; }
    
    // Control Plane & Kubernetes
    IControlPlaneOperations ControlPlanes { get; }
    IKubernetesNodeOperations KubernetesNodes { get; }
    
    // Auth
    IUserManagement Users { get; }
    IIdentityOperations Identities { get; }
    
    // === Management Services (9 properties) ===
    
    // Configuration Services
    IKubeConfigService KubeConfig { get; }
    ITalosConfigService TalosConfig { get; }
    IOmniConfigService OmniConfig { get; }
    
    // Management Services
    IServiceAccountService ServiceAccounts { get; }
    IValidationService Validation { get; }
    IKubernetesService Kubernetes { get; }
    ISchematicService Schematics { get; }
    IMachineService MachineManagement { get; }
    ISupportService Support { get; }
    
    // === Deprecated (Backward Compatibility - Optional) ===
    [Obsolete("Use specific services like KubeConfig, ServiceAccounts, etc.")]
    IManagementService Management { get; }
    
    // Client Properties
    Uri BaseUrl { get; }
    bool UseTls { get; }
    bool IsReadOnly { get; }
    string? Identity { get; }
}
```

---

## 📝 **Implementation Plan**

### Phase 1: Create Resource Operations Interfaces (4-5 hours)

**Step 1.1**: Create Generic Resource Operations Template
- Create `ResourceOperationsBase<T>` abstract class
- Implements standard CRUD + Watch
- Delegates to `IOmniResourceClient`

**Step 1.2**: Create 13 New Resource Operation Interfaces
Files to create:
1. `Interfaces/IMachineOperations.cs`
2. `Interfaces/IClusterMachineOperations.cs`
3. `Interfaces/IMachineSetOperations.cs`
4. `Interfaces/IMachineSetNodeOperations.cs`
5. `Interfaces/IConfigPatchOperations.cs`
6. `Interfaces/IExtensionsConfigurationOperations.cs`
7. `Interfaces/IIdentityOperations.cs`
8. `Interfaces/IControlPlaneOperations.cs`
9. `Interfaces/ILoadBalancerOperations.cs`
10. `Interfaces/ITalosConfigOperations.cs`
11. `Interfaces/IKubernetesNodeOperations.cs`
12. `Interfaces/IMachineClassOperations.cs`
13. `Interfaces/ITemplateOperations.cs` (update existing)

**Step 1.3**: Create Implementations
Files to create (13 files in `Services/`):
- `MachineOperations.cs`
- `ClusterMachineOperations.cs`
- ... (one for each interface)

### Phase 2: Break Up IManagementService (2-3 hours)

**Step 2.1**: Create New Service Interfaces
Files to create (9 files in `Interfaces/`):
1. `IKubeConfigService.cs`
2. `ITalosConfigService.cs`
3. `IOmniConfigService.cs`
4. `IServiceAccountService.cs`
5. `IValidationService.cs`
6. `IKubernetesService.cs`
7. `ISchematicService.cs`
8. `IMachineService.cs`
9. `ISupportService.cs`

**Step 2.2**: Create Implementations
Files to create (9 files in `Services/`):
- Split existing `OmniManagementService.cs` into 9 focused services
- Each service implements one interface
- All delegate to the underlying gRPC client

**Step 2.3**: Mark IManagementService as Obsolete
- Add `[Obsolete]` attribute with migration message
- Keep implementation for backward compatibility (optional)

### Phase 3: Update IOmniClient & OmniClient (1-2 hours)

**Step 3.1**: Update IOmniClient Interface
- Add 14 resource operation properties
- Add 9 management service properties
- Mark `Management` property as obsolete

**Step 3.2**: Update OmniClient Implementation
- Add lazy initialization for all 23 new properties
- Wire up dependencies correctly
- Update constructor

**Step 3.3**: Update Dependency Injection
- Update `ServiceCollectionExtensions.cs`
- Register all new services

### Phase 4: Update Tests & Examples (1-2 hours)

**Step 4.1**: Update Integration Tests
- Update all `client.Management.X` calls to use new services
- Add tests for new resource operations

**Step 4.2**: Update Examples
- Update all example code
- Show new convenience APIs

**Step 4.3**: Update Documentation
- Update README.md
- Update API documentation
- Create migration guide

---

## ⚡ **Quick Win: MVP Implementation**

For faster delivery, implement in priority order:

### MVP Phase 1: Most Used Resources (2-3 hours)
1. ✅ `IMachineOperations` (already have ClusterOperations as template)
2. ✅ `IConfigPatchOperations`
3. ✅ `IExtensionsConfigurationOperations`

### MVP Phase 2: Management Service Split (1-2 hours)
1. ✅ `IKubeConfigService`
2. ✅ `ITalosConfigService`
3. ✅ `IServiceAccountService`

### MVP Phase 3: Remaining Resources (2-3 hours)
- Implement all remaining resource operations

### MVP Phase 4: Remaining Services (1-2 hours)
- Implement all remaining management services

---

## 🔄 **Migration Guide for Users**

### Before (Old API):
```csharp
// Resources - verbose
await foreach (var machine in client.Resources.ListAsync<Machine>())
{
    // ...
}

// Management - monolithic
var kubeconfig = await client.Management.GetKubeConfigAsync(...);
var accounts = await client.Management.ListServiceAccountsAsync();
```

### After (New API):
```csharp
// Resources - convenient
await foreach (var machine in client.Machines.ListAsync())
{
    // ...
}

// Management - focused
var kubeconfig = await client.KubeConfig.GetKubeConfigAsync(...);
var accounts = await client.ServiceAccounts.ListServiceAccountsAsync();
```

### Backward Compatibility (Optional):
```csharp
// Keep Management property as obsolete wrapper
[Obsolete("Use specific services like KubeConfig, ServiceAccounts, etc.")]
public IManagementService Management =>
    _management ??= new ManagementServiceAdapter(
        KubeConfig, TalosConfig, OmniConfig, ServiceAccounts, ...);
```

---

## ✅ **Benefits**

1. **Better Discoverability**: IntelliSense shows `client.Machines` instead of requiring knowledge of `Resources.ListAsync<Machine>()`
2. **Type Safety**: Each operation interface is strongly typed
3. **Single Responsibility**: Each service has one clear purpose
4. **Easier Testing**: Mock individual services instead of monolithic interface
5. **Better Documentation**: Each service can have focused documentation
6. **Consistent API**: All resources follow same pattern

---

## ⚠️ **Breaking Changes**

### Breaking (Acceptable):
- `IManagementService` methods moved to focused services
- Users must update: `client.Management.X()` → `client.XService.X()`

### Non-Breaking:
- `client.Resources` still works
- Can keep `Management` property as obsolete wrapper

### Migration Effort:
- **Find/Replace**: ~10-20 occurrences in typical codebase
- **Time**: 15-30 minutes for users
- **Risk**: Low (compile-time errors make issues obvious)

---

## 📋 **Implementation Checklist**

### Phase 1: Resource Operations
- [ ] Create `ResourceOperationsBase<T>` abstract class
- [ ] Create 13 resource operation interfaces
- [ ] Create 13 resource operation implementations
- [ ] Add unit tests for each

### Phase 2: Management Service Split
- [ ] Create 9 new service interfaces
- [ ] Create 9 new service implementations
- [ ] Split `OmniManagementService` logic
- [ ] Add unit tests for each

### Phase 3: Client Updates
- [ ] Update `IOmniClient` interface
- [ ] Update `OmniClient` implementation
- [ ] Update DI registration
- [ ] Mark old APIs as obsolete

### Phase 4: Documentation & Tests
- [ ] Update all integration tests
- [ ] Update all examples
- [ ] Update README.md
- [ ] Create migration guide
- [ ] Update API documentation

---

## 🚀 **Next Steps**

1. **Review & Approve**: Get approval for breaking changes
2. **Implement MVP**: Start with most-used operations
3. **Test**: Comprehensive testing of new APIs
4. **Document**: Update all documentation
5. **Release**: Create new major version (2.0.0)

---

**Estimated Total Time**: 8-10 hours for full implementation  
**MVP Time**: 4-5 hours for core functionality  
**Risk Level**: Low (breaking changes acceptable, compile-time safe)  
**Benefit**: High (much better developer experience)

