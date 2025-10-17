# Implementation Plan: Achieving 100% omnictl Coverage

# Full Coverage Plan — Progress Update

Summary of recent work:

- Implemented `TemplateOperations` (YAML load, render, sync). It renders resources and calls `IOmniResourceClient.ApplyAsync` via reflection to support generic resource types.
- Implemented `ClusterOperations` and wired it into `OmniClient` so `Clusters` is constructed with the current `Resources` client. Operations include: `GetStatusAsync`, `CreateAsync`, `DeleteAsync`, `LockMachineAsync`, `UnlockMachineAsync` (using the default namespace for now).
- Fixed multiple API signatures: added default `CancellationToken` parameters and corrected optional parameter ordering across interfaces (`IOmniResourceClient`, `ITemplateOperations`, `IClusterOperations`, `IUserManagement`, etc.).
- Removed placeholder classes and updated `OmniClient` to instantiate real implementations.
- Build now succeeds.

Remaining items / recommended next steps:

- Remove unused proto imports to silence warnings in `Protos/omni/resources/resources.proto` (non-blocking warnings).
- Add unit and integration tests for `TemplateOperations` and `ClusterOperations`.
- Replace reflection-based `ApplyAsync` invocation with a strongly-typed approach once resource type resolution is standardized (improve performance and clarity).
- Make resource namespace configurable (currently hard-coded to `"default"`).
- Add DI/IoC support for easier wiring of `ClusterOperations` and other services.

If you want I will update `FULL_COVERAGE_PLAN.md` further to include estimated timelines for the remaining items and create tickets or tasks in the repo. Proceed? 

## Executive Summary

This document outlines a comprehensive plan to achieve 100% coverage of omnictl functionality in the SideroLabs.Omni.Api .NET library. Based on the gap analysis, we need to add support for COSI resource operations, cluster templates, user management, and additional cluster management features.

**Current Coverage**: 43% (19/44 operations)  
**Target Coverage**: 100% (44/44 operations)  
**Estimated Total Effort**: 19-27 weeks (revised from 21-29 weeks)  
**Priority**: Implement in phases based on user demand and API availability

**Phase 1 Status**: ✅ **COMPLETED** (January 17, 2025) - See [PHASE_1_FINDINGS.md](PHASE_1_FINDINGS.md)

---

## Phase 1: Research & Discovery - ✅ COMPLETED

**Status**: ✅ COMPLETED  
**Duration**: 4 hours (originally estimated 2 weeks)  
**Completion Date**: January 17, 2025

### 1.1 COSI Protocol Investigation - ✅ COMPLETED
**Objective**: Understand the COSI (Common Operating System Interface) protocol used by Omni

**Tasks**:
- [x] Review Omni source code for COSI implementation
- [x] Identify COSI gRPC service definitions (.proto files)
- [x] Understand resource schema and types
- [x] Document COSI authentication mechanism
- [x] Test COSI endpoints with existing credentials (deferred to Phase 2)

**Deliverables**: ✅ ALL COMPLETED
- ✅ COSI protocol documentation
- ✅ List of available resource types (40+ identified)
- ✅ Proto files for COSI services (already in project!)
- ✅ Authentication requirements document

**Key Findings**:
- ✅ All proto files already present in `Protos/` directory
- ✅ ResourceService gRPC API fully defined
- ✅ Authentication uses existing PGP mechanism
- ✅ JSON encoding used for resource specs
- ✅ 40+ resource types identified across 8 categories
- ✅ Zero blockers identified

**Risk Status**: **Reduced from HIGH to LOW**

**See**: [PHASE_1_FINDINGS.md](PHASE_1_FINDINGS.md) for complete details

---

<!-- Phase 2 (COSI Resource Client Foundation) moved to appear after Phase 8: Client Integration -->

## Phase 3: Resource Type Implementation (4-5 weeks) - 🔄 IN PROGRESS (60% Complete)

### 3.1 Core Resource Types
**Objective**: Implement strongly-typed resource classes

**Priority Order**:
1. **Cluster** (High Priority) - ✅ COMPLETED
2. **Machine** (High Priority) - ✅ COMPLETED
3. **ClusterMachine** (High Priority) - ✅ COMPLETED
4. **MachineStatus** (Medium Priority) - ✅ COMPLETED
5. **ConfigPatch** (Medium Priority) - ⏸️ PENDING
6. **ExtensionsConfiguration** (Low Priority) - ⏸️ PENDING
7. **Other resource types** (As needed) - ⏸️ PENDING

