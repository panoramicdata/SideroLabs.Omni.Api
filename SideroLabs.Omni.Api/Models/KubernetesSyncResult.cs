using SideroLabs.Omni.Api.Enums;

namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Kubernetes manifest sync result
/// </summary>
public class KubernetesSyncResult
{
	/// <summary>
	/// Gets or sets the sync event kind emitted by Omni, such as apply, skip, or delete.
	/// </summary>
	public SyncType ResponseType { get; set; }

	/// <summary>
	/// Gets or sets the manifest path associated with the sync event.
	/// </summary>
	public string Path { get; set; } = "";

	/// <summary>
	/// Gets or sets the raw manifest payload returned by the Kubernetes sync stream.
	/// </summary>
	public byte[] Object { get; set; } = [];

	/// <summary>
	/// Gets or sets the human-readable diff for the manifest update when available.
	/// </summary>
	public string Diff { get; set; } = "";

	/// <summary>
	/// Gets or sets a value indicating whether Omni skipped applying this manifest.
	/// </summary>
	public bool Skipped { get; set; }
}
