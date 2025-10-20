namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for the OmniClient to support dependency injection
/// </summary>
public interface IOmniClient : IDisposable
{
	// === Low-Level Resource Access ===
	
	/// <summary>
	/// Gets the Resource Client for COSI resource operations
	/// Uses the COSI v1alpha1 State service (/cosi.resource.State/*)
	/// </summary>
	IOmniResourceClient Resources { get; }

	// === Resource-Specific Operations ===
	
	/// <summary>
	/// Cluster-specific operations (status, create, delete, machine lock/unlock)
	/// </summary>
	IClusterOperations Clusters { get; }

	/// <summary>
	/// Machine-specific operations (list, get, create, update, delete, lock/unlock)
	/// </summary>
	IMachineOperations Machines { get; }

	/// <summary>
	/// ClusterMachine-specific operations (list, get, create, update, delete)
	/// </summary>
	IClusterMachineOperations ClusterMachines { get; }

	/// <summary>
	/// MachineSet-specific operations (list, get, create, update, delete)
	/// </summary>
	IMachineSetOperations MachineSets { get; }

	/// <summary>
	/// MachineSetNode-specific operations (list, get, create, update, delete)
	/// </summary>
	IMachineSetNodeOperations MachineSetNodes { get; }

	/// <summary>
	/// MachineClass-specific operations (list, get, create, update, delete)
	/// </summary>
	IMachineClassOperations MachineClasses { get; }

	/// <summary>
	/// ConfigPatch-specific operations (list, get, create, update, delete)
	/// </summary>
	IConfigPatchOperations ConfigPatches { get; }

	/// <summary>
	/// ExtensionsConfiguration-specific operations (list, get, create, update, delete)
	/// </summary>
	IExtensionsConfigurationOperations ExtensionsConfigurations { get; }

	/// <summary>
	/// TalosConfig resource operations (list, get, create, update, delete)
	/// </summary>
	ITalosConfigOperations TalosConfigs { get; }

	/// <summary>
	/// LoadBalancerConfig operations (list, get, create, update, delete)
	/// </summary>
	ILoadBalancerOperations LoadBalancers { get; }

	/// <summary>
	/// ControlPlane-specific operations (list, get, create, update, delete)
	/// </summary>
	IControlPlaneOperations ControlPlanes { get; }

	/// <summary>
	/// KubernetesNode-specific operations (list, get, create, update, delete)
	/// </summary>
	IKubernetesNodeOperations KubernetesNodes { get; }

	/// <summary>
	/// Identity-specific operations (list, get, create, update, delete)
	/// </summary>
	IIdentityOperations Identities { get; }

	/// <summary>
	/// User management operations (list, create, delete, setRole)
	/// </summary>
	IUserManagement Users { get; }

	/// <summary>
	/// Template operations (rendering, sync, export, diff)
	/// </summary>
	ITemplateOperations Templates { get; }

	// === Management Services ===

	/// <summary>
	/// Kubernetes configuration service
	/// </summary>
	IKubeConfigService KubeConfig { get; }

	/// <summary>
	/// Talos configuration service
	/// </summary>
	ITalosConfigService TalosConfig { get; }

	/// <summary>
	/// Omni configuration service
	/// </summary>
	IOmniConfigService OmniConfig { get; }

	/// <summary>
	/// Service account management
	/// </summary>
	IServiceAccountService ServiceAccounts { get; }

	/// <summary>
	/// Validation operations
	/// </summary>
	IValidationService Validation { get; }

	/// <summary>
	/// Kubernetes operations
	/// </summary>
	IKubernetesService Kubernetes { get; }

	/// <summary>
	/// Machine schematic operations
	/// </summary>
	ISchematicService Schematics { get; }

	/// <summary>
	/// Machine management operations (logs, upgrades, join config)
	/// </summary>
	IMachineService MachineManagement { get; }

	/// <summary>
	/// Support and audit operations
	/// </summary>
	ISupportService Support { get; }

	// === Legacy (Deprecated) ===

	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// </summary>
	/// <remarks>
	/// ⚠️ DEPRECATED: Use specific services like KubeConfig, ServiceAccounts, etc. instead.
	/// This property is maintained for backward compatibility but will be removed in a future version.
	/// </remarks>
	[Obsolete("Use specific services like KubeConfig, ServiceAccounts, Validation, etc. instead of the monolithic Management service.")]
	IManagementService Management { get; }

	// === Client Properties ===

	/// <summary>
	/// Gets the gRPC endpoint URL
	/// </summary>
	Uri BaseUrl { get; }

	/// <summary>
	/// Gets whether TLS is enabled
	/// </summary>
	bool UseTls { get; }

	/// <summary>
	/// Gets whether the client is in read-only mode
	/// </summary>
	bool IsReadOnly { get; }

	/// <summary>
	/// Gets the authentication identity if available
	/// </summary>
	string? Identity { get; }
}