**Tasks per Resource Type**:
- [x] Create initial resource model classes for `Cluster` and `Machine` (spec/status) ✅
- [x] Enhance status classes with proper types (`Condition`, `MachineHardware`, etc.) ✅
- [x] Create `ClusterMachine` resource type ✅
- [x] Create builder/factory methods (`ClusterBuilder`, `MachineBuilder`) ✅
- [x] Add resource type registry and initialization ✅
- [x] Implement validation logic (FluentValidation) ✅
- [x] Write unit tests for builders, validators, and resource types ✅
- [x] Add YAML serialization support tests ✅
- [ ] Write integration tests
- [ ] Add usage examples to documentation

**Files Created** (completed):
```
SideroLabs.Omni.Api/
├── Resources/
│   ├── Cluster.cs ✅ (with validation)
│   ├── ClusterSpec.cs ✅
│   ├── ClusterStatus.cs ✅ (enhanced)
│   ├── Machine.cs ✅ (with validation)
│   ├── MachineSpec.cs ✅
│   ├── MachineStatus.cs ✅ (enhanced)
│   ├── ClusterMachine.cs ✅ (with validation)
│   ├── ClusterMachineSpec.cs ✅
│   ├── ClusterMachineStatus.cs ✅
│   ├── Condition.cs ✅
│   ├── MachineHardware.cs ✅ (includes StorageDevice, NetworkInterface)
│   ├── ResourceTypes.cs ✅ (static initializer)
│   ├── ResourceTypeRegistry.cs ✅ (existing)
│   └── Validation/
│       ├── ClusterValidator.cs ✅ (new)
│       ├── MachineValidator.cs ✅ (new)
│       └── ClusterMachineValidator.cs ✅ (new)
├── Builders/
│   ├── ClusterBuilder.cs ✅
│   └── MachineBuilder.cs ✅
└── Tests/
    ├── Builders/
    │   ├── ClusterBuilderTests.cs ✅ (11 tests)
    │   └── MachineBuilderTests.cs ✅ (11 tests)
    ├── Resources/
    │   ├── ResourceTypeRegistryTests.cs ✅ (6 tests)
    │   ├── Validation/
    │   │   ├── ClusterValidatorTests.cs ✅ (8 tests, new)
    │   │   └── MachineValidatorTests.cs ✅ (7 tests, new)
    │   └── Serialization/
    │       └── ClusterSerializationTests.cs ✅ (5 tests, new)
```

**Test Coverage**: 48 unit tests passing
- Builder tests: 22 tests ✅
- Validation tests: 15 tests ✅  
- Serialization tests: 5 tests ✅
- Registry tests: 6 tests ✅

**Code Example**:
```csharp
public class Cluster : OmniResource<ClusterSpec, ClusterStatus>
{
    public override string Kind => "Cluster";
    public override string ApiVersion => "omni.sidero.dev/v1alpha1";
    
    public string ClusterId => Metadata.Name;
    public string KubernetesVersion => Spec.KubernetesVersion;
    public ClusterPhase Phase => Status.Phase;
}

public class ClusterSpec
{
    public string KubernetesVersion { get; set; } = "";
    public string TalosVersion { get; set; } = "";
    public NetworkConfig? Network { get; set; }
    public List<ControlPlaneConfig> ControlPlane { get; set; } = new();
    public List<WorkerConfig> Workers { get; set; } = new();
}

public class ClusterStatus
{
    public ClusterPhase Phase { get; set; }
    public bool Ready { get; set; }
    public List<Condition> Conditions { get; set; } = new();
    public DateTime? LastUpdateTime { get; set; }
}
```

**Acceptance Criteria** (per resource):
- [ ] Resource can be created programmatically
- [ ] Resource can be loaded from YAML
- [ ] Resource can be saved to YAML
- [ ] CRUD operations work via IOmniResourceClient
- [ ] All properties map correctly to proto
- [ ] Integration tests pass

---

## Phase 4: Resource Operations Implementation (2-3 weeks)

### 4.1 Implement omnictl get
**Objective**: Support all `omnictl get` functionality

**Features to Implement**:
- [ ] Get single resource by ID
- [ ] List resources by type
- [ ] Filter by namespace
- [ ] Filter by label selector
- [ ] ID regex matching
- [ ] Watch for changes (streaming)
- [ ] Output format options (JSON, YAML, Table)

