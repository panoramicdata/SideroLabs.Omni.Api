# Full CRUD Integration Test Coverage & Code Coverage Plan

**Date**: January 18, 2025  
**Objective**: Achieve **100% CRUD integration test coverage** and **90%+ code coverage**  
**Status**: 🚀 **READY TO EXECUTE**

---

## 📊 **Current State Analysis**

### ✅ What We Have

**Production Code**: ✅ **100% Complete**
- ManagementService: 19/19 methods ✅
- COSI State Service: 9/9 methods ✅
- Resource Types: 14 types ✅
- Cluster Operations: 5/5 methods ✅
- Template Operations: 7/7 methods ✅
- User Management: 5/5 methods ✅

**Integration Tests**: ⚠️ **~60% Coverage**
- ManagementService: 18/19 tests ✅
- COSI State: 10/10 tests ✅
- Cluster Operations: 0/5 tests ❌
- Template Operations: 0/7 tests ❌
- User Management: 0/5 tests ❌
- Resource CRUD: 0/11 tests ❌

**Total Integration Tests**:
- Current: 28 tests
- Needed: ~80 tests
- Gap: **52 tests missing**

### 📈 **Coverage Goals**

| Component | Current | Target | Gap |
|-----------|---------|--------|-----|
| **Management Service** | 95% | 100% | 5% |
| **COSI State Service** | 100% | 100% | 0% |
| **Resource Operations** | 20% | 100% | 80% |
| **Cluster Operations** | 0% | 100% | 100% |
| **Template Operations** | 0% | 100% | 100% |
| **User Management** | 0% | 100% | 100% |
| **Overall Code Coverage** | ~65% | 90%+ | 25% |

---

## 🎯 **Test Implementation Plan**

### Phase 1: Resource CRUD Tests (Priority: HIGH)
**Duration**: 4-6 hours  
**Tests to Add**: 33 tests

#### 1.1 Core Resource CRUD (15 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/ResourceCrudTests.cs

[Collection("Integration")]
public class ResourceCrudTests : TestBase
{
    // Cluster CRUD (5 tests)
    [Fact] public Task Cluster_Create_Success()
    [Fact] public Task Cluster_Get_ReturnsCluster()
    [Fact] public Task Cluster_Update_ModifiesCluster()
    [Fact] public Task Cluster_Delete_RemovesCluster()
    [Fact] public Task Cluster_List_ReturnsMultipleClusters()
    
    // Machine CRUD (5 tests)
    [Fact] public Task Machine_Create_Success()
    [Fact] public Task Machine_Get_ReturnsMachine()
    [Fact] public Task Machine_Update_ModifiesMachine()
    [Fact] public Task Machine_Delete_RemovesMachine()
    [Fact] public Task Machine_List_ReturnsMultipleMachines()
    
    // ConfigPatch CRUD (5 tests)
    [Fact] public Task ConfigPatch_Create_Success()
    [Fact] public Task ConfigPatch_Get_ReturnsConfigPatch()
    [Fact] public Task ConfigPatch_Update_ModifiesConfigPatch()
    [Fact] public Task ConfigPatch_Delete_RemovesConfigPatch()
    [Fact] public Task ConfigPatch_List_ReturnsMultipleConfigPatches()
}
```

#### 1.2 Resource Watch & Streaming (6 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/ResourceWatchTests.cs

[Collection("Integration")]
public class ResourceWatchTests : TestBase
{
    [Fact] public Task Watch_Cluster_ReceivesCreatedEvent()
    [Fact] public Task Watch_Cluster_ReceivesUpdatedEvent()
    [Fact] public Task Watch_Cluster_ReceivesDeletedEvent()
    [Fact] public Task Watch_WithSelector_FiltersEvents()
    [Fact] public Task Watch_TailEvents_ReplaysHistory()
    [Fact] public Task Watch_Cancellation_StopsStreaming()
}
```

#### 1.3 Resource Apply Operations (6 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/ResourceApplyTests.cs

[Collection("Integration")]
public class ResourceApplyTests : TestBase
{
    [Fact] public Task Apply_NewResource_CreatesResource()
    [Fact] public Task Apply_ExistingResource_UpdatesResource()
    [Fact] public Task Apply_WithDryRun_DoesNotModify()
    [Fact] public Task ApplyYaml_ValidYaml_CreatesResource()
    [Fact] public Task ApplyFile_ValidFile_CreatesResource()
    [Fact] public Task Apply_OptimisticLocking_HandlesConflicts()
}
```

#### 1.4 Bulk Delete Operations (6 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/ResourceBulkDeleteTests.cs

[Collection("Integration")]
public class ResourceBulkDeleteTests : TestBase
{
    [Fact] public Task DeleteMany_WithSelector_DeletesMatchingResources()
    [Fact] public Task DeleteMany_ReturnsCorrectCount()
    [Fact] public Task DeleteMany_ContinuesOnError()
    [Fact] public Task DeleteAll_RemovesAllResources()
    [Fact] public Task DeleteAll_InNamespace_OnlyDeletesInNamespace()
    [Fact] public Task DeleteMany_NoMatches_ReturnsZero()
}
```

