# ResourceService Not Available on Omni SaaS

## Summary

The COSI ResourceService (`/omni.resources.ResourceService/*`) is **NOT available as a gRPC endpoint on Omni SaaS**. 

All code depending on this service has been removed from the public API but is preserved in the codebase for potential on-premise Omni deployments.

## Removed from Public API

### Primary

- `IOmniClient.Resources` - ResourceService client
- `IOmniClient.Users` - User management (depends on Resources)
- `IOmniClient.Clusters` - Cluster operations (depends on Resources)
- `IOmniClient.Templates` - Template operations (depends on Resources)

### Supporting

The following remain in the codebase but are not exposed:

**Interfaces:**
- `IOmniResourceClient.cs`
- `IUserManagement.cs`
- `IClusterOperations.cs`
- `ITemplateOperations.cs`

**Implementations:**
- `ResourceClientService.cs`
- `UserManagement.cs`
- `ClusterOperations.cs`
- `TemplateOperations.cs`

**Resource Types:**
- `User.cs`, `UserSpec.cs`, `UserStatus.cs`
- `Identity.cs`, `IdentitySpec.cs`, `IdentityStatus.cs`
- `Cluster.cs`, `ClusterSpec.cs`, `ClusterStatus.cs`
- All other COSI resource types

**Examples:**
- `UserManagementExample.cs` - Commented out / removed

**Tests:**
- `UserResourceIntegrationTests.cs` - Commented out / removed
- `ResourceServiceDiagnosticTests.cs` - Kept for documentation

## Why Not Delete Entirely?

This code may work on:
1. **On-premise Omni deployments** - May expose ResourceService
2. **Future Omni SaaS updates** - If they enable ResourceService
3. **Documentation purposes** - Shows what was attempted

## Evidence

See [OMNI_SAAS_API_ARCHITECTURE.md](OMNI_SAAS_API_ARCHITECTURE.md) for detailed investigation showing HTTP 405 responses.

## What Works Instead?

**Use ManagementService for all operations:**

```csharp
// Instead of client.Users.CreateAsync()
// Users must be managed through the Omni web UI or omnictl

// Instead of client.Resources.ListAsync<Cluster>()
// Clusters can be managed through ManagementService operations
```

## Date

Removed from public API: October 18, 2025
