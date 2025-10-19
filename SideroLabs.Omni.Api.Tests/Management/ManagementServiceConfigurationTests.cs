using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Text;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Management;

/// <summary>
/// Integration tests for ManagementService configuration operations
/// Tests kubeconfig, talosconfig, omniconfig retrieval and validation
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ManagementService")]
public class ManagementServiceConfigurationTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task GetKubeConfig_Default_ReturnsValidConfig()
	{
		// Arrange & Act
		var kubeconfig = await OmniClient.Management.GetKubeConfigAsync(CancellationToken);

		// Assert
		Assert.NotNull(kubeconfig);
		Assert.Contains("apiVersion", kubeconfig);
		Assert.Contains("clusters", kubeconfig);
		Assert.Contains("users", kubeconfig);
		Assert.Contains("contexts", kubeconfig);
		
		Logger.LogInformation("✓ Retrieved kubeconfig: {Length} bytes", kubeconfig.Length);
	}

	[Fact]
	public async Task GetKubeConfig_WithServiceAccount_ReturnsConfigWithToken()
	{
		// Arrange & Act
		var kubeconfig = await OmniClient.Management.GetKubeConfigAsync(
			serviceAccount: true,
			serviceAccountTtl: TimeSpan.FromHours(24),
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotNull(kubeconfig);
		Assert.Contains("token", kubeconfig, StringComparison.OrdinalIgnoreCase);
		
		Logger.LogInformation("✓ Retrieved service account kubeconfig with token");
	}

	[Fact]
	public async Task GetKubeConfig_WithServiceAccountAndUser_ReturnsConfig()
	{
		// Arrange
		var serviceAccountUser = "test-user";

		// Act
		var kubeconfig = await OmniClient.Management.GetKubeConfigAsync(
			serviceAccount: true,
			serviceAccountTtl: TimeSpan.FromHours(1),
			serviceAccountUser: serviceAccountUser,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotNull(kubeconfig);
		Assert.Contains(serviceAccountUser, kubeconfig);
		
		Logger.LogInformation("✓ Retrieved kubeconfig for service account user: {User}", serviceAccountUser);
	}

	[Fact]
	public async Task GetTalosConfig_Default_ReturnsValidConfig()
	{
		// Arrange & Act
		var talosconfig = await OmniClient.Management.GetTalosConfigAsync(CancellationToken);

		// Assert
		Assert.NotNull(talosconfig);
		Assert.Contains("context", talosconfig);
		Assert.Contains("contexts", talosconfig);
		
		Logger.LogInformation("✓ Retrieved talosconfig: {Length} bytes", talosconfig.Length);
	}

	[Fact]
	public async Task GetTalosConfig_Raw_ReturnsAdminConfig()
	{
		// Arrange & Act
		var talosconfig = await OmniClient.Management.GetTalosConfigAsync(
			raw: true,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotNull(talosconfig);
		Assert.Contains("context", talosconfig);
		
		Logger.LogInformation("✓ Retrieved raw talosconfig (admin mode)");
	}

	[Fact]
	public async Task GetTalosConfig_WithBreakGlass_ReturnsOperatorConfig()
	{
		// Arrange & Act
		var talosconfig = await OmniClient.Management.GetTalosConfigAsync(
			raw: false,
			breakGlass: true,
			cancellationToken: CancellationToken);

		// Assert
		Assert.NotNull(talosconfig);
		
		Logger.LogWarning("⚠️ Retrieved break-glass talosconfig (operator mode)");
	}

	[Fact]
	public async Task GetOmniConfig_ReturnsValidConfig()
	{
		// Arrange & Act
		var omniconfig = await OmniClient.Management.GetOmniConfigAsync(CancellationToken);

		// Assert
		Assert.NotNull(omniconfig);
		Assert.Contains("context", omniconfig);
		Assert.Contains("contexts", omniconfig);
		
		Logger.LogInformation("✓ Retrieved omniconfig: {Length} bytes", omniconfig.Length);
	}

	[Fact]
	public async Task ValidateConfig_ValidTalosConfig_Succeeds()
	{
		// Arrange
		var validConfig = @"
version: v1alpha1
machine:
  type: controlplane
  install:
    disk: /dev/sda
";

		// Act & Assert - Should not throw
		await OmniClient.Management.ValidateConfigAsync(validConfig, CancellationToken);
		
		Logger.LogInformation("✓ Valid Talos config validated successfully");
	}

	[Fact]
	public async Task ValidateConfig_InvalidYaml_ThrowsException()
	{
		// Arrange
		var invalidConfig = "invalid yaml: [[[[";

		// Act & Assert
		var exception = await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.Management.ValidateConfigAsync(invalidConfig, CancellationToken));

		Logger.LogInformation("✓ Invalid YAML rejected: {StatusCode}", exception.StatusCode);
	}

	[Fact]
	public async Task ValidateConfig_EmptyConfig_ThrowsException()
	{
		// Arrange
		var emptyConfig = "";

		// Act & Assert
		await Assert.ThrowsAsync<RpcException>(async () =>
			await OmniClient.Management.ValidateConfigAsync(emptyConfig, CancellationToken));

		Logger.LogInformation("✓ Empty config rejected");
	}

	[Fact]
	public async Task ValidateJsonSchema_ValidData_ReturnsSuccess()
	{
		// Arrange
		var schema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""name"": { ""type"": ""string"" },
    ""age"": { ""type"": ""number"" }
  },
  ""required"": [""name""]
}";
		var data = @"{
  ""name"": ""John Doe"",
  ""age"": 30
}";

		// Act
		var result = await OmniClient.Management.ValidateJsonSchemaAsync(data, schema, CancellationToken);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
		Assert.Equal(0, result.TotalErrorCount);
		
		Logger.LogInformation("✓ Valid JSON data passed schema validation");
	}

	[Fact]
	public async Task ValidateJsonSchema_InvalidData_ReturnsErrors()
	{
		// Arrange
		var schema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""age"": { ""type"": ""number"" }
  },
  ""required"": [""age""]
}";
		var data = @"{
  ""age"": ""not a number""
}";

		// Act
		var result = await OmniClient.Management.ValidateJsonSchemaAsync(data, schema, CancellationToken);

		// Assert
		Assert.False(result.IsValid);
		Assert.NotEmpty(result.Errors);
		Assert.True(result.TotalErrorCount > 0);
		
		var summary = result.GetErrorSummary();
		Assert.Contains("age", summary, StringComparison.OrdinalIgnoreCase);
		
		Logger.LogInformation("✓ Invalid JSON data failed schema validation");
		Logger.LogInformation("Validation errors:\n{Summary}", summary);
	}

	[Fact]
	public async Task ValidateJsonSchema_MissingRequiredField_ReturnsErrors()
	{
		// Arrange
		var schema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""name"": { ""type"": ""string"" }
  },
  ""required"": [""name"", ""email""]
}";
		var data = @"{
  ""name"": ""John Doe""
}";

		// Act
		var result = await OmniClient.Management.ValidateJsonSchemaAsync(data, schema, CancellationToken);

		// Assert
		Assert.False(result.IsValid);
		Assert.NotEmpty(result.Errors);
		
		var summary = result.GetErrorSummary();
		Assert.Contains("required", summary, StringComparison.OrdinalIgnoreCase);
		
		Logger.LogInformation("✓ Missing required field detected");
		Logger.LogInformation("Validation errors:\n{Summary}", summary);
	}

	[Fact]
	public async Task ValidateJsonSchema_NestedErrors_ReturnsDetailedErrors()
	{
		// Arrange
		var schema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""person"": {
      ""type"": ""object"",
      ""properties"": {
        ""age"": { ""type"": ""number"" },
        ""email"": { ""type"": ""string"", ""format"": ""email"" }
      },
      ""required"": [""age"", ""email""]
    }
  }
}";
		var data = @"{
  ""person"": {
    ""age"": ""not a number"",
    ""email"": ""invalid-email""
  }
}";

		// Act
		var result = await OmniClient.Management.ValidateJsonSchemaAsync(data, schema, CancellationToken);

		// Assert
		Assert.False(result.IsValid);
		Assert.NotEmpty(result.Errors);
		
		// Should have errors for both age and email
		var summary = result.GetErrorSummary();
		Logger.LogInformation("✓ Nested validation errors detected");
		Logger.LogInformation("Validation errors:\n{Summary}", summary);
	}

	[Fact]
	public async Task ValidateJsonSchema_ComplexSchema_ValidatesCorrectly()
	{
		// Arrange
		var schema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""clusters"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object"",
        ""properties"": {
          ""name"": { ""type"": ""string"" },
          ""nodes"": { ""type"": ""number"", ""minimum"": 1 }
        },
        ""required"": [""name"", ""nodes""]
      }
    }
  }
}";
		var data = @"{
  ""clusters"": [
    { ""name"": ""cluster-1"", ""nodes"": 3 },
    { ""name"": ""cluster-2"", ""nodes"": 5 }
  ]
}";

		// Act
		var result = await OmniClient.Management.ValidateJsonSchemaAsync(data, schema, CancellationToken);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
		
		Logger.LogInformation("✓ Complex schema validated successfully");
	}
}
