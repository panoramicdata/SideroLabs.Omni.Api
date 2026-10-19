# COSI Resource Deserialization Challenge

**Date**: 2025-01-18  
**Status**: 🟡 PARTIAL SOLUTION - Metadata works, Spec needs work  
**Impact**: Medium - Blocks full CRUD, but discovery/monitoring works  

---

## Current Status

### What Works ✅

- ✅ COSI State service responds correctly (no HTTP 405!)
- ✅ Authentication works
- ✅ List operations return resources
- ✅ Get operations return resources  
- ✅ Metadata is fully populated
- ✅ Resource discovery works
- ✅ Watch streaming works (metadata only)

### What's Limited ⚠️

- ⚠️ Resource `Spec` is not deserialized (returns empty/default)
- ⚠️ Resource `Status` is not deserialized (returns empty/default)
- ⚠️ Create/Update operations won't work (need spec)

---

## The Problem

COSI State service returns resources in this format:

```protobuf
message Resource {
    Metadata metadata = 1;  // ✅ This works fine
    Spec spec = 2;          // ❌ This is the problem
}

message Spec {
    bytes proto_spec = 1;      // Binary protobuf data
    string yaml_spec = 2;      // YAML string (may not be populated)
}
```

**What We're Getting**:
- `proto_spec`: Contains binary protobuf encoding of the resource spec
- `yaml_spec`: Usually empty (Omni doesn't populate this)

**What We Need**:
- Either: Proto message definitions for all resource types
- Or: A way to convert `proto_spec` bytes to JSON/objects
- Or: Ask server to return `yaml_spec` instead

---

## Investigation Results

###  Test Output Analysis

From the test run, we can see the actual data:

```
Content Body: \0\0\0\x01∩┐╜
∩┐╜\x03
∩┐╜\x02
\x07default\x12\x18Machines.omni.sidero.dev\x1a$0a531a78...
```

This is **protobuf wire format** - binary encoded data following the protobuf specification.

### Example from Logs

```
managementaddress: fdae:41e4:649b:9303:abb2:cd65:1bbe:ee71
connected: true
usegrpctunnel: false
```

This shows what the **decoded** spec would look like - but we're receiving it as binary.

---

## Solution Options

### Option 1: Generate Proto Definitions for All Resources ⭐ **RECOMMENDED**

**Approach**:
1. Find/create `.proto` files for all Omni resource types
2. Generate C# classes for each resource spec
3. Deserialize `proto_spec` bytes directly to these classes

**Pros**:
- ✅ Type-safe, compile-time checked
- ✅ Fastest performance
- ✅ No runtime parsing overhead
- ✅ Full IDE support

**Cons**:
- ❌ Need proto definitions for 40+ resource types
- ❌ Maintenance burden (keep protos in sync)
- ❌ Larger compiled assembly

**Effort**: 2-3 days

**Status**: **RECOMMENDED** - Best long-term solution

---

### Option 2: Use Dynamic Protobuf Parsing

**Approach**:
1. Parse `proto_spec` bytes using protobuf descriptor
2. Convert to dynamic object or JSON
3. Map to our existing resource classes

**Pros**:
- ✅ No need for proto files
- ✅ Handles unknown resource types
- ✅ Flexible

**Cons**:
- ❌ Complex implementation
- ❌ Slower runtime performance
- ❌ No type safety
- ❌ Difficult debugging

**Effort**: 3-4 days

**Status**: **NOT RECOMMENDED** - Too complex for benefit

---

### Option 3: Request YAML Format from Server

**Approach**:
1. Add option to request `yaml_spec` instead of `proto_spec`
2. Parse YAML to our resource classes
3. Use existing YAML deserializer

**Pros**:
- ✅ Simple, clean solution
- ✅ Use existing YAML serialization
- ✅ Human-readable for debugging

**Cons**:
- ❌ Depends on server support (may not be available)
- ❌ Larger payload size
- ❌ Slower than protobuf

**Effort**: 1 day (if server supports it)

**Status**: **INVESTIGATE** - Check if Omni supports this

---

### Option 4: Hybrid - Use omnictl's Approach

**Approach**:
1. Study how omnictl deserializes resources
2. Use the same library/technique they use
3. Integrate into our client

**Pros**:
- ✅ Proven to work
- ✅ Same behavior as omnictl
- ✅ Community-vetted approach

**Cons**:
- ❌ May require Go libraries (hard in .NET)
- ❌ Tight coupling to omnictl implementation

**Effort**: 2-3 days

**Status**: **INVESTIGATE** - See omnictl source code

---

## Immediate Next Steps

### 1. Investigate omnictl's Deserialization (1-2 hours)

Look at:
```go
// From omnictl source
func (client *Client) State() state.State {
    return client.state
}

// How does it deserialize resources?
resource := client.State().Get(ctx, ...)
// What does this return?
```

**Goal**: Understand their deserialization strategy

### 2. Check for Proto Definitions (30 mins)

Search Omni repo for:
- `*.proto` files in `/api/` or `/pkg/`
- Resource spec definitions
- Message definitions for Cluster, Machine, etc.

**Locations to check**:
- `github.com/siderolabs/omni/api/`
- `github.com/siderolabs/omni/client/api/omni/specs/`

###  3. Test YAML Request Option (1 hour)

Try requesting resources with different options:
```csharp
// Try adding options to ListRequest
var request = new ListRequest
{
    Namespace = "default",
    Type = "Clusters.omni.sidero.dev",
    Options = new ListOptions
    {
        // Try different options here
    }
};
```

**Goal**: See if we can get YAML instead of proto

---

## Workaround for Now

**Current Implementation** (Temporary):

```csharp
private TResource DeserializeResource<TResource>(Cosi.Resource.Resource cosiResource)
{
    var resource = new TResource();
    
    // ✅ Populate metadata (this works!)
    resource.Metadata = ResourceMetadata.FromProto(cosiResource.Metadata);
    
    // ❌ Spec remains empty/default (known limitation)
    _logger.LogWarning("Resource spec not yet deserialized - metadata only");
    
    return resource;
}
```

**What This Enables**:
- ✅ Resource discovery (`ListAsync` for all resources)
- ✅ Resource monitoring (`WatchAsync` for change events)
- ✅ Cluster inventory
- ✅ Machine inventory
- ✅ Resource existence checks

**What This Blocks**:
- ❌ Create resources (need spec)
- ❌ Update resources (need spec)
- ❌ Read spec details (kubernetes version, config, etc.)

---

## Impact Assessment

### High Priority Operations (BLOCKED)

- ❌ Create cluster
- ❌ Update cluster
- ❌ Apply configuration
- ❌ Template rendering (needs spec data)

### Medium Priority Operations (PARTIAL)

- ⚠️ List clusters (metadata only, no k8s version)
- ⚠️ Get cluster (metadata only, no config)
- ⚠️ Watch changes (know what changed, not details)

### Low Priority Operations (WORKING)

- ✅ Check if cluster exists
- ✅ Get cluster ID/name
- ✅ Get cluster namespace
- ✅ Get cluster labels
- ✅ Monitor cluster creation/deletion events

---

## Recommended Path Forward

### Phase 1: Investigation (TODAY)

1. **Check omnictl source** for deserialization approach
2. **Search Omni repo** for proto definitions
3. **Test YAML option** (if available)

**Time**: 2-3 hours  
**Goal**: Identify best solution

### Phase 2: Implement Solution (THIS WEEK)

**If proto files found**:
- Generate C# classes
- Implement proto deserialization
- Test with real resources

**If YAML option works**:
- Update request to ask for YAML
- Test YAML deserialization
- Verify all fields map correctly

**If neither**:
- Implement basic proto parsing for core types only
- Focus on Cluster and Machine first
- Expand to other types as needed

**Time**: 2-4 days  
**Goal**: Full spec access for at least Cluster and Machine

### Phase 3: Complete Testing (NEXT WEEK)

- Full CRUD tests with actual spec data
- Validation that create/update works
- Template operations
- High-level operations

**Time**: 3-5 days  
**Goal**: Production-ready

---

## Success Criteria

### Minimum Viable (Current) ✅

- [x] COSI State endpoint works
- [x] List operations return resources
- [x] Metadata is accessible
- [x] 8/8 basic operation tests pass

### Production Ready (Target)

- [ ] Spec deserialization working
- [ ] Create/Update operations functional
- [ ] All resource fields accessible
- [ ] Template operations working
- [ ] 95%+ test coverage

---

## Conclusion

We've made **major progress** by discovering and implementing the correct COSI State endpoint. The current limitation (spec deserialization) is a **solvable technical challenge**, not a fundamental blocker.

**Options ranked**:
1. ⭐ Generate proto definitions (best long-term)
2. 🔍 Investigate omnictl approach (learn from experts)
3. 🧪 Test YAML option (quick win if available)
4. ❌ Dynamic parsing (too complex)

**Recommendation**: Spend 2-3 hours investigating omnictl and looking for proto files. This will give us the data needed to choose the best path forward.

**Timeline**:
- Today: Investigation
- This week: Implement solution
- Next week: Complete testing
- Target: Production-ready in 1-2 weeks

---

**Status**: 🟡 **PARTIAL SUCCESS** - Core functionality works, spec access in progress!

**Confidence**: ✅ **HIGH** - We know the endpoint works, just need to handle the data format correctly!

