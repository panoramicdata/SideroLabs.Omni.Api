using AwesomeAssertions;
using Grpc.Core;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Models;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Models;

/// <summary>
/// Offline tests for the small result and exception models
/// </summary>
public class ModelBehaviourTests
{
	private static ValidateJsonSchemaError Nested() => new()
	{
		Cause = "outer",
		DataPath = "/a",
		SchemaPath = "#/a",
		Errors = [new() { Cause = "inner", DataPath = "/a/b" }]
	};

	/// <summary>
	/// An empty result is valid and reports success.
	/// </summary>
	[Fact]
	public void ValidateJsonSchemaResult_WithoutErrors_IsValid()
	{
		var result = new ValidateJsonSchemaResult();

		result.IsValid.Should().BeTrue();
		result.TotalErrorCount.Should().Be(0);
		result.GetErrorSummary().Should().Contain("successful");
	}

	/// <summary>
	/// Nested causes are counted and appear in the summary.
	/// </summary>
	[Fact]
	public void ValidateJsonSchemaResult_WithNestedErrors_CountsAndSummarises()
	{
		var result = new ValidateJsonSchemaResult { Errors = [Nested()] };

		result.IsValid.Should().BeFalse();
		result.TotalErrorCount.Should().Be(2);
		result.GetErrorSummary().Should().Contain("2 error(s)").And.Contain("inner");
	}

	/// <summary>
	/// The full message lists causes with indented paths.
	/// </summary>
	[Fact]
	public void ValidateJsonSchemaError_GetFullErrorMessage_IncludesPathsAndNesting()
	{
		var error = Nested();

		error.HasNestedErrors.Should().BeTrue();
		error.GetFullErrorMessage().Should().Contain("Data path: /a").And.Contain("Schema path: #/a").And.Contain("  • inner");
	}

	/// <summary>
	/// Progress strings reflect the most important state first.
	/// </summary>
	[Theory]
	[InlineData("src", "boom", "", 0, 0, 0, "Error from src: boom")]
	[InlineData("", "", "collecting", 10, 5, 0, "collecting - 5/10 (50.0%)")]
	[InlineData("", "", "collecting", 0, 0, 0, "collecting")]
	[InlineData("", "", "", 0, 0, 3, "Bundle data received: 3 bytes")]
	[InlineData("", "", "", 0, 0, 0, "Processing...")]
	public void SupportBundleProgress_GetProgressString_DescribesState(
		string source, string error, string state, int total, int value, int dataLength, string expected)
	{
		var progress = new SupportBundleProgress
		{
			Source = source,
			Error = error,
			State = state,
			Total = total,
			Value = value,
			BundleData = dataLength > 0 ? new byte[dataLength] : null
		};

		progress.GetProgressString().Should().Be(expected);
		progress.HasError.Should().Be(error.Length > 0);
		progress.HasBundleData.Should().Be(dataLength > 0);
	}

	/// <summary>
	/// The join config summary describes whichever parts are present.
	/// </summary>
	[Theory]
	[InlineData(false, false, 0, "Empty configuration")]
	[InlineData(true, false, 1, "2 kernel argument(s)")]
	[InlineData(true, true, 2, "2 kernel argument(s), config (4 characters)")]
	public void MachineJoinConfig_Summary_ReflectsContent(bool withArgs, bool withConfig, int items, string summary)
	{
		var config = new MachineJoinConfig
		{
			KernelArgs = withArgs ? ["a=1", "b=2"] : [],
			Config = withConfig ? "text" : ""
		};

		config.TotalItems.Should().Be(items);
		config.GetSummary().Should().Be(summary);
		config.GetKernelArgsString().Should().Be(withArgs ? "a=1 b=2" : "");
	}

	/// <summary>
	/// Pre-check results expose their reason and outcome.
	/// </summary>
	[Fact]
	public void KubernetesUpgradePreCheckResult_Factories_SetOutcome()
	{
		KubernetesUpgradePreCheckResult.Success().Reason.Should().Be("All pre-checks passed");
		KubernetesUpgradePreCheckResult.Success("ok").Ok.Should().BeTrue();
		KubernetesUpgradePreCheckResult.Failure("bad").Ok.Should().BeFalse();
	}

	/// <summary>
	/// HTTP details are formatted into the message and truncated when long.
	/// </summary>
	[Fact]
	public void OmniGrpcException_WithHttpDetails_FormatsMessage()
	{
		var exception = new OmniGrpcException("op", StatusCode.Internal, "detail", "/m", 502, new string('x', 600));

		exception.HasHttpErrorDetails.Should().BeTrue();
		exception.HttpStatusCode.Should().Be(502);
		exception.GrpcStatusCode.Should().Be(StatusCode.Internal);
		exception.Message.Should().Contain("Internal: detail (HTTP 502)").And.Contain("(truncated)");
	}

	/// <summary>
	/// Without HTTP details the exception reports none.
	/// </summary>
	[Fact]
	public void OmniGrpcException_WithoutHttpDetails_HasNone()
	{
		var exception = new OmniGrpcException("op", StatusCode.NotFound);

		exception.HasHttpErrorDetails.Should().BeFalse();
		exception.Message.Should().Be("gRPC operation 'op' failed with status NotFound");
	}
}
