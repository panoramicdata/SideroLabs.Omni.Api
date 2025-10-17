namespace SideroLabs.Omni.Api.Models;

public class SyncResult
{
	public required string Action { get; init; }

	public required object Resource { get; init; }

	public string? Error { get; internal set; }
}
