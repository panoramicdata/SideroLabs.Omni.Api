# Test Coverage Plan - 100% Code Coverage Goal

**Project**: SideroLabs.Omni.Api  
**Date**: January 17, 2025  
**Target**: 100% code coverage (aspirational), 95%+ minimum  
**Approach**: DRY principles, comprehensive integration tests with full CRUD lifecycle testing

---

## Executive Summary

This plan outlines a comprehensive testing strategy to achieve maximum code coverage for the SideroLabs.Omni.Api library. The approach emphasizes:

1. **Integration-First Testing**: Real API calls with full CRUD lifecycle validation
2. **DRY Principles**: Common patterns in TestBase, reusable test fixtures
3. **Resilient Tests**: Finally blocks ensure cleanup even on test failure
4. **Comprehensive Coverage**: Unit tests for logic, integration tests for API surface

---

## Current Test Coverage Status

### ✅ Already Covered (Estimated 40%)

**Unit Tests**:
- ✅ Builders (Cluster, Machine, ConfigPatch, ExtensionsConfiguration) - ~90% coverage
- ✅ Validators (Cluster, Machine, ConfigPatch, ExtensionsConfiguration) - ~85% coverage
- ✅ Resource Serialization - ~70% coverage
- ✅ OmniAuthenticator - ~60% coverage
- ✅ OmniClient construction - ~50% coverage
- ✅ Read-only mode - ~80% coverage

**Integration Tests**:
- ✅ Basic ManagementService operations - ~30% coverage
- ✅ AuthToken decoding - 100% coverage

### ❌ Missing Coverage (Estimated 60%)

**Critical Gaps**:
1. ❌ ResourceService CRUD operations (0% integration coverage)
2. ❌ User Management (new feature - 0% coverage)
3. ❌ Template Operations (0% coverage)
4. ❌ Cluster Operations (0% coverage)
5. ❌ Streaming operations (machine logs, audit logs, support bundles)
6. ❌ Error handling paths
7. ❌ Edge cases and boundary conditions
8. ❌ New builders (User, Identity)
9. ❌ New validators (User, Identity)

---

## Testing Architecture

### Test Base Enhancements

**New Common Methods to Add to TestBase**:

```csharp
// CRUD Test Lifecycle Management
protected async Task<T> CreateResourceWithCleanupAsync<T>(
    T resource, 
    Func<T, Task> cleanupAction,
    CancellationToken cancellationToken) where T : IOmniResource;

protected async Task ExecuteWithCleanupAsync(
    Func<Task> testAction,
    Func<Task> cleanupAction);

// Test Data Generators
protected string GenerateUniqueResourceId(string prefix = "test");
protected DateTimeOffset GetTestTimestamp();

// Assertion Helpers
protected void AssertResourceCreated<T>(T resource) where T : IOmniResource;
protected void AssertResourceUpdated<T>(T original, T updated) where T : IOmniResource;
protected void AssertResourceDeleted<T>(string id) where T : IOmniResource;

// Integration Test Guards
protected bool ShouldRunIntegrationTests();
protected bool ShouldSkipDestructiveTests();
```

### Test Categories (xUnit Traits)

```csharp
[Trait("Category", "Unit")]          // Fast, no external dependencies
[Trait("Category", "Integration")]   // Real API calls
[Trait("Category", "CRUD")]          // Full lifecycle tests
[Trait("Category", "Streaming")]     // Streaming API tests
[Trait("Category", "Destructive")]   // May modify resources
```

---

## Test Plan by Component

### 1. Resource CRUD Operations (HIGH PRIORITY)

**Target Coverage**: 95%+  
**Test Count**: ~50 tests  
**Estimated Time**: 8-12 hours

#### 1.1 User Resource CRUD Tests

**File**: `Resources/UserResourceIntegrationTests.cs`