**Interface**:
```csharp
public interface IOmniResourceClient
{
    Task<TResource> GetAsync<TResource>(
        string id,
        string? @namespace = "default",
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
    
    IAsyncEnumerable<TResource> ListAsync<TResource>(
        string? @namespace = "default",
        string? selector = null,
        string? idMatchRegexp = null,
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
    
    IAsyncEnumerable<ResourceEvent<TResource>> WatchAsync<TResource>(
        string? @namespace = "default",
        string? selector = null,
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
}
```

**Usage Example**:
```csharp
// Get single cluster
var cluster = await client.Resources.GetAsync<Cluster>("production");

// List all machines
await foreach (var machine in client.Resources.ListAsync<Machine>())
{
    Console.WriteLine($"Machine: {machine.Metadata.Name}");
}

// Watch for cluster changes
await foreach (var evt in client.Resources.WatchAsync<Cluster>())
{
    Console.WriteLine($"{evt.Type}: {evt.Resource.Metadata.Name}");
}

// List with label selector
var prodMachines = client.Resources.ListAsync<Machine>(
    selector: "environment=production");
```

### 4.2 Implement omnictl apply
**Objective**: Support declarative resource creation/updates

**Features to Implement**:
- [ ] Apply from object
- [ ] Apply from YAML string
- [ ] Apply from file
- [ ] Dry-run mode
- [ ] Server-side apply
- [ ] Field manager support

**Interface**:
```csharp
public interface IOmniResourceClient
{
    Task<TResource> ApplyAsync<TResource>(
        TResource resource,
        bool dryRun = false,
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
    
    Task<TResource> ApplyYamlAsync<TResource>(
        string yaml,
        bool dryRun = false,
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
    
    Task<TResource> ApplyFileAsync<TResource>(
        string filePath,
        bool dryRun = false,
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
}
```

**Usage Example**:
```csharp
// Apply from object
var cluster = new Cluster
{
    Metadata = new ResourceMetadata { Name = "production" },
    Spec = new ClusterSpec
    {
        KubernetesVersion = "v1.29.0",
        TalosVersion = "v1.7.0"
    }
};
await client.Resources.ApplyAsync(cluster);

// Apply from YAML
var yaml = File.ReadAllText("cluster.yaml");
await client.Resources.ApplyYamlAsync<Cluster>(yaml);

// Apply with dry-run
await client.Resources.ApplyFileAsync<Cluster>(
    "cluster.yaml", 
    dryRun: true);
```

### 4.3 Implement omnictl delete
**Objective**: Support resource deletion

**Features to Implement**:
- [ ] Delete by ID
- [ ] Delete by selector
- [ ] Delete all of type
- [ ] Namespace filtering
- [ ] Grace period support

**Interface**:
```csharp
public interface IOmniResourceClient
{
    Task DeleteAsync<TResource>(
        string id,
        string? @namespace = "default",
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
    
    Task DeleteManyAsync<TResource>(
        string? selector = null,
        string? @namespace = "default",
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
    
    Task DeleteAllAsync<TResource>(
        string? @namespace = "default",
        CancellationToken cancellationToken = default) 
        where TResource : IOmniResource;
}
```

**Usage Example**:
```csharp
// Delete single resource
await client.Resources.DeleteAsync<Cluster>("old-cluster");

// Delete with selector
await client.Resources.DeleteManyAsync<Machine>(
    selector: "environment=test");

// Delete all of type
await client.Resources.DeleteAllAsync<ConfigPatch>();
```

---

## Phase 5: Cluster Management Features (2-3 weeks)

### 5.1 Cluster Operations
**Objective**: Implement cluster-specific operations

**Features to Implement**:
- [ ] Cluster status monitoring
- [ ] Cluster creation helper
- [ ] Cluster deletion helper
- [ ] Machine lock/unlock
- [ ] Cluster health checks

**Files to Create**:
```
SideroLabs.Omni.Api/
├── Interfaces/
│   └── IClusterOperations.cs
└── Services/
    └── ClusterOperations.cs
```

**Interface**:
```csharp
public interface IClusterOperations
{
    Task<ClusterStatus> GetStatusAsync(
        string clusterName,
        TimeSpan? waitTimeout = null,
        CancellationToken cancellationToken = default);
    
    Task<Cluster> CreateAsync(
        ClusterBuilder builder,
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(
        string clusterName,
        bool force = false,
        CancellationToken cancellationToken = default);
    
    Task LockMachineAsync(
        string machineId,
        string clusterName,
        CancellationToken cancellationToken = default);
    
    Task UnlockMachineAsync(
        string machineId,
        string clusterName,
        CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
// Get cluster status with wait
var status = await client.Clusters.GetStatusAsync(
    "production",
    waitTimeout: TimeSpan.FromMinutes(5));

if (status.Ready)
{
    Console.WriteLine("Cluster is ready!");
}

// Lock a machine
await client.Clusters.LockMachineAsync(
    "machine-001",
    "production");
```

