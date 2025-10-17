namespace SideroLabs.Omni.Api.Resources;

/// <summary>
/// Resource event types for watch operations
/// </summary>
public enum ResourceEventType
{
	/// <summary>
	/// Unknown event type
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Resource was created
	/// </summary>
	Created = 1,

	/// <summary>
	/// Resource was updated
	/// </summary>
	Updated = 2,

	/// <summary>
	/// Resource was destroyed
	/// </summary>
	Destroyed = 3,

	/// <summary>
	/// Initial bootstrap event
	/// </summary>
	Bootstrapped = 4
}

/// <summary>
/// Resource event wrapper for watch streams
/// </summary>
/// <typeparam name="TResource">Resource type</typeparam>
public class ResourceEvent<TResource> where TResource : IOmniResource
{
	/// <summary>
	/// Event type
	/// </summary>
	public ResourceEventType Type { get; set; }

	/// <summary>
	/// The resource that changed
	/// </summary>
	public TResource Resource { get; set; } = default!;

	/// <summary>
	/// Previous version of the resource (for updates)
	/// </summary>
	public TResource? OldResource { get; set; }

	/// <summary>
	/// Total number of resources (for list/watch operations)
	/// </summary>
	public int Total { get; set; }
}
