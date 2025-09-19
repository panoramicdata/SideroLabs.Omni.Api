namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Kubernetes manifest sync result
/// </summary>
public class KubernetesSyncResult
{
	public enum SyncType
	{
		Unknown = 0,
		Manifest = 1,
		Rollout = 2
	}

	public SyncType ResponseType { get; set; }
	public string Path { get; set; } = "";
	public byte[] Object { get; set; } = [];
	public string Diff { get; set; } = "";
	public bool Skipped { get; set; }
}