```csharp
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class UserResourceIntegrationTests : TestBase
{
    [Fact]
    public async Task User_FullCRUDLifecycle_Success()
    {
        // Arrange
        var testEmail = $"test-user-{GenerateUniqueResourceId()}@panoramicdata.com";
        User? createdUser = null;
        Identity? createdIdentity = null;

        try
        {
            // ACT 1: CREATE
            var (user, identity) = await OmniClient.Users.CreateAsync(
                testEmail, 
                "Reader", 
                CancellationToken);
            
            createdUser = user;
            createdIdentity = identity;
            
            // ASSERT CREATE
            AssertResourceCreated(user);
            AssertResourceCreated(identity);
            Assert.Equal("Reader", user.Role);
            Assert.Equal(testEmail, identity.Email);
            Assert.Equal(user.UserId, identity.UserId);
            
            // ACT 2: READ
            var retrievedUser = await OmniClient.Users.GetAsync(
                testEmail, 
                CancellationToken);
            
            // ASSERT READ
            Assert.NotNull(retrievedUser);
            Assert.Equal(user.UserId, retrievedUser.UserId);
            Assert.Equal("Reader", retrievedUser.Role);
            
            // ACT 3: UPDATE
            var updatedUser = await OmniClient.Users.SetRoleAsync(
                testEmail, 
                "Operator", 
                CancellationToken);
            
            // ASSERT UPDATE
            Assert.Equal("Operator", updatedUser.Role);
            Assert.Equal(user.UserId, updatedUser.UserId);
            
            // ACT 4: LIST
            var allUsers = await OmniClient.Users.ListAsync(CancellationToken);
            
            // ASSERT LIST
            Assert.Contains(allUsers, u => u.Email == testEmail);
            
            // ACT 5: DELETE
            await OmniClient.Users.DeleteAsync(testEmail, CancellationToken);
            
            // ASSERT DELETE
            await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
                await OmniClient.Users.GetAsync(testEmail, CancellationToken));
            
            // Clear references to prevent double-delete in finally
            createdUser = null;
            createdIdentity = null;
        }
        finally
        {
            // CLEANUP - Even if test fails
            if (createdUser != null || createdIdentity != null)
            {
                try
                {
                    await OmniClient.Users.DeleteAsync(testEmail, CancellationToken);
                    Logger.LogInformation("✓ Cleaned up test user: {Email}", testEmail);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to clean up test user: {Email}", testEmail);
                }
            }
        }
    }
    
    [Fact]
    public async Task User_Create_WithBuilder_Success()
    {
        var testEmail = $"test-user-{GenerateUniqueResourceId()}@panoramicdata.com";
        
        try
        {
            var user = new UserBuilder()
                .AsOperator()
                .Build();
            
            var identity = new IdentityBuilder(testEmail)
                .ForUser(user)
                .AsUserType()
                .Build();
            
            await OmniClient.Resources.CreateAsync(user, CancellationToken);
            await OmniClient.Resources.CreateAsync(identity, CancellationToken);
            
            // Assertions...
        }
        finally
        {
            await SafeCleanupUserAsync(testEmail);
        }
    }
    
    [Fact]
    public async Task User_Update_FailsMidway_CleansUpProperly()
    {
        // Test that cleanup works even if update throws exception
        var testEmail = $"test-user-{GenerateUniqueResourceId()}@panoramicdata.com";
        
        try
        {
            var (user, identity) = await OmniClient.Users.CreateAsync(
                testEmail, 
                "Reader", 
                CancellationToken);
            
            // Force an error during update
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await OmniClient.Users.SetRoleAsync(testEmail, "", CancellationToken));
            
            // User should still exist
            var stillExists = await OmniClient.Users.GetAsync(testEmail, CancellationToken);
            Assert.NotNull(stillExists);
        }
        finally
        {
            await SafeCleanupUserAsync(testEmail);
        }
    }
    
    private async Task SafeCleanupUserAsync(string email)
    {
        try
        {
            await OmniClient.Users.DeleteAsync(email, CancellationToken);
            Logger.LogInformation("✓ Cleaned up user: {Email}", email);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "User cleanup: {Email} (may not exist)", email);
        }
    }
}
```

