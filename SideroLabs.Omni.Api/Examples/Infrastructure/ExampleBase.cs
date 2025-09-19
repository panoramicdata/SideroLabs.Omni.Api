using SideroLabs.Omni.Api.Examples.Infrastructure;

namespace SideroLabs.Omni.Api.Examples.Infrastructure;

/// <summary>
/// Base class for all example scenarios
/// </summary>
/// <param name="output">Output interface</param>
public abstract class ExampleBase(IExampleOutput output)
{
	protected readonly IExampleOutput Output = output ?? throw new ArgumentNullException(nameof(output));

	/// <summary>
	/// Executes the example with standard error handling
	/// </summary>
	/// <returns>Task representing the example execution</returns>
	public Task RunAsync() => RunAsync(CancellationToken.None);

	/// <summary>
	/// Executes the example with standard error handling
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Task representing the example execution</returns>
	public async Task RunAsync(CancellationToken cancellationToken)
	{
		try
		{
			await ExecuteExampleAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Output.WriteError("Operation was cancelled");
		}
		catch (Exception ex)
		{
			Output.WriteError($"Error: {ex.Message}");
		}
	}

	/// <summary>
	/// Executes the specific example logic
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Task representing the example execution</returns>
	protected abstract Task ExecuteExampleAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates a cancellation token source with the specified timeout
	/// </summary>
	/// <param name="timeout">Timeout duration</param>
	/// <returns>Cancellation token source</returns>
	protected static CancellationTokenSource CreateTimeoutSource(TimeSpan timeout) => new(timeout);
}
