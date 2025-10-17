namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a JSON schema validation error
/// This is a recursive structure that can contain nested errors
/// </summary>
public class ValidateJsonSchemaError
{
	/// <summary>
	/// Gets or sets nested validation errors
	/// </summary>
	public List<ValidateJsonSchemaError> Errors { get; set; } = [];

	/// <summary>
	/// Gets or sets the path in the JSON schema where the error occurred
	/// </summary>
	public string SchemaPath { get; set; } = "";

	/// <summary>
	/// Gets or sets the path in the data where the error occurred
	/// </summary>
	public string DataPath { get; set; } = "";

	/// <summary>
	/// Gets or sets the cause or description of the validation error
	/// </summary>
	public string Cause { get; set; } = "";

	/// <summary>
	/// Gets a value indicating whether this error has nested errors
	/// </summary>
	public bool HasNestedErrors => Errors.Count > 0;

	/// <summary>
	/// Gets a formatted error message including all nested errors
	/// </summary>
	public string GetFullErrorMessage()
	{
		var messages = new List<string>();
		CollectErrorMessages(this, messages, 0);
		return string.Join(Environment.NewLine, messages);
	}

	private static void CollectErrorMessages(ValidateJsonSchemaError error, List<string> messages, int depth)
	{
		var indent = new string(' ', depth * 2);

		if (!string.IsNullOrEmpty(error.Cause))
		{
			messages.Add($"{indent}• {error.Cause}");
			if (!string.IsNullOrEmpty(error.DataPath))
			{
				messages.Add($"{indent}  Data path: {error.DataPath}");
			}

			if (!string.IsNullOrEmpty(error.SchemaPath))
			{
				messages.Add($"{indent}  Schema path: {error.SchemaPath}");
			}
		}

		foreach (var nestedError in error.Errors)
		{
			CollectErrorMessages(nestedError, messages, depth + 1);
		}
	}
}
