namespace SideroLabs.Omni.Api.Tests.Infrastructure;

/// <summary>
/// Configuration model for test expectations
/// </summary>
public class TestExpectations
{
	/// <summary>
	/// The expected identity from the test PGP key
	/// </summary>
	public required string ExpectedIdentity { get; init; }

	/// <summary>
	/// Whether the test PGP key file is expected to exist
	/// </summary>
	public required bool ExpectedKeyFileExists { get; init; }
}
