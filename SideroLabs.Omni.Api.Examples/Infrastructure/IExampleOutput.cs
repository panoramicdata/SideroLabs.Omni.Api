namespace SideroLabs.Omni.Api.Examples.Infrastructure;

/// <summary>
/// Interface for example output handling
/// </summary>
public interface IExampleOutput
{
	/// <summary>
	/// Writes a line of text
	/// </summary>
	/// <param name="message">Message to write</param>
	void WriteLine(string message);

	/// <summary>
	/// Writes a formatted line of text
	/// </summary>
	/// <param name="format">Format string</param>
	/// <param name="args">Format arguments</param>
	void WriteLine(string format, params object[] args);

	/// <summary>
	/// Writes a section header
	/// </summary>
	/// <param name="title">Section title</param>
	void WriteSection(string title);

	/// <summary>
	/// Writes a success message
	/// </summary>
	/// <param name="message">Success message</param>
	void WriteSuccess(string message);

	/// <summary>
	/// Writes an error message
	/// </summary>
	/// <param name="message">Error message</param>
	void WriteError(string message);
}