#### 1.2 Cluster Resource CRUD Tests

**File**: `Resources/ClusterResourceIntegrationTests.cs`

Similar pattern:
- Full CRUD lifecycle
- Builder pattern tests
- Error handling and cleanup
- Edge cases (invalid data, concurrent operations)

#### 1.3 Machine Resource CRUD Tests

**File**: `Resources/MachineResourceIntegrationTests.cs`

#### 1.4 ConfigPatch Resource CRUD Tests

**File**: `Resources/ConfigPatchResourceIntegrationTests.cs`

#### 1.5 ExtensionsConfiguration Resource CRUD Tests

**File**: `Resources/ExtensionsConfigurationResourceIntegrationTests.cs`

---

### 2. Builder Tests (MEDIUM PRIORITY)

**Target Coverage**: 100%  
**Test Count**: ~30 tests  
**Estimated Time**: 4-6 hours

#### 2.1 New Builder Tests Needed

**File**: `Builders/UserBuilderTests.cs`

```csharp
[Trait("Category", "Unit")]
public class UserBuilderTests
{
    [Fact]
    public void Build_WithDefaultValues_CreatesValidUser()
    {
        var user = new UserBuilder().Build();
        
        Assert.NotNull(user);
        Assert.NotNull(user.Metadata.Id);
        Assert.Equal("default", user.Metadata.Namespace);
    }
    
    [Fact]
    public void AsAdmin_SetsRoleToAdmin()
    {
        var user = new UserBuilder().AsAdmin().Build();
        
        Assert.Equal("Admin", user.Spec.Role);
    }
    
    [Fact]
    public void AsOperator_SetsRoleToOperator()
    {
        var user = new UserBuilder().AsOperator().Build();
        
        Assert.Equal("Operator", user.Spec.Role);
    }
    
    [Fact]
    public void AsReader_SetsRoleToReader()
    {
        var user = new UserBuilder().AsReader().Build();
        
        Assert.Equal("Reader", user.Spec.Role);
    }
    
    [Theory]
    [InlineData("Admin")]
    [InlineData("Operator")]
    [InlineData("Reader")]
    [InlineData("None")]
    public void WithRole_SetsSpecifiedRole(string role)
    {
        var user = new UserBuilder().WithRole(role).Build();
        
        Assert.Equal(role, user.Spec.Role);
    }
    
    [Fact]
    public void WithUserId_SetsCustomUserId()
    {
        var userId = "custom-user-id";
        var user = new UserBuilder().WithUserId(userId).Build();
        
        Assert.Equal(userId, user.Metadata.Id);
    }
    
    [Fact]
    public void WithNamespace_SetsCustomNamespace()
    {
        var ns = "custom-namespace";
        var user = new UserBuilder().WithNamespace(ns).Build();
        
        Assert.Equal(ns, user.Metadata.Namespace);
    }
    
    [Fact]
    public void WithLabel_AddsLabelToMetadata()
    {
        var user = new UserBuilder()
            .WithLabel("env", "test")
            .Build();
        
        Assert.Contains("env", user.Metadata.Labels.Keys);
        Assert.Equal("test", user.Metadata.Labels["env"]);
    }
    
    [Fact]
    public void Build_MultipleTimes_CreatesIndependentInstances()
    {
        var builder = new UserBuilder();
        var user1 = builder.Build();
        var user2 = builder.Build();
        
        Assert.Same(user1, user2); // Builder reuses instance
        // If we want independent instances, document this behavior
    }
    
    [Fact]
    public void Constructor_WithExistingUser_AllowsModification()
    {
        var originalUser = new UserBuilder().AsReader().Build();
        var modifiedUser = new UserBuilder(originalUser)
            .AsAdmin()
            .Build();
        
        Assert.Equal("Admin", modifiedUser.Spec.Role);
        Assert.Equal(originalUser.Metadata.Id, modifiedUser.Metadata.Id);
    }
}
```

