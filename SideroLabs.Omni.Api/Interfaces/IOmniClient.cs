namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for the OmniClient to support dependency injection
/// </summary>
public interface IOmniClient : IDisposable
{
	/// <summary>
	/// Gets the Management Service for administrative and operational tasks
	/// </summary>
	IManagementService Management { get; }

	/// <summary>
	/// Gets the gRPC endpoint URL
	/// </summary>
	string Endpoint { get; }

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
