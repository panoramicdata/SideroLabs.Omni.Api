namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for the OmniClient to support dependency injection
/// </summary>
public interface IOmniClient : IDisposable
{
	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// This is the primary (and only confirmed working) service interface provided by Omni SaaS
	/// </summary>
	IManagementService Management { get; }

	/// <summary>
	/// Gets the Resource Client for COSI resource operations
	/// Uses the COSI v1alpha1 State service (/cosi.resource.State/*)
	/// </summary>
	IOmniResourceClient Resources { get; }

	/// <summary>
	/// Cluster-specific operations (status, create, delete, machine lock/unlock)
	/// </summary>
	IClusterOperations Clusters { get; }

	/// <summary>
	/// Template operations (rendering, sync, export, diff)
	/// </summary>
	ITemplateOperations Templates { get; }

	/// <summary>
	/// User management operations
	/// </summary>
	IUserManagement Users { get; }

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