---

## Phase 6: Cluster Templates (3-4 weeks)

### 6.1 Template System
**Objective**: Implement cluster template support

**Features to Implement**:
- [ ] Template parsing (YAML with Go templates)
- [ ] Template variable substitution
- [ ] Template validation
- [ ] Template rendering to resources
- [ ] Template export from existing clusters
- [ ] Template sync (compare and update)
- [ ] Template diff display

**Files to Create**:
```
SideroLabs.Omni.Api/
├── Templates/
│   ├── IClusterTemplate.cs
│   ├── ClusterTemplate.cs
│   ├── TemplateParser.cs
│   ├── TemplateRenderer.cs
│   └── TemplateValidator.cs
└── Interfaces/
    └── ITemplateOperations.cs
```

**Interface**:
```csharp
public interface ITemplateOperations
{
    Task<ClusterTemplate> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default);
    
    Task<List<IOmniResource>> RenderAsync(
        ClusterTemplate template,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken = default);
    
    Task ValidateAsync(
        ClusterTemplate template,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<TemplateSyncResult> SyncAsync(
        ClusterTemplate template,
        Dictionary<string, object> variables,
        bool dryRun = false,
        CancellationToken cancellationToken = default);
    
    Task<ClusterTemplate> ExportAsync(
        string clusterName,
        CancellationToken cancellationToken = default);
    
    Task<List<ResourceDiff>> DiffAsync(
        ClusterTemplate template,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
// Load template
var template = await client.Templates.LoadAsync("cluster-template.yaml");

// Render with variables
var variables = new Dictionary<string, object>
{
    ["clusterName"] = "production",
    ["kubernetesVersion"] = "v1.29.0",
    ["controlPlaneCount"] = 3,
    ["workerCount"] = 5
};

var resources = await client.Templates.RenderAsync(template, variables);

// Sync template (apply all resources)
await foreach (var result in client.Templates.SyncAsync(
    template, 
    variables, 
    dryRun: false))
{
    Console.WriteLine($"{result.Action}: {result.ResourceType}/{result.Name}");
}

// Export existing cluster as template
var exportedTemplate = await client.Templates.ExportAsync("production");
await File.WriteAllTextAsync("exported.yaml", exportedTemplate.ToYaml());

// Show diff
var diffs = await client.Templates.DiffAsync(template, variables);
foreach (var diff in diffs)
{
    Console.WriteLine($"Resource: {diff.Name}");
    Console.WriteLine(diff.UnifiedDiff);
}
```

### 6.2 Template Features
**Advanced Features**:
- [ ] Template inheritance
- [ ] Template validation against schemas
- [ ] Template status monitoring
- [ ] Template versioning support

---

## Phase 7: User Management (1-2 weeks)

### 7.1 User Operations
**Objective**: Implement user management operations

**Research Required**:
- Identify user management API/service
- Understand authentication/authorization model
- Document user/role structure

**Features to Implement**:
- [ ] Create user
- [ ] Delete user
- [ ] List users
- [ ] Set user role
- [ ] Get user info

**Files to Create**:
```
SideroLabs.Omni.Api/
├── Interfaces/
│   └── IUserManagement.cs
├── Services/
│   └── UserManagementService.cs
└── Models/
    ├── User.cs
    ├── UserRole.cs
    └── UserInfo.cs
```

**Interface**:
```csharp
public interface IUserManagement
{
    Task<User> CreateAsync(
        string email,
        string? role = null,
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(
        string email,
        CancellationToken cancellationToken = default);
    
    Task<List<User>> ListAsync(
        CancellationToken cancellationToken = default);
    
    Task SetRoleAsync(
        string email,
        string role,
        CancellationToken cancellationToken = default);
    
    Task<UserInfo> GetInfoAsync(
        string email,
        CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
// Create user
await client.Users.CreateAsync(
    "engineer@company.com",
    role: "Operator");

// List users
var users = await client.Users.ListAsync();
foreach (var user in users)
{
    Console.WriteLine($"{user.Email}: {user.Role}");
}

// Change role
await client.Users.SetRoleAsync(
    "engineer@company.com",
    "Admin");

// Delete user
await client.Users.DeleteAsync("engineer@company.com");
```

