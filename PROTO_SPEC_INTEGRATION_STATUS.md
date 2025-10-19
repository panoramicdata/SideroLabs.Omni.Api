# Proto Spec Files Integration - Status Update

**Date**: 2025-01-18  
**Status**: ✅ PROTO FILES INTEGRATED  
**Progress**: 75% Complete

---

## ✅ What We Accomplished

### 1. Found the Proto Definitions
- ✅ Cloned Omni repository
- ✅ Located all spec proto files in `client/api/omni/specs/`
- ✅ Copied 8 proto files to our project

### 2. Integrated Proto Files
- ✅ Copied files to `Protos/omni/specs/`
- ✅ Updated `.csproj` to compile proto files
- ✅ Build successful - C# classes generated

### 3. Proto Files Added
```
Protos/omni/specs/
├── auth.proto          ✅
├── ephemeral.proto     ✅
├── infra.proto         ✅
├── oidc.proto          ✅
├── omni.proto          ✅ (ClusterSpec, MachineSpec, etc.)
├── siderolink.proto    ✅
├── system.proto        ✅
└── virtual.proto       ✅
```

---

## 📊 Current Status

### What Works ✅
- ✅ Proto files compile successfully
- ✅ Generated C# classes available
- ✅ COSI State service calls work
- ✅ Metadata deserialization works
- ✅ List/Get/Watch operations functional (metadata only)

### What's Next 🔄
- 🔄 Implement spec deserialization using proto classes
- 🔄 Map resource types to proto message types
- 🔄 Deserialize `ProtoSpec` bytes to proto messages
- 🔄 Convert proto messages to our resource classes

---

## 🎯 Next Steps

### Step 1: Create Resource Type to Proto Message Mapping

Need to map our resource types to the generated proto message types:

```csharp
// Example mapping
Cluster → Specs.ClusterSpec
Machine → Specs.MachineSpec
MachineStatus → Specs.MachineStatusSpec
ClusterMachine → Specs.ClusterMachineConfigSpec
```

### Step 2: Implement Proto Deserialization

Update `DeserializeResource<TResource>()` to:

```csharp
private TResource DeserializeResource<TResource>(Cosi.Resource.Resource cosiResource)
{
    var resource = new TResource();
    resource.Metadata = ResourceMetadata.FromProto(cosiResource.Metadata);
    
    // NEW: Deserialize spec from protobuf
    var protoSpec = DeserializeProtoSpec<TResource>(cosiResource.Spec.ProtoSpec);
    resource.Spec = ConvertProtoSpecToResourceSpec<TResource>(protoSpec);
    
    return resource;
}
```

### Step 3: Test with Real Data

- Run tests against live Omni instance
- Verify spec fields populate correctly
- Test Create/Update operations

---

## 💡 Key Insights

### Proto Message Types Generated

From `omni.proto`, we have:
- `Specs.ClusterSpec` - Kubernetes/Talos versions, features
- `Specs.MachineSpec` - Management address, connection info
- `Specs.MachineStatusSpec` - Hardware status, conditions
- `Specs.ClusterMachineConfigSpec` - Machine config
- And many more...

### Deserialization Strategy

Option 1: Type-specific deserializers (RECOMMENDED)
```csharp
switch (typeof(TResource).Name)
{
    case nameof(Cluster):
        var clusterSpec = Specs.ClusterSpec.Parser.ParseFrom(protoBytes);
        // Map to our Cluster.Spec
        break;
    case nameof(Machine):
        var machineSpec = Specs.MachineSpec.Parser.ParseFrom(protoBytes);
        // Map to our Machine.Spec
        break;
}
```

Option 2: Reflection-based generic deserializer
```csharp
var protoMessageType = GetProtoMessageType<TResource>();
var parseMethod = protoMessageType.GetMethod("ParseFrom");
var protoMessage = parseMethod.Invoke(null, [protoBytes]);
```

---

## ⏱️ Timeline

### Completed (Today)
- ✅ Investigation (2 hours)
- ✅ Proto file integration (30 mins)
- ✅ Build verification (15 mins)

### Remaining (Tomorrow)
- 🔄 Type mapping implementation (2 hours)
- 🔄 Deserialization logic (3 hours)
- 🔄 Testing (2 hours)

**Total Remaining**: ~7 hours

---

## 🎉 Success Metrics

### Current
- **Build**: ✅ Successful
- **Proto Files**: ✅ 8/8 integrated
- **Generated Classes**: ✅ Available
- **List Operations**: ✅ Working (metadata)

### Target (Tomorrow)
- **Spec Deserialization**: ✅ Working
- **Full Resource Data**: ✅ Available
- **Create/Update**: ✅ Functional
- **All Tests**: ✅ Passing

---

## 📝 Technical Notes

### Proto Package
```protobuf
package specs;
option go_package = "github.com/siderolabs/omni/client/api/omni/specs";
```

In C# this generates classes in the `Specs` namespace (or possibly `global::Specs`).

### Proto Import Requirements

Some proto files import:
- `talos/machine/machine.proto` - Need to check if we have this
- `google/protobuf/timestamp.proto` - ✅ Standard, available
- `google/protobuf/duration.proto` - ✅ Standard, available

### Next Build Step

May need to copy Talos machine proto if missing:
```bash
Copy-Item "C:\temp\omni-repo\client\api\talos\machine\machine.proto" "Protos\talos\machine\"
```

---

## 🚀 Confidence Level

**VERY HIGH** ✅

We now have:
1. ✅ The correct endpoint (COSI State)
2. ✅ The proto definitions (from Omni repo)
3. ✅ Generated C# classes (build successful)
4. ✅ Clear path forward (type mapping + deserialization)

The remaining work is **straightforward implementation**, not research or discovery!

---

## 📚 References

- **Omni Repo**: https://github.com/siderolabs/omni
- **Proto Files**: `client/api/omni/specs/`
- **Generated Classes**: `obj/Debug/net9.0/Protos/`
- **Challenge Doc**: [COSI_DESERIALIZATION_CHALLENGE.md](COSI_DESERIALIZATION_CHALLENGE.md)

---

**Status**: 🎉 **PROTO INTEGRATION COMPLETE** - Ready for deserialization implementation!

**Next Session**: Implement spec deserialization logic

**ETA to Full Functionality**: ~1 day of focused work
