using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

/// <summary>
/// Unit tests for ExtensionsConfigurationValidator FluentValidation rules.
/// </summary>
public class ExtensionsConfigurationValidatorTests
{
	private static ExtensionsConfiguration Config(string id, List<string> extensions, string? talosVersion = null) => new()
	{
		Metadata = new ResourceMetadata { Id = id, Namespace = "default" },
		Spec = new ExtensionsConfigurationSpec { Extensions = extensions, TalosVersion = talosVersion ?? "" }
	};

	private static ExtensionsConfiguration ValidConfig() => new ExtensionsConfigurationBuilder("test-config")
		.WithExtension("siderolabs/util-linux-tools")
		.ForTalosVersion("v1.7.0")
		.Build();

	private static void AssertInvalid(ExtensionsConfiguration config, Func<FluentValidation.Results.ValidationFailure, bool> match)
	{
		var result = config.Validate();

		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => match(e));
	}

	/// <summary>
	/// Verifies that a fully populated valid ExtensionsConfiguration passes all validation rules.
	/// </summary>
	[Fact]
	public void Validate_WithValidConfiguration_ReturnsSuccess()
	{
		var result = ValidConfig().Validate();

		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	/// <summary>
	/// Verifies that an ExtensionsConfiguration with an empty extension list fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithoutExtensions_ReturnsError() =>
		AssertInvalid(Config("test-config", []), e => e.PropertyName == "Spec.Extensions");

	/// <summary>
	/// Verifies that an ExtensionsConfiguration containing an empty extension name string fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithEmptyExtensionName_ReturnsError() =>
		AssertInvalid(Config("test-config", [""]), e => e.PropertyName.Contains("Extensions") && e.ErrorMessage.Contains("empty"));

	/// <summary>
	/// Verifies that a configuration ID containing uppercase letters or underscores fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidConfigId_ReturnsError() =>
		AssertInvalid(Config("Test_Config", ["siderolabs/util-linux-tools"]), e => e.PropertyName == "Metadata.Id" && e.ErrorMessage.Contains("DNS-1123"));

	/// <summary>
	/// Verifies that a Talos version missing the 'v' prefix fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidTalosVersion_ReturnsError() =>
		AssertInvalid(Config("test-config", ["siderolabs/util-linux-tools"], "1.7.0"), e => e.PropertyName == "Spec.TalosVersion" && e.ErrorMessage.Contains("vX.Y.Z"));

	/// <summary>
	/// Verifies that ValidateAndThrow throws ValidationException for an invalid ExtensionsConfiguration.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithInvalidConfiguration_ThrowsValidationException() =>
		Assert.Throws<ValidationException>(() => Config("test-config", []).ValidateAndThrow());

	/// <summary>
	/// Verifies that ValidateAndThrow does not throw for a fully valid ExtensionsConfiguration.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithValidConfiguration_DoesNotThrow() => ValidConfig().ValidateAndThrow();

	/// <summary>
	/// Verifies that an extensions configuration containing a versioned extension reference passes validation successfully.
	/// </summary>
	[Fact]
	public void Validate_WithVersionedExtension_ReturnsSuccess() =>
		Assert.True(Config("test-config", ["siderolabs/util-linux-tools@v2.13.1"]).Validate().IsValid);

	/// <summary>
	/// Verifies that an extensions configuration containing multiple extension references passes validation successfully.
	/// </summary>
	[Fact]
	public void Validate_WithMultipleExtensions_ReturnsSuccess() =>
		Assert.True(Config("test-config", ["siderolabs/util-linux-tools", "siderolabs/qemu-guest-agent", "siderolabs/iscsi-tools"], "v1.7.0").Validate().IsValid);
}
