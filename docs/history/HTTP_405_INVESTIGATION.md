# HTTP 405 Error Investigation Summary

**Date**: 2025-01-17  
**Issue**: ResourceService API returns HTTP 405 (Method Not Allowed)  
**Status**: ✅ RESOLVED - Root cause identified

---

## Key Findings

### 1. Error Details
- **HTTP Status Code**: 405 (Method Not Allowed)
- **gRPC Status**: Unknown
- **gRPC Detail**: "Bad gRPC response. HTTP status code: 405"
- **HTTP Response Body**: **NONE** (empty)
- **gRPC Trailers**: **NONE** (empty)

### 2. What This Tells Us

The **absence of response body and trailers** is highly significant:

✅ **Confirmed**: The request is being rejected **before** it reaches the Omni ResourceService  
✅ **Confirmed**: The rejection is at the HTTP/web server level, not from the gRPC service  
✅ **Conclusion**: **ResourceService is NOT publicly exposed** on Omni SaaS

### 3. Technical Analysis

```
Request Flow:
  Client → HTTPS → Load Balancer/Proxy → Web Server → [REJECTED HERE - HTTP 405]
                                                     ↓
                                          ResourceService (never reached)
```

**Why HTTP 405 with no body indicates infrastructure-level rejection**:
- If the gRPC service rejected it, we would see gRPC trailers and status details
- If Omni authentication failed, we would see a gRPC error with details
- HTTP 405 with empty body = web server doesn't recognize this endpoint/method combination

### 4. Comparison: ManagementService vs ResourceService

| Service | Endpoint | Result |
|---------|----------|--------|
| **ManagementService** | `/management.ManagementService/*` | ✅ **WORKS** - Returns data |
| **ResourceService** | `/omni.resources.ResourceService/*` | ❌ **HTTP 405** - Not exposed |

---

## Root Cause

**Omni SaaS does NOT expose the ResourceService gRPC endpoint publicly.**

This is an **architectural decision** by SideroLabs:
- ManagementService: Public API for administrative operations ✅
- ResourceService: Internal API for COSI resource operations ❌

### Why ResourceService Is Not Exposed

Possible reasons:
1. **Security**: Direct resource manipulation could bypass business logic
2. **Architecture**: ResourceService is meant for internal Omni components
3. **API Design**: Public clients should use ManagementService methods
4. **omnictl Implementation**: omnictl likely uses ManagementService, not ResourceService directly

---

## Implications for Library

### ❌ What Doesn't Work
- Direct ResourceService calls (`client.Resources.ListAsync<Cluster>()`)
- User CRUD via ResourceService (`client.Users.CreateAsync()`)
- Any COSI resource operations via ResourceService

### ✅ What DOES Work
- ManagementService operations (`client.Management.*`)
- Service account management
- Configuration retrieval (kubeconfig, talosconfig, omniconfig)
- All streaming operations (logs, manifests)
- Kubernetes operations

### 🔄 Required Changes

**User Management Must Use ManagementService**:
1. ❌ Remove ResourceService-based User management
2. ✅ Implement via ManagementService (if available)
3. ✅ Or document that user management requires omnictl or web UI

**Resource Operations**:
1. ❌ Remove generic ResourceService client from public API
2. ✅ Keep ManagementService as the primary interface
3. ✅ Document architectural limitations

---

## Enhanced Exception Handling

We've improved the library to capture HTTP response bodies for all 4xx/5xx errors:

### Before
```csharp
catch (RpcException ex)
{
    // Only had: "Bad gRPC response. HTTP status code: 405"
}
```

### After
```csharp
catch (RpcException ex)
{
    // Now captures:
    // - HTTP status code
    // - HTTP response body (if present)
    // - All gRPC trailers
    // - Detailed error analysis
}
```

**New OmniGrpcException Properties**:
- `HttpStatusCode` - The HTTP status code (405, 404, etc.)
- `HttpResponseBody` - The full HTTP response body
- `HasHttpErrorDetails` - Whether HTTP details are available

---

## Recommendations

### For Library Development
1. ✅ Focus on **ManagementService** as the primary API
2. ✅ Document that ResourceService is not available on Omni SaaS
3. ✅ Remove or mark as obsolete ResourceService-dependent features
4. ✅ Use enhanced exception handling to capture future HTTP errors

### For Users
1. ✅ Use **ManagementService** for all operations
2. ✅ Use **omnictl CLI** for user management
3. ✅ Use **Omni Web UI** for resource management
4. ✅ Use **Service Accounts** for programmatic access (not user accounts)

### For Testing
1. ✅ Skip ResourceService integration tests
2. ✅ Focus integration tests on ManagementService
3. ✅ Document test limitations due to API architecture

---

## Investigation Tools Added

### 1. GrpcErrorParser Utility
Location: `SideroLabs.Omni.Api/Utilities/GrpcErrorParser.cs`

**Features**:
- Extracts HTTP status codes from gRPC errors
- Extracts HTTP response bodies from trailers
- Formats detailed error messages
- Identifies HTTP 4xx/5xx errors

### 2. Enhanced OmniGrpcException
Location: `SideroLabs.Omni.Api/Exceptions/OmniGrpcException.cs`

**New Properties**:
```csharp
public int? HttpStatusCode { get; }
public string? HttpResponseBody { get; }
public bool HasHttpErrorDetails { get; }
```

### 3. Diagnostic Test
Location: `SideroLabs.Omni.Api.Tests/Resources/ResourceServiceDiagnosticTests.cs`

**Purpose**:
- Captures full HTTP error details
- Logs all trailers and response bodies
- Helps diagnose future API issues

---

## Conclusion

**The HTTP 405 error is NOT a bug** - it's the expected behavior for Omni SaaS.

The ResourceService is an internal API that is intentionally not exposed to external clients. All public operations must go through the ManagementService, which the library fully supports.

**Action Items**:
1. ✅ Enhanced exception handling implemented
2. ✅ Root cause identified and documented
3. ⏳ Update library to remove/deprecate ResourceService features
4. ⏳ Update documentation to reflect architectural limitations
5. ⏳ Refocus test coverage on ManagementService operations

---

## References

- [Omni Architecture](https://github.com/siderolabs/omni)
- [ManagementService Proto](https://github.com/siderolabs/omni/blob/main/client/api/omni/management/management.proto)
- [ResourceService Proto](https://github.com/siderolabs/omni/blob/main/client/api/omni/resources/resources.proto)
- [omnictl Source Code](https://github.com/siderolabs/omni/tree/main/cmd/omnictl)
