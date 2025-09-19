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

		if (string.IsNullOrWhiteSpace(options.Endpoint))
		{
			errors.Add("Endpoint is required");
		}
		else if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _))
		{
			errors.Add("Endpoint must be a valid URI");
		}

		if (options.TimeoutSeconds <= 0)
		{
			errors.Add("TimeoutSeconds must be positive");
		}

		return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure([.. errors]);
	}
}
