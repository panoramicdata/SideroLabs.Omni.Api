namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents the result of a JSON schema validation operation
/// </summary>
public class ValidateJsonSchemaResult
{
	/// <summary>
	/// Gets or sets the list of validation errors
	/// Empty list means validation succeeded
	/// </summary>
	public List<ValidateJsonSchemaError> Errors { get; set; } = [];

	/// <summary>
	/// Gets a value indicating whether the validation was successful
	/// </summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>
	/// Gets the total number of errors (including nested errors)
	/// </summary>
	public int TotalErrorCount
	{
		get
		{
			var count = 0;
			CountErrors(Errors, ref count);
			return count;
		}
	}

	/// <summary>
	/// Gets a formatted error message with all validation errors
	/// </summary>
	public string GetErrorSummary()
	{
		if (IsValid)
		{
			return "Validation successful - no errors found";
		}

		var messages = new List<string>
		{
			$"JSON Schema Validation Failed - {TotalErrorCount} error(s) found:",
			""
		};

		foreach (var error in Errors)
		{
			messages.Add(error.GetFullErrorMessage());
			messages.Add("");
		}

		return string.Join(Environment.NewLine, messages);
	}

	private static void CountErrors(List<ValidateJsonSchemaError> errors, ref int count)
	{
		foreach (var error in errors)
		{
			if (!string.IsNullOrEmpty(error.Cause))
			{
				count++;
			}

			CountErrors(error.Errors, ref count);
		}
	}
}