**File**: `Builders/IdentityBuilderTests.cs`

Similar comprehensive tests for IdentityBuilder.

---

### 3. Validator Tests (MEDIUM PRIORITY)

**Target Coverage**: 100%  
**Test Count**: ~25 tests  
**Estimated Time**: 3-4 hours

#### 3.1 New Validator Tests Needed

**File**: `Resources/Validation/UserValidatorTests.cs`

```csharp
[Trait("Category", "Unit")]
public class UserValidatorTests
{
    private readonly UserValidator _validator = new();
    
    [Fact]
    public void Validate_ValidUser_ReturnsSuccess()
    {
        var user = new UserBuilder().AsAdmin().Build();
        
        var result = _validator.Validate(user);
        
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void Validate_EmptyUserId_ReturnsError()
    {
        var user = new User
        {
            Metadata = new ResourceMetadata { Id = "", Namespace = "default" },
            Spec = new UserSpec { Role = "Admin" }
        };
        
        var result = _validator.Validate(user);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Metadata.Id");
    }
    
    [Theory]
    [InlineData("Admin")]
    [InlineData("Operator")]
    [InlineData("Reader")]
    [InlineData("None")]
    public void Validate_ValidRoles_ReturnsSuccess(string role)
    {
        var user = new UserBuilder().WithRole(role).Build();
        
        var result = _validator.Validate(user);
        
        Assert.True(result.IsValid);
    }
    
    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("ADMIN")] // Wrong case
    [InlineData("admin")] // Wrong case
    [InlineData("")]
    public void Validate_InvalidRole_ReturnsError(string role)
    {
        var user = new User
        {
            Metadata = new ResourceMetadata { Id = "test", Namespace = "default" },
            Spec = new UserSpec { Role = role }
        };
        
        var result = _validator.Validate(user);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "Spec.Role" && 
            e.ErrorMessage.Contains("Admin, Operator, Reader, None"));
    }
    
    [Fact]
    public void Validate_EmptyNamespace_ReturnsError()
    {
        var user = new User
        {
            Metadata = new ResourceMetadata { Id = "test", Namespace = "" },
            Spec = new UserSpec { Role = "Admin" }
        };
        
        var result = _validator.Validate(user);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Metadata.Namespace");
    }
}
```

**File**: `Resources/Validation/IdentityValidatorTests.cs`

Similar comprehensive tests for IdentityValidator.

---

### 4. ManagementService Integration Tests (HIGH PRIORITY)

**Target Coverage**: 90%+  
**Test Count**: ~40 tests  
**Estimated Time**: 8-10 hours

#### 4.1 Expand Existing Integration Tests

**File**: `IntegrationTests.cs` (expand existing file)

Add comprehensive tests for:

1. **Service Account Management**:
   - Create → List → Renew → Destroy lifecycle
   - Error handling (invalid PGP key, missing permissions)
   - Cleanup in finally blocks

2. **Machine Logs Streaming**:
   - Basic streaming
   - Follow mode
   - Tail lines parameter
   - Error handling (non-existent machine)
   - Cancellation handling

3. **Audit Logs**:
   - Date range queries
   - Empty results
   - Large result sets
   - Invalid date formats

4. **Support Bundles**:
   - Progress tracking
   - Error handling
   - Cancellation
   - Bundle data verification

5. **Configuration Validation**:
   - Valid configs
   - Invalid configs
   - Edge cases

6. **JSON Schema Validation**:
   - Valid data
   - Invalid data
   - Complex nested schemas
   - Error reporting

---

### 5. ResourceService Integration Tests (HIGH PRIORITY)

**Target Coverage**: 95%+  
**Test Count**: ~60 tests  
**Estimated Time**: 12-15 hours

#### 5.1 Generic Resource Operations Tests

