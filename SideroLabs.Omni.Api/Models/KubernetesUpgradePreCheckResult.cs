namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Result of Kubernetes upgrade pre-checks
/// </summary>
public class KubernetesUpgradePreCheckResult
{
	/// <summary>
	/// Whether the pre-checks passed
	/// </summary>
	public required bool Ok { get; init; }

	/// <summary>
	/// Reason for the check result (error message if Ok is false, success message otherwise)
	/// </summary>
	public required string Reason { get; init; }

	/// <summary>
	/// Creates a successful pre-check result
	/// </summary>
	/// <param name="reason">Success message</param>
	/// <returns>Successful pre-check result</returns>
	public static KubernetesUpgradePreCheckResult Success(string reason = "All pre-checks passed") =>
		new() { Ok = true, Reason = reason };

	/// <summary>
	/// Creates a failed pre-check result
	/// </summary>
	/// <param name="reason">Failure reason</param>
	/// <returns>Failed pre-check result</returns>
	public static KubernetesUpgradePreCheckResult Failure(string reason) =>
		new() { Ok = false, Reason = reason };
}