---

## Phase 8: Enhanced OmniClient Integration (1 week)

### 8.1 Unified Client Interface
**Objective**: Integrate all new services into OmniClient

**Updates Required**:
```csharp
public interface IOmniClient : IDisposable
{
    // Existing
    IManagementService Management { get; }
    
    // New additions
    IOmniResourceClient Resources { get; }        // Phase 2-4
    IClusterOperations Clusters { get; }          // Phase 5
    ITemplateOperations Templates { get; }        // Phase 6
    IUserManagement Users { get; }                // Phase 7
    
    Uri BaseUrl { get; }
    bool UseTls { get; }
    bool IsReadOnly { get; }
    string? Identity { get; }
}
```

**Usage Example**:
```csharp
using var client = new OmniClient(options);

// Management operations (existing)
var kubeconfig = await client.Management.GetKubeConfigAsync();

// Resource operations (new)
var cluster = await client.Resources.GetAsync<Cluster>("production");

// Cluster operations (new)
await client.Clusters.LockMachineAsync("machine-001", "production");

// Template operations (new)
var template = await client.Templates.LoadAsync("cluster.yaml");
await foreach (var result in client.Templates.SyncAsync(template, vars))
{
    // Process sync results
}

// User management (new)
await client.Users.CreateAsync("user@example.com", "Operator");
```

---

## Phase 2: COSI Resource Client Foundation (2-3 weeks) - 🔄 IN PROGRESS (90% Complete)

### 2.1 Core COSI Client Infrastructure
**Objective**: Create foundation for COSI resource operations

**Tasks**:
- [x] Add core serialization and resource model infrastructure
- [x] Create `IOmniResourceClient` interface
- [x] Implement `ResourceSerializer` utilities (JSON/YAML)
- [x] Create `ResourceTypeRegistry` and registration mechanism
- [x] Implement `ResourceClientService` (initial COSI client implementation)
- [x] Add basic resource models: `Cluster`, `Machine` and related spec/status types
- [ ] Copy COSI proto files to `Protos/` and enable code generation
- [ ] Finalize `OmniResourceClient` full feature set (apply, watch, pagination)
- [ ] Add unit and integration tests

**Files created so far**:
```
SideroLabs.Omni.Api/
├── Interfaces/
│   └── IOmniResourceClient.cs
├── Services/
│   └── OmniResourceClient.cs
├── Models/
│   ├── ResourceMetadata.cs
│   ├── ResourceSpec.cs
│   └── ResourceStatus.cs
├── Serialization/
│   ├── IResourceSerializer.cs
│   └── YamlResourceSerializer.cs
└── Protos/
    └── cosi/
        └── resource.proto (if available)
```

**Interface (current)**:
```csharp
public interface IOmniResourceClient
{
    Task<TResource> GetAsync<TResource>(
        string id,
        string? @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;

    IAsyncEnumerable<TResource> ListAsync<TResource>(
        string? @namespace = "default",
        string? selector = null,
        string? idMatchRegexp = null,
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;

    IAsyncEnumerable<ResourceEvent<TResource>> WatchAsync<TResource>(
        string? @namespace = "default",
        string? selector = null,
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;

    Task<TResource> ApplyAsync<TResource>(
        TResource resource,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;

    Task<TResource> ApplyYamlAsync<TResource>(
        string yaml,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;

    Task<TResource> ApplyFileAsync<TResource>(
        string filePath,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;

    Task DeleteAsync<TResource>(
        string id,
        string? @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : IOmniResource;
}
```

**Acceptance Criteria (updated)**:
- [ ] Proto-based gRPC client generated and integrated
- [x] Serialize/deserialize resources (JSON/YAML) ✅
- [x] Basic resource models and registry in place ✅
- [ ] CRUD + watch operations fully implemented and tested

---

## Phase 9: Testing & Documentation (2-3 weeks)

### 9.1 Comprehensive Testing
**Test Coverage Goals**: 80%+

**Test Types**:
- [ ] Unit tests for all new classes
- [ ] Integration tests for COSI operations
- [ ] Integration tests for cluster operations
- [ ] Integration tests for templates
- [ ] Integration tests for user management
- [ ] End-to-end scenario tests
- [ ] Performance tests for streaming operations
- [ ] Load tests for concurrent operations

