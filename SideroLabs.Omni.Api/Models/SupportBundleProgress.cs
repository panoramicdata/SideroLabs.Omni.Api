namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents progress information for support bundle generation
/// </summary>
public class SupportBundleProgress
{
	/// <summary>
	/// Gets or sets the source of the progress update
	/// </summary>
	public string Source { get; set; } = "";

	/// <summary>
	/// Gets or sets any error that occurred during bundle generation
	/// </summary>
	public string Error { get; set; } = "";

	/// <summary>
	/// Gets or sets the current state of the bundle generation
	/// </summary>
	public string State { get; set; } = "";

	/// <summary>
	/// Gets or sets the total number of items to process
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// Gets or sets the current progress value
	/// </summary>
	public int Value { get; set; }

	/// <summary>
	/// Gets or sets the bundle data (if available in this update)
	/// </summary>
	public byte[]? BundleData { get; set; }

	/// <summary>
	/// Gets a value indicating whether this update contains bundle data
	/// </summary>
	public bool HasBundleData => BundleData != null && BundleData.Length > 0;

	/// <summary>
	/// Gets a value indicating whether an error occurred
	/// </summary>
	public bool HasError => !string.IsNullOrEmpty(Error);

	/// <summary>
	/// Gets the progress percentage (0-100)
	/// </summary>
	public double ProgressPercentage => Total > 0 ? (double)Value / Total * 100 : 0;

	/// <summary>
	/// Gets a formatted progress string
	/// </summary>
	public string GetProgressString()
	{
		if (HasError)
		{
			return $"Error from {Source}: {Error}";
		}

		if (!string.IsNullOrEmpty(State))
		{
			return Total > 0
				? $"{State} - {Value}/{Total} ({ProgressPercentage:F1}%)"
				: State;
		}

		if (HasBundleData)
		{
			return $"Bundle data received: {BundleData!.Length} bytes";
		}

		return "Processing...";
	}
}