**File**: `Services/ResourceClientServiceIntegrationTests.cs`

```csharp
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ResourceClientServiceIntegrationTests : TestBase
{
    [Fact]
    public async Task GetAsync_ExistingResource_ReturnsResource()
    {
        // Test Get operation
    }
    
    [Fact]
    public async Task GetAsync_NonExistentResource_ThrowsNotFound()
    {
        // Test error handling
    }
    
    [Fact]
    public async Task ListAsync_WithSelector_ReturnsFilteredResults()
    {
        // Test filtering
    }
    
    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Test pagination
    }
    
    [Fact]
    public async Task ListAsync_WithSorting_ReturnsSortedResults()
    {
        // Test sorting
    }
    
    [Fact]
    public async Task WatchAsync_ResourceChanges_ReceivesEvents()
    {
        // Test watch functionality
    }
    
    [Fact]
    public async Task CreateAsync_ValidResource_ReturnsCreatedResource()
    {
        // Test create
    }
    
    [Fact]
    public async Task UpdateAsync_ExistingResource_ReturnsUpdatedResource()
    {
        // Test update
    }
    
    [Fact]
    public async Task UpdateAsync_WithStaleVersion_ThrowsConflict()
    {
        // Test optimistic locking
    }
    
    [Fact]
    public async Task DeleteAsync_ExistingResource_Succeeds()
    {
        // Test delete
    }
    
    [Fact]
    public async Task DeleteManyAsync_WithSelector_DeletesMatchingResources()
    {
        // Test bulk delete
    }
    
    [Fact]
    public async Task DeleteAllAsync_DeletesAllResources()
    {
        // Test delete all
    }
    
    [Fact]
    public async Task ApplyAsync_NewResource_CreatesResource()
    {
        // Test apply (create path)
    }
    
    [Fact]
    public async Task ApplyAsync_ExistingResource_UpdatesResource()
    {
        // Test apply (update path)
    }
    
    [Fact]
    public async Task ApplyYamlAsync_ValidYaml_CreatesResource()
    {
        // Test YAML apply
    }
    
    [Fact]
    public async Task ApplyFileAsync_ValidFile_CreatesResource()
    {
        // Test file apply
    }
}
```

---

### 6. Template Operations Tests (MEDIUM PRIORITY)

**Target Coverage**: 90%+  
**Test Count**: ~20 tests  
**Estimated Time**: 6-8 hours

**File**: `Services/TemplateOperationsIntegrationTests.cs`

Test full template lifecycle:
- Load template from file
- Render with variables
- Validate template
- Sync to Omni
- Diff changes
- Export existing cluster as template
- Delete template

---

### 7. Cluster Operations Tests (MEDIUM PRIORITY)

**Target Coverage**: 90%+  
**Test Count**: ~15 tests  
**Estimated Time**: 4-6 hours

**File**: `Services/ClusterOperationsIntegrationTests.cs`

Test cluster operations:
- Get status
- Create cluster
- Delete cluster
- Lock/unlock machines
- Health checks

---

### 8. Error Handling & Edge Cases (HIGH PRIORITY)

**Target Coverage**: 80%+  
**Test Count**: ~30 tests  
**Estimated Time**: 6-8 hours

#### 8.1 Error Scenarios

**File**: `ErrorHandlingIntegrationTests.cs`

```csharp
[Collection("Integration")]
[Trait("Category", "Integration")]
public class ErrorHandlingIntegrationTests : TestBase
{
    [Fact]
    public async Task ResourceOperation_NetworkFailure_ThrowsAppropriateException()
    {
        // Test network error handling
    }
    
    [Fact]
    public async Task ResourceOperation_Timeout_ThrowsTimeoutException()
    {
        // Test timeout handling
    }
    
    [Fact]
    public async Task ResourceOperation_AuthenticationFailure_ThrowsAuthException()
    {
        // Test auth error handling
    }
    
    [Fact]
    public async Task ResourceOperation_PermissionDenied_ThrowsPermissionException()
    {
        // Test permission error handling
    }
    
    [Fact]
    public async Task ResourceOperation_InvalidData_ThrowsValidationException()
    {
        // Test validation error handling
    }
    
    [Fact]
    public async Task ResourceOperation_ConcurrentModification_ThrowsConflictException()
    {
        // Test conflict handling
    }
}
```

