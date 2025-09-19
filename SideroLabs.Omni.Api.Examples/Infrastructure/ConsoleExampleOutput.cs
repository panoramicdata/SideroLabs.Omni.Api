namespace SideroLabs.Omni.Api.Examples.Infrastructure;

/// <summary>
/// Console implementation of example output
/// </summary>
public class ConsoleExampleOutput : IExampleOutput
{
	/// <inheritdoc />
	public void WriteLine(string message)
	{
		Console.WriteLine(message);
	}

	/// <inheritdoc />
	public void WriteLine(string format, params object[] args)
	{
		Console.WriteLine(format, args);
	}

	/// <inheritdoc />
	public void WriteSection(string title)
	{
		Console.WriteLine();
		Console.WriteLine($"=== {title} ===");
	}

	/// <inheritdoc />
	public void WriteSuccess(string message)
	{
		var originalColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine($"✅ {message}");
		Console.ForegroundColor = originalColor;
	}

	/// <inheritdoc />
	public void WriteError(string message)
	{
		var originalColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"❌ {message}");
		Console.ForegroundColor = originalColor;
	}
}
