using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;

namespace SideroLabs.Omni.Api.Validation;

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
	/// <summary>
	/// Gets or sets whether the validation was successful
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// Gets or sets the validation error messages
	/// </summary>
	public List<string> Errors { get; set; } = [];

	/// <summary>
	/// Creates a successful validation result
	/// </summary>
	/// <returns>A successful validation result</returns>
	public static ValidationResult Success() => new() { IsValid = true };

	/// <summary>
	/// Creates a failed validation result with error messages
	/// </summary>
	/// <param name="errors">The validation error messages</param>
	/// <returns>A failed validation result</returns>
	public static ValidationResult Failure(params string[] errors) => new()
	{
		IsValid = false,
		Errors = [.. errors]
	};

	/// <summary>
	/// Throws an OmniConfigurationException if validation failed
	/// </summary>
	/// <exception cref="OmniConfigurationException">Thrown when validation failed</exception>
	public void ThrowIfInvalid()
	{
		if (!IsValid)
		{
			throw new OmniConfigurationException(Errors);
		}
	}
}

/// <summary>
/// Validator for OmniClient options
/// </summary>
internal class OmniClientOptionsValidator : IOptionsValidator<OmniClientOptions>
{
	/// <inheritdoc />
	public ValidationResult Validate(OmniClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		var errors = new List<string>();

		if (!options.BaseUrl.IsAbsoluteUri)
		{
			errors.Add("BaseUrl must be an absolute Uri");
		}

		if (options.TimeoutSeconds <= 0)
		{
			errors.Add("TimeoutSeconds must be positive");
		}

		// Exactly one of the following must be provided:
		// - PgpPrivateKey
		// - PgpKeyFilePath
		// - AuthToken
		var authMethodsProvided = new[]
		{
			!string.IsNullOrWhiteSpace(options.PgpPrivateKey),
			!string.IsNullOrWhiteSpace(options.PgpKeyFilePath),
			!string.IsNullOrWhiteSpace(options.AuthToken)
		}.Count(b => b);

		switch (authMethodsProvided)
		{
			case 0:
				errors.Add("One of PgpPrivateKey, PgpKeyFilePath, or AuthToken must be provided for authentication");
				break;
			case > 1:
				errors.Add("Only one of PgpPrivateKey, PgpKeyFilePath, or AuthToken should be provided");
				break;
		}

		return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure([.. errors]);
	}
}