---

### Phase 2: User Management Tests (Priority: HIGH)
**Duration**: 2-3 hours  
**Tests to Add**: 12 tests

#### 2.1 User Management Service Tests (6 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Services/UserManagementTests.cs

[Collection("Integration")]
public class UserManagementTests : TestBase
{
    [Fact] public Task CreateUser_ValidEmail_CreatesUserAndIdentity()
    [Fact] public Task ListUsers_ReturnsAllUsers()
    [Fact] public Task ListUsers_ExcludesServiceAccounts()
    [Fact] public Task GetUser_ByEmail_ReturnsUser()
    [Fact] public Task SetRole_UpdatesUserRole()
    [Fact] public Task DeleteUser_RemovesUserAndIdentity()
}
```

#### 2.2 User/Identity Resource Tests (6 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/UserResourceTests.cs

[Collection("Integration")]
public class UserResourceTests : TestBase
{
    [Fact] public Task User_CreateWithBuilder_Success()
    [Fact] public Task User_Validation_EnforcesRoles()
    [Fact] public Task Identity_CreateWithBuilder_Success()
    [Fact] public Task Identity_Validation_RequiresEmail()
    [Fact] public Task Identity_LinksToUser_Correctly()
    [Fact] public Task User_Watch_ReceivesRoleChanges()
}
```

---

### Phase 3: Cluster Operations Tests (Priority: MEDIUM)
**Duration**: 3-4 hours  
**Tests to Add**: 10 tests

#### 3.1 Cluster Status & Lifecycle (5 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Operations/ClusterOperationsTests.cs

[Collection("Integration")]
public class ClusterOperationsTests : TestBase
{
    [Fact] public Task GetStatus_ReturnsClusterStatus()
    [Fact] public Task GetStatus_WithWait_WaitsForReady()
    [Fact] public Task CreateCluster_FromBuilder_CreatesCluster()
    [Fact] public Task DeleteCluster_RemovesCluster()
    [Fact] public Task DeleteCluster_Force_BypassesChecks()
}
```

#### 3.2 Machine Lock/Unlock (5 tests)

```csharp
[Fact] public Task LockMachine_LocksToCluster()
[Fact] public Task UnlockMachine_ReleasesLock()
[Fact] public Task LockMachine_AlreadyLocked_Throws()
[Fact] public Task UnlockMachine_NotLocked_Throws()
[Fact] public Task LockUnlock_FullCycle_Success()
```

---

### Phase 4: Template Operations Tests (Priority: MEDIUM)
**Duration**: 4-5 hours  
**Tests to Add**: 14 tests

#### 4.1 Template Loading & Rendering (7 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Operations/TemplateOperationsTests.cs

[Collection("Integration")]
public class TemplateOperationsTests : TestBase
{
    [Fact] public Task LoadTemplate_ValidYaml_LoadsTemplate()
    [Fact] public Task LoadTemplate_InvalidYaml_Throws()
    [Fact] public Task RenderTemplate_WithVariables_RendersResources()
    [Fact] public Task RenderTemplate_MissingVariable_Throws()
    [Fact] public Task ValidateTemplate_ValidTemplate_Succeeds()
    [Fact] public Task ValidateTemplate_InvalidTemplate_ReturnsErrors()
    [Fact] public Task ExportTemplate_FromCluster_CreatesTemplate()
}
```

#### 4.2 Template Sync & Diff (7 tests)

```csharp
[Fact] public Task SyncTemplate_CreatesNewResources()
[Fact] public Task SyncTemplate_UpdatesExistingResources()
[Fact] public Task SyncTemplate_DryRun_DoesNotApply()
[Fact] public Task SyncTemplate_ReturnsResults()
[Fact] public Task DiffTemplate_ShowsChanges()
[Fact] public Task DiffTemplate_NoChanges_ReturnsEmpty()
[Fact] public Task TemplateStatus_ReturnsCurrentState()
```

---

### Phase 5: Advanced Resource Features (Priority: LOW)
**Duration**: 2-3 hours  
**Tests to Add**: 13 tests

#### 5.1 Advanced Filtering (6 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/ResourceFilteringTests.cs

[Collection("Integration")]
public class ResourceFilteringTests : TestBase
{
    [Fact] public Task List_WithSelector_FiltersResults()
    [Fact] public Task List_WithRegex_MatchesIds()
    [Fact] public Task List_WithPagination_ReturnsCorrectPage()
    [Fact] public Task List_WithSorting_ReturnsSortedResults()
    [Fact] public Task List_WithSearch_FindsMatches()
    [Fact] public Task List_CombinedFilters_AppliesAll()
}
```

#### 5.2 Infrastructure Resources (7 tests)

```csharp
// File: SideroLabs.Omni.Api.Tests/Resources/InfrastructureResourceTests.cs

