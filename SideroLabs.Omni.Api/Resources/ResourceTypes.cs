namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Static class for registering core Omni resource types
/// Call Initialize() at application startup to register all resource types
/// </summary>
public static class ResourceTypes
{
	private static bool _initialized;
	private static readonly object _lock = new();

	/// <summary>
	/// Initializes and registers all core resource types
	/// This should be called once at application startup
	/// </summary>
	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}

		lock (_lock)
		{
			if (_initialized)
			{
				return;
			}

		// Register core resource types with their proto type names
		// Based on omni proto definitions
		ResourceTypeRegistry.Register<Cluster>("Clusters.omni.sidero.dev");
		ResourceTypeRegistry.Register<Machine>("Machines.omni.sidero.dev");
		ResourceTypeRegistry.Register<ClusterMachine>("ClusterMachines.omni.sidero.dev");
		ResourceTypeRegistry.Register<ConfigPatch>("ConfigPatches.omni.sidero.dev");
		ResourceTypeRegistry.Register<ExtensionsConfiguration>("ExtensionsConfigurations.omni.sidero.dev");
		
		// Register auth resource types
		ResourceTypeRegistry.Register<User>("Users.omni.sidero.dev");
		ResourceTypeRegistry.Register<Identity>("Identities.omni.sidero.dev");

		_initialized = true;
		}
	}

	/// <summary>
	/// Gets whether the resource types have been initialized
	/// </summary>
	public static bool IsInitialized => _initialized;

	/// <summary>
	/// Proto type name for Cluster resources
	/// </summary>
	public const string ClusterType = "Clusters.omni.sidero.dev";

	/// <summary>
	/// Proto type name for Machine resources
	/// </summary>
	public const string MachineType = "Machines.omni.sidero.dev";

	/// <summary>
	/// Proto type name for ClusterMachine resources
	/// </summary>
	public const string ClusterMachineType = "ClusterMachines.omni.sidero.dev";

	/// <summary>
	/// Proto type name for ConfigPatch resources
	/// </summary>
	public const string ConfigPatchType = "ConfigPatches.omni.sidero.dev";

	/// <summary>
	/// Proto type name for ExtensionsConfiguration resources
	/// </summary>
	public const string ExtensionsConfigurationType = "ExtensionsConfigurations.omni.sidero.dev";

	/// <summary>
	/// Proto type name for User resources
	/// </summary>
	public const string UserType = "Users.omni.sidero.dev";

	/// <summary>
	/// Proto type name for Identity resources
	/// </summary>
	public const string IdentityType = "Identities.omni.sidero.dev";
}