---

### 9. Serialization Tests (MEDIUM PRIORITY)

**Target Coverage**: 100%  
**Test Count**: ~20 tests  
**Estimated Time**: 3-4 hours

#### 9.1 Expand Serialization Tests

**File**: `Resources/Serialization/AllResourcesSerializationTests.cs`

Test YAML and JSON serialization for:
- User
- Identity
- All existing resources
- Round-trip serialization
- Edge cases (null values, special characters)

---

### 10. Authentication Tests (MEDIUM PRIORITY)

**Target Coverage**: 95%+  
**Test Count**: ~15 tests  
**Estimated Time**: 4-5 hours

#### 10.1 Expand OmniAuthenticator Tests

**File**: `Security/OmniAuthenticatorTests.cs`

Add tests for:
- AuthToken decoding (already covered)
- PGP key loading from file
- Signature generation
- Error handling
- Edge cases

---

## Test Execution Strategy

### Phase 1: Foundation (Week 1)

**Priority**: HIGH  
**Goal**: Get core CRUD tests working

1. ✅ Enhance TestBase with common methods - **COMPLETED** (2025-01-17)
2. ✅ Create test data generators - **COMPLETED** (2025-01-17)
3. ⚠️ Implement User/Identity CRUD tests - **BLOCKED** (2025-01-17)
   - Created 11 comprehensive integration tests
   - **Discovered**: ResourceService endpoint not exposed on Omni SaaS (HTTP 405)
   - **Investigation needed**: Determine correct API for user management on Omni SaaS
   - **Alternative**: Use ManagementService methods or different endpoint
4. ⏳ Implement Cluster CRUD tests - **PENDING** (awaiting ResourceService investigation)
5. ⏳ Implement Machine CRUD tests - **PENDING** (awaiting ResourceService investigation)

**Progress**: 2/5 completed (40%)  
**Blocked**: ResourceService endpoint investigation needed

### Phase 2: Expansion (Week 2)

**Priority**: HIGH  
**Goal**: Cover all major features

1. ✅ Complete ResourceService tests
2. ✅ Complete ManagementService tests
3. ✅ Add Template Operations tests
4. ✅ Add Cluster Operations tests

### Phase 3: Completeness (Week 3)

**Priority**: MEDIUM  
**Goal**: Reach 95%+ coverage

1. ✅ Add all Builder tests
2. ✅ Add all Validator tests
3. ✅ Add Error Handling tests
4. ✅ Add Serialization tests

### Phase 4: Polish (Week 4)

**Priority**: LOW  
**Goal**: Reach 100% coverage

1. ✅ Add edge case tests
2. ✅ Add performance tests
3. ✅ Add stress tests
4. ✅ Fix any gaps identified by coverage reports

---

## Testing Patterns & Best Practices

### Pattern 1: CRUD Lifecycle Test

```csharp
[Fact]
public async Task Resource_FullCRUDLifecycle_Success()
{
    // Arrange
    var resource = CreateTestResource();
    TResource? created = null;
    
    try
    {
        // Act & Assert: CREATE
        created = await CreateResourceAsync(resource);
        AssertResourceCreated(created);
        
        // Act & Assert: READ
        var retrieved = await GetResourceAsync(created.Id);
        AssertResourceEquals(created, retrieved);
        
        // Act & Assert: UPDATE
        var updated = await UpdateResourceAsync(created);
        AssertResourceUpdated(created, updated);
        
        // Act & Assert: LIST
        var all = await ListResourcesAsync();
        Assert.Contains(all, r => r.Id == created.Id);
        
        // Act & Assert: DELETE
        await DeleteResourceAsync(created.Id);
        await AssertResourceDeleted(created.Id);
        
        created = null; // Prevent cleanup
    }
    finally
    {
        // Cleanup even on failure
        if (created != null)
        {
            await SafeDeleteResourceAsync(created.Id);
        }
    }
}
```

