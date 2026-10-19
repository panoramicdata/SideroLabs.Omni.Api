# Plan: Achieve Full Parity with omnictl

## Executive Summary

After deep investigation of the `omnictl` source code and Omni server architecture, here's the **ACTUAL** way to achieve parity with omnictl functionality.

## Key Discoveries

### 1. omnictl Architecture

The omnictl client has **THREE** separate API surfaces, all using the **SAME** gRPC connection:

```go
// From client/pkg/client/client.go
type Client struct {
    conn *grpc.ClientConn  // Single gRPC connection
}

func (c *Client) Omni() *omni.Client           // COSI resource operations
func (c *Client) Management() *management.Client  // Management operations
func (c *Client) OIDC() *oidc.Client            // OIDC operations
```

### 2. The COSI State API

The `Omni()` client wraps the **COSI v1alpha1 State service**:

```go
// From client/pkg/client/omni/omni.go
c.state = state.WrapCore(
    client.NewAdapter(
        v1alpha1.NewStateClient(c),  // <-- THIS is the key!
        ...
    )
)
```

This means `omnictl get clusters` is NOT calling `/omni.resources.ResourceService/List`.

Instead, it's calling the **COSI State API**, which is a DIFFERENT gRPC service!

### 3. Our Current Implementation Gap

We currently have:
- ✅ `ManagementService` client - WORKS
- ✅ `ResourceService` client - Returns HTTP 405 on Omni SaaS
- ❌ **COSI State API client** - **MISSING!** This is what we need!

## The Solution

### Option 1: Implement COSI State API Client (RECOMMENDED)

**What**: Create a C# wrapper around the COSI `v1alpha1.State` gRPC service

**Why**: This is what `omnictl` actually uses for resource operations

**How**:
1. We already have the proto files in `Protos/v1alpha1/resource.proto`
2. Need to find/add the COSI State service proto definition
3. Generate C# client from the proto
4. Wrap it with a friendly .NET API

**Proto Service** (need to confirm exact definition):
```protobuf
service State {
    rpc Get(GetRequest) returns (GetResponse);
    rpc List(ListRequest) returns (stream ListResponse);
    rpc Create(CreateRequest) returns (CreateResponse);
    rpc Update(UpdateRequest) returns (UpdateResponse);
    rpc Destroy(DestroyRequest) returns (DestroyResponse);
    rpc Watch(WatchRequest) returns (stream WatchResponse);
}
```

**Endpoint**: Likely `/cosi.resource.v1alpha1.State/*` or similar

### Option 2: Use gRPC-Gateway REST API

**What**: Use the HTTP/REST API that wraps the gRPC services

**Evidence**: Found `resources.pb.gw.go` - indicates gRPC-Gateway is used

**Potential Endpoints**:
- `GET /api/omni/resources/{type}/{id}` - Get resource
- `GET /api/omni/resources/{type}` - List resources
- `POST /api/omni/resources/{type}` - Create resource
- etc.

**How**:
1. Discover the actual REST API endpoints (might be documented)
2. Create HTTP client wrapper
3. Handle authentication (same PGP signature mechanism)

### Option 3: Hybrid Approach (MOST PRAGMATIC)

**What**: Use both gRPC (ManagementService) and REST API (Resources)

**Why**: 
- ManagementService already works via gRPC
- ResourceService might be available via REST even if gRPC is blocked

## Investigation Needed

### 1. Find the COSI State Proto Definition

**Where to look**:
```bash
# Check if we already have it
Get-ChildItem "SideroLabs.Omni.Api\Protos" -Recurse -Filter "state.proto"

# Check COSI runtime repository
Get-ChildItem "C:\Users\DavidBond\go\pkg\mod\github.com\cosi-project\runtime*" -Recurse -Filter "*.proto"
```

### 2. Test REST API Availability

