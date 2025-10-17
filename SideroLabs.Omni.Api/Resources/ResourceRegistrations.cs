namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Registers known resource types with the ResourceTypeRegistry at assembly load time.
/// Add additional registrations here as resource types are implemented.
/// </summary>
internal static class ResourceRegistrations
{
	static ResourceRegistrations()
	{
		// Register Cluster resource proto type name. Update the proto type name if different.
		ResourceTypeRegistry.Register<Cluster>("Clusters.omni.sidero.dev");
		// Register Machine resource
		ResourceTypeRegistry.Register<Machine>("Machines.omni.sidero.dev");
	}
}