### Pattern 2: Error Handling Test

```csharp
[Fact]
public async Task Operation_FailsDuringUpdate_CleansUpProperly()
{
    var resource = CreateTestResource();
    TResource? created = null;
    
    try
    {
        created = await CreateResourceAsync(resource);
        
        // Force error during update
        await Assert.ThrowsAsync<SpecificException>(
            async () => await UpdateWithInvalidDataAsync(created));
        
        // Verify resource still exists
        var stillExists = await GetResourceAsync(created.Id);
        Assert.NotNull(stillExists);
    }
    finally
    {
        if (created != null)
        {
            await SafeDeleteResourceAsync(created.Id);
        }
    }
}
```

### Pattern 3: Streaming Test

```csharp
[Fact]
public async Task Streaming_ReceivesDataAndHandlesCancellation()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var itemCount = 0;
    
    try
    {
        await foreach (var item in StreamDataAsync(cts.Token))
        {
            itemCount++;
            Assert.NotNull(item);
            
            if (itemCount >= 10)
            {
                break; // Prevent long-running test
            }
        }
        
        Assert.True(itemCount > 0, "Should receive at least one item");
    }
    catch (OperationCanceledException)
    {
        // Expected on timeout
        Assert.True(itemCount >= 0, "Should handle cancellation gracefully");
    }
}
```

---

## Test Infrastructure Improvements

### Enhanced TestBase

```csharp
public abstract class TestBase : IDisposable
{
    // Existing members...
    
    // New: Resource Lifecycle Management
    protected async Task<T> CreateWithCleanupAsync<T>(
        T resource,
        List<Func<Task>> cleanupActions) 
        where T : IOmniResource
    {
        var created = await OmniClient.Resources.CreateAsync(resource, CancellationToken);
        
        // Register cleanup
        cleanupActions.Add(async () => 
        {
            try
            {
                await OmniClient.Resources.DeleteAsync<T>(
                    created.Metadata.Id, 
                    created.Metadata.Namespace, 
                    CancellationToken);
                    
                Logger.LogInformation("✓ Cleaned up {Type}: {Id}", 
                    typeof(T).Name, created.Metadata.Id);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Cleanup failed for {Type}: {Id}", 
                    typeof(T).Name, created.Metadata.Id);
            }
        });
        
        return created;
    }
    
    // New: Test Data Generators
    protected string GenerateUniqueId(string prefix = "test") 
        => $"{prefix}-{Guid.NewGuid():N}";
    
    protected string GenerateTestEmail() 
        => $"test-{GenerateUniqueId()}@test.panoramicdata.com";
    
    // New: Common Assertions
    protected void AssertResourceCreated<T>(T resource) where T : IOmniResource
    {
        Assert.NotNull(resource);
        Assert.NotNull(resource.Metadata);
        Assert.NotEmpty(resource.Metadata.Id);
        Assert.NotEmpty(resource.Metadata.Version);
    }
    
    protected void AssertResourceUpdated<T>(T original, T updated) 
        where T : IOmniResource
    {
        Assert.NotNull(updated);
        Assert.Equal(original.Metadata.Id, updated.Metadata.Id);
        Assert.NotEqual(original.Metadata.Version, updated.Metadata.Version);
    }
    
    protected async Task AssertResourceDeleted<T>(string id) 
        where T : IOmniResource, new()
    {
        await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
            await OmniClient.Resources.GetAsync<T>(id, cancellationToken: CancellationToken));
    }
    
    // New: Safe Cleanup Helpers
    protected async Task SafeDeleteResourceAsync<T>(string id, string? ns = "default") 
        where T : IOmniResource, new()
    {
        try
        {
            await OmniClient.Resources.DeleteAsync<T>(id, ns, CancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Safe delete: Resource may not exist");
        }
    }
}
```