**Test if REST API works**:
```csharp
// Try HTTP GET instead of gRPC POST
var request = new HttpRequestMessage(
    HttpMethod.Get,
    "https://panoramicdata.eu-central-1.omni.siderolabs.io/api/omni/resources/clusters"
);

// Add PGP authentication headers
request.Headers.Add("x-sidero-timestamp", timestamp);
request.Headers.Add("x-sidero-signature", signature);

var response = await httpClient.SendAsync(request);
```

### 3. Examine omnictl Network Traffic

**Use Wireshark or Fiddler to capture**:
```bash
# Enable gRPC logging
$env:GRPC_GO_LOG_VERBOSITY_LEVEL = "99"
$env:GRPC_GO_LOG_SEVERITY_LEVEL = "info"

# Run omnictl and capture traffic
omnictl get clusters --debug
```

This will show the ACTUAL gRPC method being called.

## Recommended Implementation Plan

### Phase 1: Discovery (2-4 hours)

1. ✅ **Find COSI State proto definition**
   - Check our Protos directory
   - Check COSI runtime repo
   - Download if needed

2. ✅ **Capture omnictl network traffic**
   - Run `omnictl get clusters` with gRPC debugging
   - See which gRPC method is actually called
   - Verify it's NOT `/omni.resources.ResourceService/List`

3. ✅ **Test REST API**
   - Try HTTP GET to potential REST endpoints
   - Check for HTTP 200 vs HTTP 405
   - Document working endpoints

### Phase 2: Implementation (8-12 hours)

#### If COSI State API is the answer:

1. **Add COSI State proto** (if missing)
   ```csharp
   // Add to Protos/v1alpha1/state.proto
   ```

2. **Generate C# client**
   ```xml
   <!-- Add to SideroLabs.Omni.Api.csproj -->
   <Protobuf Include="Protos\v1alpha1\state.proto" />
   ```

3. **Create StateClient wrapper**
   ```csharp
   public class OmniStateClient : IOmniStateClient
   {
       private readonly State.StateClient _client;
       private readonly OmniAuthenticator _authenticator;
       
       public async Task<T> GetAsync<T>(string id, string ns, CancellationToken ct)
       {
           var request = new GetRequest { Id = id, Namespace = ns };
           var headers = new Metadata();
           _authenticator.SignRequest(headers, "/cosi.resource.v1alpha1.State/Get");
           
           var response = await _client.GetAsync(request, headers, cancellationToken: ct);
           return Deserialize<T>(response);
       }
   }
   ```

4. **Update OmniClient**
   ```csharp
   public class OmniClient : IOmniClient
   {
       public IManagementService Management { get; }
       public IOmniStateClient State { get; }  // NEW!
   }
   ```

#### If REST API is the answer:

1. **Create HTTP client wrapper**
   ```csharp
   public class OmniRestClient : IOmniResourceClient
   {
       private readonly HttpClient _httpClient;
       private readonly OmniAuthenticator _authenticator;
       
       public async Task<T> GetAsync<T>(string id, string ns, CancellationToken ct)
       {
           var url = $"/api/omni/resources/{GetResourceType<T>()}/{id}";
           var request = new HttpRequestMessage(HttpMethod.Get, url);
           
           // Sign the request
           _authenticator.SignHttpRequest(request, url);
           
           var response = await _httpClient.SendAsync(request, ct);
           response.EnsureSuccessStatusCode();
           
           var json = await response.Content.ReadAsStringAsync(ct);
           return JsonSerializer.Deserialize<T>(json);
       }
   }
   ```

2. **Adapt authentication for HTTP**
   ```csharp
   public class OmniAuthenticator
   {
       public void SignHttpRequest(HttpRequestMessage request, string path)
       {
           var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
           var payload = new { path, method = request.Method.Method };
           var signature = SignPayload(payload);
           
           request.Headers.Add("x-sidero-timestamp", timestamp.ToString());
           request.Headers.Add("x-sidero-payload", JsonSerializer.Serialize(payload));
           request.Headers.Add("x-sidero-signature", signature);
       }
   }
   ```

### Phase 3: Testing & Validation (4-6 hours)

