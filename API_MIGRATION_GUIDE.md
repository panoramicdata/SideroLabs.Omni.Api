# API Migration Guide - Examples and Tests

**Date**: January 19, 2025  
**Purpose**: Migrate all example and test code to use new refactored API

## Migration Pattern

### Old API (Deprecated):
```csharp
client.Management.GetKubeConfigAsync(...)
client.Management.GetTalosConfigAsync(...)
client.Management.ListServiceAccountsAsync(...)
client.Resources.ListAsync<Machine>()
```

### New API:
```csharp
client.KubeConfig.GetAsync(...)
client.TalosConfig.GetAsync(...)
client.ServiceAccounts.ListAsync(...)
client.Machines.ListAsync()
```

## Find & Replace Mapping

| Old API | New API | Notes |
|---------|---------|-------|
| `client.Management.GetKubeConfigAsync` | `client.KubeConfig.GetAsync` | Config service |
| `client.Management.GetTalosConfigAsync` | `client.TalosConfig.GetAsync` | Config service |
| `client.Management.GetOmniConfigAsync` | `client.OmniConfig.GetAsync` | Config service |
| `client.Management.CreateServiceAccountAsync` | `client.ServiceAccounts.CreateAsync` | Service accounts |
| `client.Management.ListServiceAccountsAsync` | `client.ServiceAccounts.ListAsync` | Service accounts |
| `client.Management.RenewServiceAccountAsync` | `client.ServiceAccounts.RenewAsync` | Service accounts |
| `client.Management.DestroyServiceAccountAsync` | `client.ServiceAccounts.DestroyAsync` | Service accounts |
| `client.Management.ValidateConfigAsync` | `client.Validation.ValidateConfigAsync` | Validation |
| `client.Management.ValidateJsonSchemaAsync` | `client.Validation.ValidateJsonSchemaAsync` | Validation |
| `client.Management.KubernetesUpgradePreChecksAsync` | `client.Kubernetes.UpgradePreChecksAsync` | K8s operations |
| `client.Management.StreamKubernetesSyncManifestsAsync` | `client.Kubernetes.StreamSyncManifestsAsync` | K8s operations |
| `client.Management.CreateSchematicAsync` | `client.Schematics.CreateAsync` | Schematics |
| `client.Management.StreamMachineLogsAsync` | `client.MachineManagement.StreamLogsAsync` | Machine mgmt |
| `client.Management.MaintenanceUpgradeAsync` | `client.MachineManagement.MaintenanceUpgradeAsync` | Machine mgmt |
| `client.Management.GetMachineJoinConfigAsync` | `client.MachineManagement.GetJoinConfigAsync` | Machine mgmt |
| `client.Management.CreateJoinTokenAsync` | `client.MachineManagement.CreateJoinTokenAsync` | Machine mgmt |
| `client.Management.GetSupportBundleAsync` | `client.Support.GetSupportBundleAsync` | Support |
| `client.Management.ReadAuditLogAsync` | `client.Support.ReadAuditLogAsync` | Support |
| `client.Management.TearDownLockedClusterAsync` | `client.Support.TearDownLockedClusterAsync` | Support |
| `client.Resources.ListAsync<Machine>()` | `client.Machines.ListAsync()` | Resource ops |
| `client.Resources.GetAsync<Machine>(id)` | `client.Machines.GetAsync(id)` | Resource ops |
| `client.Resources.CreateAsync(machine)` | `client.Machines.CreateAsync(machine)` | Resource ops |

## Files to Update

### Examples (High Priority):
1. ✅ `SideroLabs.Omni.Api.Examples\OmniClientExample.cs` - Main example file
2. ✅ `SideroLabs.Omni.Api.Examples\Scenarios\BasicUsageExample.cs` - Basic usage

### Tests (High Priority):
3. ✅ `SideroLabs.Omni.Api.Tests\Management\ManagementServiceConfigurationTests.cs`
4. ✅ `SideroLabs.Omni.Api.Tests\Management\ManagementServiceAccountTests.cs`