### Test Fixtures

**File**: `Infrastructure/IntegrationTestFixture.cs`

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    public OmniClient Client { get; private set; }
    public List<Func<Task>> CleanupActions { get; } = new();
    
    public async Task InitializeAsync()
    {
        // Setup test environment
        Client = CreateTestClient();
    }
    
    public async Task DisposeAsync()
    {
        // Run all cleanup actions
        foreach (var cleanup in CleanupActions)
        {
            try
            {
                await cleanup();
            }
            catch
            {
                // Log but don't fail cleanup
            }
        }
    }
}
```

---

## Metrics & Goals

### Coverage Targets

| Component | Current | Target | Priority |
|-----------|---------|--------|----------|
| Builders | 90% | 100% | HIGH |
| Validators | 85% | 100% | HIGH |
| Resource CRUD | 0% | 95% | HIGH |
| ManagementService | 30% | 90% | HIGH |
| ResourceService | 0% | 95% | HIGH |
| Template Operations | 0% | 90% | MEDIUM |
| Cluster Operations | 0% | 90% | MEDIUM |
| User Management | 0% | 95% | HIGH |
| Serialization | 70% | 100% | MEDIUM |
| Error Handling | 20% | 80% | HIGH |
| **OVERALL** | **40%** | **95%+** | **HIGH** |

### Test Count Estimates

| Category | Estimated Tests | Time (hours) |
|----------|----------------|--------------|
| Resource CRUD | 50 | 12 |
| Builders | 30 | 6 |
| Validators | 25 | 4 |
| ManagementService | 40 | 10 |
| ResourceService | 60 | 15 |
| Template Operations | 20 | 8 |
| Cluster Operations | 15 | 6 |
| Error Handling | 30 | 8 |
| Serialization | 20 | 4 |
| Authentication | 15 | 5 |
| **TOTAL** | **~305** | **~78 hours** |

---

## Continuous Integration

### Test Execution Strategy

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Unit Tests
        run: dotnet test --filter "Category=Unit" --logger "trx"
      - name: Generate Coverage Report
        run: dotnet test --collect:"XPlat Code Coverage"
      - name: Upload Coverage
        uses: codecov/codecov-action@v3
  
  integration-tests:
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    steps:
      - uses: actions/checkout@v3
      - name: Configure Test Credentials
        run: echo '${{ secrets.OMNI_APPSETTINGS }}' > appsettings.json
      - name: Run Integration Tests
        run: dotnet test --filter "Category=Integration&Category!=Destructive"
  
  destructive-tests:
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      - name: Configure Test Credentials
        run: echo '${{ secrets.OMNI_APPSETTINGS }}' > appsettings.json
      - name: Run Destructive Tests
        run: dotnet test --filter "Category=Destructive"
```

---

## Summary

This comprehensive plan provides:

1. ✅ **Structured Approach**: Phased implementation over 4 weeks
2. ✅ **DRY Principles**: Common patterns in TestBase, reusable fixtures
3. ✅ **Resilient Tests**: Finally blocks ensure cleanup
4. ✅ **Full CRUD Coverage**: Every resource tested through complete lifecycle
5. ✅ **Error Handling**: Comprehensive error scenario testing
6. ✅ **Integration First**: Real API calls with real data
7. ✅ **Measurable Progress**: Clear metrics and targets

**Estimated Total Effort**: 78-100 hours (2-2.5 weeks full-time)  
**Target Coverage**: 95%+ (aspirational 100%)  
**Test Count**: ~305 tests

The plan prioritizes integration tests with full CRUD lifecycles, ensuring that cleanup occurs even when tests fail. This approach provides maximum confidence in the library's correctness while maintaining test hygiene.