1. **Create integration tests**
   ```csharp
   [Fact]
   public async Task State_GetCluster_Success()
   {
       var cluster = await OmniClient.State.GetAsync<Cluster>("cluster-1", "default", CancellationToken);
       Assert.NotNull(cluster);
   }
   ```

2. **Verify parity with omnictl**
   - `omnictl get clusters` → `client.State.ListAsync<Cluster>()`
   - `omnictl get cluster cluster-1` → `client.State.GetAsync<Cluster>("cluster-1")`
   - etc.

3. **Update documentation**
   - Document which API is used for what
   - Update README with State API examples
   - Add migration guide from ResourceService

## Expected Outcomes

### If COSI State API works:

✅ **Full parity with omnictl** for resource operations  
✅ **Native gRPC** - fast, efficient, type-safe  
✅ **Proper authentication** - same PGP signature mechanism  
✅ **Streaming support** - watch operations work  

### If REST API works:

✅ **Full parity with omnictl** for resource operations  
⚠️ **HTTP-based** - slightly slower but works  
✅ **Proper authentication** - same PGP signature  
⚠️ **Streaming via SSE** - might need different approach for watch  

## CRITICAL UPDATE - Investigation Results

After investigating the omnictl source code and Omni server architecture:

### Key Finding #1: omnictl Uses COSI Runtime Adapter

`omnictl` doesn't directly call gRPC services. It uses:
```go
c.state = state.WrapCore(client.NewAdapter(v1alpha1.NewStateClient(c)))
```

This is a **Go-specific COSI runtime abstraction layer** that:
- Wraps the gRPC client connection
- Implements the COSI `state.State` interface
- Translates COSI operations to gRPC calls

The `v1alpha1.State` service is from `github.com/cosi-project/runtime`, NOT from Omni!

### Key Finding #2: On-Premise vs SaaS Architecture

**On-Premise Omni** probably:
- ✅ Exposes ResourceService via gRPC
- ✅ Allows direct COSI State access
- ✅ Full gRPC API surface

**Omni SaaS**:
- ✅ ManagementService - WORKS via gRPC
- ❌ ResourceService - Returns HTTP 405 (blocked)
- ❓ COSI State service - Unknown (likely also blocked)

### Key Finding #3: The REAL Question

The question isn't "how does omnictl work" - it's **"how can we access resources on Omni SaaS?"**

Possible answers:
1. **ManagementService has resource operations we haven't discovered**
2. **REST API endpoints exist** (gRPC-Gateway)
3. **WebSocket/GraphQL API** (for the web UI)
4. **It's literally impossible** - SaaS locks down resource access

## Revised Next Steps

**IMMEDIATE INVESTIGATION** (next 2 hours):

1. ✅ **Examine the web UI network traffic**
   - Open Omni SaaS in browser
   - Use DevTools Network tab
   - Click through clusters, machines, etc.
   - See what API endpoints the UI calls

2. ✅ **Check for GraphQL/REST APIs**
   - Look for `/api/`, `/graphql`, `/query` endpoints
   - Check HTTP headers for API version
   - Document actual working endpoints

3. ✅ **Review ManagementService proto more carefully**
   - Look for undiscovered resource operations
   - Check for generic "query" methods
   - See if there are resource management RPCs

4. ⚠️ **Contact Sidero Labs**
   - Ask directly: "How do we access cluster/machine resources via API on SaaS?"
   - May need different authentication for resource access
   - Might be intentionally locked down for security

**Then implement**: Whatever actually works on SaaS

## Conclusion

The HTTP 405 issue is a **red herring**. The real issue is that we're calling the **WRONG service**.

omnictl uses:
- **COSI State API** (v1alpha1.State service) for resource operations
- **NOT** ResourceService (/omni.resources.ResourceService)

Once we implement the COSI State API client, we'll achieve full parity with omnictl.

---

**Status**: Investigation phase - need to confirm which approach works on Omni SaaS  
**Priority**: HIGH - this unblocks all resource operations  
**Estimated Time**: 14-22 hours total  
