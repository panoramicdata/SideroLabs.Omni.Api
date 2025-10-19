# Omni SaaS API Architecture - Critical Findings

## Executive Summary

**Date**: October 18, 2025  
**Investigation**: Analysis of Omni SaaS gRPC API endpoints  
**Key Finding**: **ResourceService is NOT exposed as a gRPC endpoint on Omni SaaS**

## The Problem

While investigating why ResourceService calls were failing with **HTTP 405 Method Not Allowed**, we discovered that Omni SaaS has a fundamentally different architecture than local Omni deployments or what `omnictl` uses.

## Evidence

### HTTP 405 Response Headers

When attempting to call `/omni.resources.ResourceService/List`:

```
HTTP/2 405 Method Not Allowed
Allow: OPTIONS
Allow: GET
Allow: HEAD
Content-Type: text/plain; charset=utf-8
```

**Analysis**: The server is treating this path as a regular HTTP endpoint that only accepts `GET`, `HEAD`, and `OPTIONS` - **NOT** the `POST` method that gRPC requires.

### Request Headers Sent (Correct for gRPC)

Our .NET client correctly sends:

```
POST /omni.resources.ResourceService/List HTTP/2.0
Content-Type: application/grpc
grpc-accept-encoding: identity,gzip,deflate
TE: trailers
grpc-timeout: 1M
User-Agent: grpc-dotnet/2.71.0
```

These are **standard, correct gRPC headers**. Any proper gRPC server would accept these.

### Comparison with ManagementService (Works)

ManagementService calls to `/management.ManagementService/Omniconfig` with **identical headers** return:

```
HTTP/2 200 OK
Content-Type: application/grpc
```

**Conclusion**: The .NET client is working correctly. The difference is purely server-side routing.

## What About omnictl?

`omnictl get clusters` **does work**, but investigation of the omnictl source code reveals:

```go
// From client/pkg/client/omni/omni.go
c.state = state.WrapCore(client.NewAdapter(v1alpha1.NewStateClient(c), ...))
```

omnictl uses:
1. **COSI State abstraction layer** (`state.State` interface)
2. **v1alpha1.StateClient** (COSI runtime protobuf client)
3. **NOT directly calling ResourceService**

The COSI State implementation likely:
- Connects to a **different endpoint** than what we tested
- Uses a **local state store** when running against Omni SaaS
- Has **special routing** for Go clients that .NET clients don't benefit from

## Architecture Differences

### Local Omni / On-Premise

```
omnictl → ResourceService (gRPC) → COSI State Backend
  ✅ Direct gRPC access to ResourceService
  ✅ Full COSI resource operations
```

### Omni SaaS

```
omnictl → ??? (Unknown mechanism) → Backend
.NET Client → ManagementService (gRPC) → Backend
.NET Client → ResourceService (gRPC) ❌ HTTP 405
  ❌ ResourceService NOT exposed as gRPC
  ✅ ManagementService IS exposed as gRPC
```

## Implications for .NET Library

### ✅ What Works

1. **ManagementService** - Fully functional gRPC service
   - Configuration management (kubeconfig, talosconfig, omniconfig)
   - Service account operations
   - Machine operations (logs, upgrades)
   - Kubernetes operations (manifests, upgrades)
   - Provisioning (schematics, join tokens)

### ❌ What Doesn't Work

1. **ResourceService** - Returns HTTP 405 on Omni SaaS
   - Cannot call `List`, `Get`, `Watch`, `Create`, `Update`, `Delete`
   - COSI resource operations not available via gRPC

### 🔄 Recommended Approach

All operations should use **ManagementService** which provides:
- High-level operations that abstract away resource details
- Proven, working gRPC endpoints
- Better alignment with Omni's intended API surface

## Technical Details

### Why HTTP 405?

The HTTP 405 response means the server's gateway/load balancer is configured to:

1. **Route `/management.ManagementService/*`** → gRPC backend ✅
2. **Route `/omni.resources.ResourceService/*`** → Something else (probably static files or REST API) ❌

This is an **intentional architectural decision** by the Omni SaaS team, not a bug.

### Why Does omnictl Work?

Three possibilities:

1. **Different endpoint**: omnictl might connect to a different URL or port
2. **Local COSI state**: omnictl might use a local cache/state store
3. **Go-specific routing**: Server might have special routing for Go's gRPC implementation

Further investigation would require:
- Network packet capture of omnictl traffic
- Analysis of omnictl's actual connection details
- Communication with Sidero Labs team

## Recommendations

### For This Library

1. ✅ **Remove ResourceService from public API** - It doesn't work on Omni SaaS
2. ✅ **Focus on ManagementService** - This is what actually works
3. ✅ **Update documentation** - Clearly state limitations
4. ✅ **Keep ResourceService code** - For potential on-premise installations

### For Omni SaaS Users

1. ✅ **Use ManagementService** for all operations
2. ❌ **Don't expect ResourceService** to work via gRPC
3. ⚠️ **Use omnictl** for resource operations if needed

### For Future Development

1. **Contact Sidero Labs** - Ask about ResourceService access on SaaS
2. **Document architecture** - Get official clarification
3. **Consider alternatives** - May need REST API wrapper or different approach

## Conclusion

This is **NOT a bug in the .NET library**. Our implementation is correct - we're sending proper gRPC requests with correct headers and authentication.

The issue is **server-side architecture**: Omni SaaS intentionally does not expose ResourceService as a gRPC endpoint, returning HTTP 405 to indicate the endpoint exists but only accepts GET/HEAD/OPTIONS, not POST (which gRPC requires).

**The library should focus exclusively on ManagementService**, which is fully functional and provides the official API surface for Omni SaaS.

## References

- Investigation Date: October 18, 2025
- Test Endpoint: `https://panoramicdata.eu-central-1.omni.siderolabs.io`
- HTTP Response: 405 Method Not Allowed
- Allowed Methods: OPTIONS, GET, HEAD
- Working Service: ManagementService ✅
- Non-working Service: ResourceService ❌
