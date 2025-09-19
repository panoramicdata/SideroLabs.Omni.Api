using SideroLabs.Omni.Api.Validation;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for validating options
/// </summary>
/// <typeparam name="T">The type of options to validate</typeparam>
internal interface IOptionsValidator<in T>
{
	/// <summary>
	/// Validates the specified options
	/// </summary>
	/// <param name="options">The options to validate</param>
	/// <returns>The validation result</returns>
	ValidationResult Validate(T options);
}
