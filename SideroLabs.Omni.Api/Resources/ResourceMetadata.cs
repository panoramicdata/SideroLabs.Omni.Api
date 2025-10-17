namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Resource metadata wrapper for COSI resource metadata
/// </summary>
public class ResourceMetadata
{
	/// <summary>
	/// Resource namespace (default: "default")
	/// </summary>
	public string Namespace { get; set; } = "default";

	/// <summary>
	/// Resource type identifier
	/// </summary>
	public string Type { get; set; } = "";

	/// <summary>
	/// Unique resource ID within namespace and type
	/// </summary>
	public string Id { get; set; } = "";

	/// <summary>
	/// Resource version for optimistic concurrency control
	/// </summary>
	public string Version { get; set; } = "";

	/// <summary>
	/// Owner controller name (for controller-managed resources)
	/// </summary>
	public string Owner { get; set; } = "";

	/// <summary>
	/// Resource phase (running, tearing down, etc.)
	/// </summary>
	public string Phase { get; set; } = "";

	/// <summary>
	/// Creation timestamp
	/// </summary>
	public DateTimeOffset? Created { get; set; }

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTimeOffset? Updated { get; set; }

	/// <summary>
	/// Finalizers - controllers blocking teardown
	/// </summary>
	public List<string> Finalizers { get; set; } = [];

	/// <summary>
	/// Labels - queryable key-value pairs
	/// </summary>
	public Dictionary<string, string> Labels { get; set; } = [];

	/// <summary>
	/// Annotations - non-queryable metadata
	/// </summary>
	public Dictionary<string, string> Annotations { get; set; } = [];

	/// <summary>
	/// Resource name (alias for Id)
	/// </summary>
	public string Name
	{
		get => Id;
		set => Id = value;
	}

	/// <summary>
	/// Converts proto metadata to ResourceMetadata
	/// </summary>
	public static ResourceMetadata FromProto(Cosi.Resource.Metadata proto)
	{
		return new ResourceMetadata
		{
			Namespace = proto.Namespace,
			Type = proto.Type,
			Id = proto.Id,
			Version = proto.Version,
			Owner = proto.Owner,
			Phase = proto.Phase,
			Created = proto.Created?.ToDateTimeOffset(),
			Updated = proto.Updated?.ToDateTimeOffset(),
			Finalizers = [.. proto.Finalizers],
			Labels = new Dictionary<string, string>(proto.Labels),
			Annotations = new Dictionary<string, string>(proto.Annotations)
		};
	}

	/// <summary>
	/// Converts ResourceMetadata to proto metadata
	/// </summary>
	public Cosi.Resource.Metadata ToProto()
	{
		var proto = new Cosi.Resource.Metadata
		{
			Namespace = Namespace,
			Type = Type,
			Id = Id,
			Version = Version,
			Owner = Owner,
			Phase = Phase
		};

		if (Created.HasValue)
		{
			proto.Created = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(Created.Value);
		}

		if (Updated.HasValue)
		{
			proto.Updated = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(Updated.Value);
		}

		proto.Finalizers.AddRange(Finalizers);
		foreach (var kvp in Labels)
		{
			proto.Labels[kvp.Key] = kvp.Value;
		}

		foreach (var kvp in Annotations)
		{
			proto.Annotations[kvp.Key] = kvp.Value;
		}

		return proto;
	}
}
