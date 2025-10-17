namespace SideroLabs.Omni.Api.Resources;

public class ClusterSpec
{
	public string KubernetesVersion { get; set; } = string.Empty;
	public string TalosVersion { get; set; } = string.Empty;
	public NetworkConfig? Network { get; set; }
	public List<object> ControlPlane { get; set; } = [];
	public List<object> Workers { get; set; } = [];
}