[Collection("Integration")]
public class InfrastructureResourceTests : TestBase
{
    [Fact] public Task MachineSet_CRUD_Success()
    [Fact] public Task ControlPlane_CRUD_Success()
    [Fact] public Task LoadBalancerConfig_CRUD_Success()
    [Fact] public Task TalosConfig_CRUD_Success()
    [Fact] public Task KubernetesNode_CRUD_Success()
    [Fact] public Task MachineClass_CRUD_Success()
    [Fact] public Task InfrastructureResources_Watch_ReceivesEvents()
}
```

---

## 📋 **Execution Plan**

### Week 1: Resource CRUD & User Management
**Days 1-2**: Resource CRUD Tests (33 tests)
- Day 1 Morning: Core CRUD (15 tests)
- Day 1 Afternoon: Watch & Streaming (6 tests)
- Day 2 Morning: Apply Operations (6 tests)
- Day 2 Afternoon: Bulk Delete (6 tests)

**Days 3-4**: User Management Tests (12 tests)
- Day 3: User Management Service (6 tests)
- Day 4: User/Identity Resources (6 tests)

**Day 5**: Review & Bug Fixes

### Week 2: Cluster & Template Operations
**Days 1-2**: Cluster Operations Tests (10 tests)
- Day 1: Status & Lifecycle (5 tests)
- Day 2: Machine Lock/Unlock (5 tests)

**Days 3-4**: Template Operations Tests (14 tests)
- Day 3: Loading & Rendering (7 tests)
- Day 4: Sync & Diff (7 tests)

**Day 5**: Review & Bug Fixes

### Week 3: Advanced Features & Polish
**Days 1-2**: Advanced Resource Features (13 tests)
- Day 1: Advanced Filtering (6 tests)
- Day 2: Infrastructure Resources (7 tests)

**Days 3-4**: Code Coverage Improvements
- Fill gaps identified by coverage report
- Add edge case tests
- Performance tests

**Day 5**: Final Review & Documentation

---

## 🎯 **Success Metrics**

### Coverage Targets

| Metric | Current | Target | Status |
|--------|---------|--------|--------|
| **Integration Tests** | 28 | 110+ | ⏳ 0/82 remaining |
| **Code Coverage** | ~65% | 90%+ | ⏳ 25% to go |
| **ManagementService** | 95% | 100% | ⏳ 5% to go |
| **COSI State** | 100% | 100% | ✅ DONE |
| **Resources** | 20% | 100% | ⏳ 80% to go |
| **Operations** | 0% | 100% | ⏳ 100% to go |

### Quality Gates

✅ **Must Pass Before Merge**:
- All existing tests pass
- All new tests pass
- Code coverage ≥ 90%
- Zero critical bugs
- Zero compiler warnings
- All public APIs documented

⚠️ **Should Pass (Optional)**:
- Performance benchmarks
- Load testing
- Memory profiling
- Zero test flakiness

---

## 🛠️ **Implementation Guidelines**

### Test Structure Template

```csharp
[Collection("Integration")]
[Trait("Category", "Integration")]
public class [FeatureName]Tests : TestBase
{
    [Fact]
    public async Task [Operation]_[Scenario]_[ExpectedResult]()
    {
        // Skip if not configured
        if (!ShouldRunIntegrationTests())
        {
            Logger.LogInformation("⏭️ Skipping - no valid Omni configuration");
            return;
        }

        // Arrange
        using var client = CreateClient();
        var testData = CreateTestData();

        try
        {
            // Act
            var result = await client.[Operation](testData);

            // Assert
            result.Should().NotBeNull();
            result.Should().[Assertion]();

            Logger.LogInformation("✅ Test passed: [description]");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
        {
            Logger.LogInformation("🔒 Permission denied - expected with Reader role");
        }
        finally
        {
            // Cleanup
            await CleanupTestData(testData);
        }
    }
}
```

### Naming Conventions

**Test Methods**:
- Format: `[Subject]_[Scenario]_[ExpectedResult]`
- Examples:
  - `Cluster_Create_Success`
  - `User_Delete_RemovesUserAndIdentity`
  - `Template_Sync_DryRun_DoesNotApply`

**Test Files**:
- Format: `[Feature][Operation]Tests.cs`
- Examples:
  - `ResourceCrudTests.cs`
  - `UserManagementTests.cs`
  - `TemplateOperationsTests.cs`

### Test Categories

```csharp
[Trait("Category", "Integration")]     // All integration tests
[Trait("Category", "CRUD")]             // CRUD operations
[Trait("Category", "Streaming")]        // Streaming/watch operations
[Trait("Category", "Management")]       // Management service
[Trait("Category", "Resources")]        // Resource operations
[Trait("Category", "SlowTest")]         // Tests > 5 seconds
```

---

## 📊 **Progress Tracking**

### Test Implementation Checklist

**Phase 1: Resource CRUD** (33 tests)
- [ ] Core CRUD Tests (15)
  - [ ] Cluster CRUD (5)
  - [ ] Machine CRUD (5)
  - [ ] ConfigPatch CRUD (5)
- [ ] Watch & Streaming (6)
- [ ] Apply Operations (6)
- [ ] Bulk Delete (6)

**Phase 2: User Management** (12 tests)
- [ ] User Management Service (6)
- [ ] User/Identity Resources (6)

**Phase 3: Cluster Operations** (10 tests)
- [ ] Status & Lifecycle (5)
- [ ] Machine Lock/Unlock (5)

**Phase 4: Template Operations** (14 tests)
- [ ] Loading & Rendering (7)
- [ ] Sync & Diff (7)

**Phase 5: Advanced Features** (13 tests)
- [ ] Advanced Filtering (6)
- [ ] Infrastructure Resources (7)

**Total**: 0/82 tests completed

### Daily Targets

| Day | Target Tests | Cumulative |
|-----|--------------|------------|
| Day 1 | 21 | 21 |
| Day 2 | 12 | 33 |
| Day 3 | 6 | 39 |
| Day 4 | 6 | 45 |
| Day 5 | 0 (Review) | 45 |
| Day 6 | 5 | 50 |
| Day 7 | 5 | 55 |
| Day 8 | 7 | 62 |
| Day 9 | 7 | 69 |
| Day 10 | 0 (Review) | 69 |
| Day 11 | 6 | 75 |
| Day 12 | 7 | 82 |
| Day 13 | Gap filling | ~90 |
| Day 14 | Gap filling | ~100 |
| Day 15 | Final review | 110+ |

---

## 🚀 **Quick Start**

### Step 1: Set Up Test Infrastructure

```bash
# Ensure appsettings.json is configured
dotnet test --list-tests

# Run existing tests to verify setup
dotnet test --filter "Category=Integration"
```

### Step 2: Start with Phase 1

```bash
# Create test file
New-Item SideroLabs.Omni.Api.Tests\Resources\ResourceCrudTests.cs

# Implement first 5 tests (Cluster CRUD)
# Run tests
dotnet test --filter "FullyQualifiedName~ResourceCrudTests"
```

### Step 3: Track Progress

```bash
# Get coverage report
dotnet test --collect:"XPlat Code Coverage"

# View coverage
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
```

---

## 📝 **Notes**

### Test Data Management

**Create Test Data**:
```csharp
private static string CreateUniqueId() => $"test-{Guid.NewGuid():N}";

private static Cluster CreateTestCluster() => new ClusterBuilder()
    .WithName(CreateUniqueId())
    .WithKubernetesVersion("v1.29.0")
    .WithTalosVersion("v1.7.0")
    .Build();
```

**Cleanup Test Data**:
```csharp
private async Task CleanupTestCluster(OmniClient client, string clusterId)
{
    try
    {
        await client.Resources.DeleteAsync<Cluster>(clusterId);
        Logger.LogDebug("Cleaned up test cluster: {Id}", clusterId);
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "Failed to cleanup test cluster: {Id}", clusterId);
    }
}
```

### Handling Permissions

All tests should handle `PermissionDenied` gracefully:

```csharp
catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
{
    Logger.LogInformation("🔒 Permission denied - expected with Reader role");
    Logger.LogInformation("✅ API authentication working correctly");
}
```

### Performance Considerations

- Keep individual tests < 5 seconds
- Use `[Trait("Category", "SlowTest")]` for longer tests
- Parallelize where possible (use different test data)
- Clean up resources immediately after test

---

## ✅ **Definition of Done**

A test is considered "done" when:

1. ✅ Test method exists and compiles
2. ✅ Test follows naming convention
3. ✅ Test has proper error handling
4. ✅ Test cleans up resources
5. ✅ Test passes consistently (3+ runs)
6. ✅ Test is properly categorized
7. ✅ Test has XML documentation
8. ✅ Code coverage increased

---

## 🎯 **Final Goal**

By the end of this plan:

✅ **110+ integration tests** covering all operations  
✅ **90%+ code coverage** across the library  
✅ **100% CRUD coverage** for all resource types  
✅ **Zero regressions** - all existing tests pass  
✅ **Production ready** - confidence to ship  

**Status**: 🚀 **READY TO START**

---

*Created: January 18, 2025*  
*Author: AI Assistant*  
*Status: Approved*  
*Start Date: [TBD]*

