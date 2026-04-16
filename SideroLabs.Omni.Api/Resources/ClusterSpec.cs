namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Defines the desired state for an Omni cluster resource.
/// </summary>
public class ClusterSpec
{
	/// <summary>
	/// Gets or sets the desired Kubernetes version in <c>vX.Y.Z</c> form.
	/// </summary>
	public string KubernetesVersion { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the desired Talos version in <c>vX.Y.Z</c> form.
	/// </summary>
	public string TalosVersion { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets cluster networking configuration, including pod and service CIDRs.
	/// </summary>
	public NetworkConfig? Network { get; set; }

	/// <summary>
	/// Gets or sets control-plane placement descriptors used by Omni scheduling.
	/// </summary>
	public List<object> ControlPlane { get; set; } = [];

	/// <summary>
	/// Gets or sets worker placement descriptors used by Omni scheduling.
	/// </summary>
	public List<object> Workers { get; set; } = [];
}
