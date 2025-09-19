namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Exception thrown when configuration validation fails
/// </summary>
/// <param name="validationErrors">The validation errors</param>
/// <param name="innerException">The inner exception</param>
public class OmniConfigurationException(IReadOnlyList<string> validationErrors, Exception? innerException = null) : OmniException("configuration", FormatMessage(validationErrors), null, innerException)
{
	/// <summary>
	/// Gets the validation errors
	/// </summary>
	public IReadOnlyList<string> ValidationErrors { get; } = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));

	private static string FormatMessage(IReadOnlyList<string> errors) =>
		$"Configuration validation failed: {string.Join("; ", errors)}";
}
