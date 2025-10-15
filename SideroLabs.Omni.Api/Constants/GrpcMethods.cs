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
	internal const string ValidateJsonSchema = "/management.ManagementService/ValidateJSONSchema";
	internal const string KubernetesUpgradePreChecks = "/management.ManagementService/KubernetesUpgradePreChecks";
	internal const string CreateSchematic = "/management.ManagementService/CreateSchematic";
	internal const string GetSupportBundle = "/management.ManagementService/GetSupportBundle";
	internal const string ReadAuditLog = "/management.ManagementService/ReadAuditLog";
	internal const string MaintenanceUpgrade = "/management.ManagementService/MaintenanceUpgrade";
	internal const string GetMachineJoinConfig = "/management.ManagementService/GetMachineJoinConfig";
	internal const string CreateJoinToken = "/management.ManagementService/CreateJoinToken";
	internal const string TearDownLockedCluster = "/management.ManagementService/TearDownLockedCluster";
	internal const string MachineLogs = "/management.ManagementService/MachineLogs";
	internal const string KubernetesSyncManifests = "/management.ManagementService/KubernetesSyncManifests";
}
