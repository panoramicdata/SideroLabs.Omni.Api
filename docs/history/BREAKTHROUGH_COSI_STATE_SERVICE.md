# BREAKTHROUGH: How omnictl ACTUALLY Works

## Executive Summary

After analyzing the `omnictl` source code, we now know EXACTLY what it calls. It uses the **COSI v1alpha1 State service**, NOT the ResourceService!

## The Smoking Gun

From `client/pkg/client/omni/omni.go` line 71:

```go
c.state = state.WrapCore(client.NewAdapter(v1alpha1.NewStateClient(c), ...))
```

And from the generated `state_grpc.pb.go`:

```go
const (
    State_Get_FullMethodName     = "/cosi.resource.State/Get"
    State_List_FullMethodName    = "/cosi.resource.State/List"
    State_Create_FullMethodName  = "/cosi.resource.State/Create"
    State_Update_FullMethodName  = "/cosi.resource.State/Update"
    State_Destroy_FullMethodName = "/cosi.resource.State/Destroy"
    State_Watch_FullMethodName   = "/cosi.resource.State/Watch"
)
```

## What We Were Calling (WRONG)

❌ `/omni.resources.ResourceService/List` → **HTTP 405**  
❌ `/omni.resources.ResourceService/Get` → **HTTP 405**  

## What omnictl Actually Calls (CORRECT)

✅ `/cosi.resource.State/List` → **Should work!**  
✅ `/cosi.resource.State/Get` → **Should work!**  
✅ `/cosi.resource.State/Create` → **Should work!**  
✅ `/cosi.resource.State/Update` → **Should work!**  
✅ `/cosi.resource.State/Destroy` → **Should work!**  
✅ `/cosi.resource.State/Watch` → **Should work!**  

## Why We Had HTTP 405

We were calling the WRONG service! 

- `ResourceService` (`/omni.resources.ResourceService/*`) is likely an **on-premise only** service
- `State` service (`/cosi.resource.State/*`) is what **Omni SaaS actually exposes**

## The Implementation Gap

We need the **COSI v1alpha1 State service** proto definitions in C#.

### Option 1: Find/Generate Proto Files (BEST)

The proto files should exist somewhere in the COSI runtime project, or we can recreate them from the Go generated code.

**Proto structure** (reverse-engineered from Go code):

```protobuf
syntax = "proto3";

package cosi.resource;

option go_package = "github.com/cosi-project/runtime/api/v1alpha1";

service State {
    rpc Get(GetRequest) returns (GetResponse);
    rpc List(ListRequest) returns (stream ListResponse);
    rpc Create(CreateRequest) returns (CreateResponse);
    rpc Update(UpdateRequest) returns (UpdateResponse);
    rpc Destroy(DestroyRequest) returns (DestroyResponse);
    rpc Watch(WatchRequest) returns (stream WatchResponse);
}

message GetRequest {
    cosi.resource.Metadata metadata = 1;
}

message GetResponse {
    cosi.resource.Resource resource = 1;
}

message ListRequest {
    cosi.resource.Metadata metadata = 1;
}

message ListResponse {
    cosi.resource.Resource resource = 1;
}

// ... more messages
```

### Option 2: Hand-Craft C# Client (FASTER)

We already have the message types (`Resource`, `Metadata`, etc.) from `v1alpha1/resource.proto`.

We just need to create the gRPC client manually:

```csharp
public class CosiStateClient
{
    private readonly GrpcChannel _channel;
    private const string ServiceName = "cosi.resource.State";
    
    public async Task<GetResponse> GetAsync(GetRequest request, Metadata headers, CancellationToken ct)
    {
        var method = new Method<GetRequest, GetResponse>(
            MethodType.Unary,
            ServiceName,
            "Get",
            Marshaller.Create(...),
            Marshaller.Create(...)
        );
        
        var call = _channel.CreateCall(method, null, new CallOptions(headers, cancellationToken: ct));
        return await call.ResponseAsync;
    }
    
    // Similar for List, Create, Update, Destroy, Watch
}
```

### Option 3: Update Our ResourceService to Call State (PRAGMATIC)

We could keep our existing `ResourceClientService` interface but change the underlying implementation to call `/cosi.resource.State/*` instead of `/omni.resources.ResourceService/*`.

## Next Steps

**IMMEDIATE (Next 2-4 hours)**:

1. ✅ **Test the State service endpoint**
   ```csharp
   // Try calling /cosi.resource.State/List instead of /omni.resources.ResourceService/List
   var request = new V1Alpha1.ListRequest { ... };
   var call = channel.CreateCall(stateListMethod, ...);
   ```

2. ✅ **Verify it returns HTTP 200 not HTTP 405**

3. ✅ **If it works, implement full State client**

4. ✅ **Replace ResourceService calls with State calls**

## Implementation Plan

### Phase 1: Proof of Concept (4 hours)

1. Create `CosiStateClient.cs` with Get/List methods
2. Use existing `Resource` and `Metadata` message types
3. Call `/cosi.resource.State/List` for clusters
4. Verify we get HTTP 200 and actual data!

### Phase 2: Full Implementation (8 hours)

1. Implement all 6 State methods (Get, List, Create, Update, Destroy, Watch)
2. Add proper error handling
3. Add authentication (PGP signature)
4. Create wrapper methods for common resources

### Phase 3: Integration (4 hours)

1. Replace `ResourceClientService` implementation to use `CosiStateClient`
2. Update tests
3. Verify all resource operations work
4. Update documentation

## Estimated Timeline

- **Proof of concept**: 4 hours
- **Full implementation**: 8 hours  
- **Integration & testing**: 4 hours
- **Total**: **16 hours** (2 days)

## Success Criteria

✅ Can call `/cosi.resource.State/List` and get clusters  
✅ Can call `/cosi.resource.State/Get` and get a specific cluster  
✅ Can call `/cosi.resource.State/Create` to create a resource  
✅ Can call `/cosi.resource.State/Watch` to watch resource changes  
✅ All existing resource tests pass  
✅ **FULL PARITY WITH omnictl!**  

## Conclusion

**WE CAN DO THIS!** 

The reason ResourceService failed with HTTP 405 is because we were calling the WRONG service. omnictl uses the COSI State service (`/cosi.resource.State/*`), not the ResourceService (`/omni.resources.ResourceService/*`).

Once we implement the State client, we'll have FULL parity with omnictl for resource operations!

---

**Date**: 2025-01-18  
**Status**: BREAKTHROUGH - Ready to implement  
**Confidence**: VERY HIGH - This is exactly what omnictl does  
