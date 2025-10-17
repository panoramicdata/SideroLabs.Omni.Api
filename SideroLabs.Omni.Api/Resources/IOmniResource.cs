namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Base interface for all Omni resources
/// </summary>
public interface IOmniResource
{
	/// <summary>
	/// Resource metadata containing namespace, type, id, version, labels, etc.
	/// </summary>
	ResourceMetadata Metadata { get; set; }

	/// <summary>
	/// Resource kind (e.g., "Cluster", "Machine")
	/// </summary>
	string Kind { get; }

	/// <summary>
	/// API version (e.g., "omni.sidero.dev/v1alpha1")
	/// </summary>
	string ApiVersion { get; }

	/// <summary>
	/// Converts the resource to YAML format
	/// </summary>
	string ToYaml();

	/// <summary>
	/// Converts the resource to JSON format
	/// </summary>
	string ToJson();
}
