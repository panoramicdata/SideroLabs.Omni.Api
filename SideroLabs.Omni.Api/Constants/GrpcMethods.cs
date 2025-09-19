namespace SideroLabs.Omni.Api.Constants;

/// <summary>
/// Constants for gRPC method paths
/// </summary>
internal static class GrpcMethods
{
	internal const string Kubeconfig = "/management.ManagementService/Kubeconfig";
	internal const string Talosconfig = "/management.ManagementService/Talosconfig";
	internal const string Omniconfig = "/management.ManagementService/Omniconfig";
	internal const string CreateServiceAccount = "/management.ManagementService/CreateServiceAccount";
	internal const string ListServiceAccounts = "/management.ManagementService/ListServiceAccounts";
	internal const string RenewServiceAccount = "/management.ManagementService/RenewServiceAccount";
	internal const string DestroyServiceAccount = "/management.ManagementService/DestroyServiceAccount";
	internal const string ValidateConfig = "/management.ManagementService/ValidateConfig";
	internal const string KubernetesUpgradePreChecks = "/management.ManagementService/KubernetesUpgradePreChecks";
	internal const string CreateSchematic = "/management.ManagementService/CreateSchematic";
	internal const string MachineLogs = "/management.ManagementService/MachineLogs";
	internal const string KubernetesSyncManifests = "/management.ManagementService/KubernetesSyncManifests";
}