**Test Files to Create**:
```
SideroLabs.Omni.Api.Tests/
├── Resources/
│   ├── ResourceClientTests.cs
│   ├── ClusterResourceTests.cs
│   └── MachineResourceTests.cs
├── Operations/
│   ├── ClusterOperationsTests.cs
│   └── TemplateOperationsTests.cs
├── Integration/
│   ├── ResourceCrudTests.cs
│   ├── ClusterLifecycleTests.cs
│   └── TemplateWorkflowTests.cs
└── Performance/
    └── StreamingPerformanceTests.cs
```

### 9.2 Documentation Updates

**Documents to Create/Update**:
- [ ] Update README.md with new features
- [ ] Create RESOURCE_OPERATIONS.md guide
- [ ] Create CLUSTER_TEMPLATES.md guide
- [ ] Create USER_MANAGEMENT.md guide
- [ ] Update API reference documentation
- [ ] Create migration guide from omnictl
- [ ] Add troubleshooting guide
- [ ] Update OMNICTL_GAP_ANALYSIS.md to show 100%

**Example Documentation**:
- [ ] 20+ code examples for new features
- [ ] Architecture diagrams
- [ ] Best practices guide
- [ ] Performance tuning guide

### 9.3 Examples
**New Examples to Create**:
```
SideroLabs.Omni.Api.Examples/
├── Scenarios/
│   ├── ResourceOperationsExample.cs
│   ├── ClusterCreationExample.cs
│   ├── ClusterTemplateExample.cs
│   ├── MachineManagementExample.cs
│   └── UserManagementExample.cs
└── RealWorld/
    ├── MultiClusterDeployment.cs
    ├── AutomatedClusterUpgrade.cs
    └── DisasterRecovery.cs
```

---

## Phase 10: Performance & Optimization (1-2 weeks)

### 10.1 Performance Optimization

**Areas to Optimize**:
- [ ] Caching for resource listings
- [ ] Connection pooling for COSI clients
- [ ] Batch operations for multiple resources
- [ ] Pagination for large result sets
- [ ] Compression for large payloads
- [ ] Retry policies with exponential backoff

**Features to Add**:
```csharp
public class ResourceClientOptions
{
    public int MaxConcurrentConnections { get; set; } = 10;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    public int PageSize { get; set; } = 100;
    public bool EnableCompression { get; set; } = true;
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
}
```

### 10.2 Monitoring & Observability

**Features to Add**:
- [ ] Metrics collection (operation counts, latencies)
- [ ] Distributed tracing support
- [ ] Health check endpoints
- [ ] Resource usage monitoring

---

## Success Metrics

### Coverage Metrics
- [ ] 100% omnictl command coverage (44/44 operations)
- [ ] 80%+ test coverage for new code
- [ ] All resource types supported
- [ ] All template features supported

### Quality Metrics
- [ ] Zero critical bugs in production
- [ ] < 5 known issues at release
- [ ] Documentation completeness: 100%
- [ ] Example coverage: 90%+

### Performance Metrics
- [ ] Resource list operations < 500ms
- [ ] Single resource get < 100ms
- [ ] Template rendering < 2s for typical cluster
- [ ] Streaming operations < 50ms latency

### Adoption Metrics
- [ ] 10+ GitHub stars in first month
- [ ] 5+ production deployments
- [ ] 20+ NuGet downloads per week
- [ ] Positive community feedback

---

**Current Progress**: Phase 1 completed in 4 hours (vs 2 weeks estimated)

---

## Next Steps

This section explains the lightweight process to keep this plan accurate and actionable as work progresses. Make updates immediately after meaningful progress and perform regular reviews to keep estimates and priorities realistic.

- Update procedure (on every merged PR or completed task):
  - Edit `FULL_COVERAGE_PLAN.md` (or append to `PROGRESS_LOG.md` if present) and mark the task done with the completion date and a link to the PR/commit.
  - Update the overall progress numbers (e.g., operations completed / total) and recalculate the percent complete.
  - Adjust timelines and remaining effort estimates when scope or velocity changes.
  - Add a one-line rationale for any schedule or scope change.

- Progress entry format (recommended):
  - `YYYY-MM-DD — [Phase/Task] — Completed — PR #123 — 2h` (or similar concise log entry).

Keeping the plan live and precise improves transparency and helps with planning, prioritization, and stakeholder communication.

---

## Upcoming Milestones (suggested targets)

- Phase 2 complete (gRPC client + Apply/List/Get basic): target +2 weeks
- Phase 3 initial resource set (Cluster, Machine, ClusterMachine): target +4 weeks
- Phase 4 resource operations (get/apply/delete + watch): target +6 weeks

Adjust these dates during weekly reviews to reflect team velocity.
