using System.Text.Json;
using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Security;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Security;

public class OmniAuthenticatorTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task FromFileAsync_WithValidPgpKeyFile_ReturnsCorrectInfo()
	{
		// Arrange
		var currentDirectory = Directory.GetCurrentDirectory();
		Logger.LogInformation("Current Directory: {CurrentDirectory}", currentDirectory);

		var testDataDirectory = Path.Combine(currentDirectory, "TestData");
		Logger.LogInformation("Test Data Directory: {TestDataDirectory}", testDataDirectory);

		var testKeyFile = new FileInfo(Path.Combine(testDataDirectory, "pgp-key-test.txt"));
		Logger.LogInformation("Test Key File Path: {TestKeyFilePath}", testKeyFile.FullName);
		Logger.LogInformation("Test Key File Exists: {TestKeyFileExists}", testKeyFile.Exists);

		testKeyFile.Exists.Should().BeTrue("Test PGP key file should exist for this test");

		var logger = new LoggerFactory().CreateLogger<OmniAuthenticator>();

		// Act
		Logger.LogInformation("Calling FromFileAsync...");
		var authenticator = await OmniAuthenticator.FromFileAsync(testKeyFile, logger, CancellationToken);
		Logger.LogInformation("FromFileAsync completed successfully");

		// Assert
		authenticator.Should().NotBeNull();
		authenticator.Identity.Should().Be("david-bond", "Should contain the correct identity");
		authenticator.KeyFingerprint.Should().MatchRegex("^[a-f0-9]+$", "Key fingerprint should be lowercase hex");

		var authInfo = authenticator.GetAuthenticationInfo();
		authInfo.Should().Contain("david-bond");
		authInfo.Should().MatchRegex("Fingerprint: [a-f0-9]+");

		// Log the generated info for inspection
		Logger.LogInformation("Generated Auth Info: {AuthInfo}", authInfo);
	}

	[Fact]
	public async Task FromFileAsync_WithNonExistentFile_ThrowsException()
	{
		// Arrange
		var nonExistentFile = new FileInfo("non-existent-file.txt");
		var logger = new LoggerFactory().CreateLogger<OmniAuthenticator>();

		// Act & Assert
		var action = () => OmniAuthenticator.FromFileAsync(nonExistentFile, logger, CancellationToken);
		await action.Should().ThrowAsync<FileNotFoundException>();
	}

	[Fact]
	public async Task FromFileAsync_WithInvalidBase64Content_ThrowsInvalidOperationException()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllTextAsync(tempFile, "invalid-base64-content!", CancellationToken);
			var testKeyFile = new FileInfo(tempFile);

			var logger = new LoggerFactory().CreateLogger<OmniAuthenticator>();

			// Act & Assert
			var action = () => OmniAuthenticator.FromFileAsync(testKeyFile, logger, CancellationToken);
			await action.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Invalid Base64 content in the file");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task FromFileAsync_WithMissingNameProperty_ThrowsInvalidOperationException()
	{
		// Arrange
		var jsonContent = """{"pgp_key":"-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"}""";
		var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));

		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllTextAsync(tempFile, base64Content, CancellationToken);
			var testKeyFile = new FileInfo(tempFile);

			var logger = new LoggerFactory().CreateLogger<OmniAuthenticator>();

			// Act & Assert
			var action = () => OmniAuthenticator.FromFileAsync(testKeyFile, logger, CancellationToken);
			await action.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Missing 'name' property in JSON content");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task FromFileAsync_WithMissingPgpKeyProperty_ThrowsInvalidOperationException()
	{
		// Arrange
		var jsonContent = """{"name":"test-user"}""";
		var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));

		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllTextAsync(tempFile, base64Content, CancellationToken);
			var testKeyFile = new FileInfo(tempFile);

			var logger = new LoggerFactory().CreateLogger<OmniAuthenticator>();

			// Act & Assert
			var action = () => OmniAuthenticator.FromFileAsync(testKeyFile, logger, CancellationToken);
			await action.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Missing 'pgp_key' property in JSON content");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task SignRequest_WithValidPgpKeyFile_SignsGrpcRequest()
	{
		// Arrange
		var authenticator = await CreateTestAuthenticatorAsync();

		// Create sample gRPC metadata and method
		var metadata = new Metadata();
		const string method = "/omni.management.ManagementService/ListClusters";

		// Act
		Logger.LogInformation("Calling SignRequest...");
		authenticator.SignRequest(metadata, method);
		Logger.LogInformation("SignRequest completed successfully");

		// Assert
		metadata.Should().NotBeEmpty("Metadata should contain authentication headers");

		// Check that all required headers are present
		var timestampHeader = metadata.FirstOrDefault(m => m.Key == "x-sidero-timestamp");
		timestampHeader.Should().NotBeNull("Timestamp header should be present");
		timestampHeader.Value.Should().NotBeNullOrEmpty("Timestamp should not be empty");

		var payloadHeader = metadata.FirstOrDefault(m => m.Key == "x-sidero-payload");
		payloadHeader.Should().NotBeNull("Payload header should be present");
		payloadHeader.Value.Should().NotBeNullOrEmpty("Payload should not be empty");

		var signatureHeader = metadata.FirstOrDefault(m => m.Key == "x-sidero-signature");
		signatureHeader.Should().NotBeNull("Signature header should be present");
		signatureHeader.Value.Should().NotBeNullOrEmpty("Signature should not be empty");

		// Verify signature format: "siderov1 {identity} {fingerprint} {base64_signature}"
		var signatureParts = signatureHeader.Value.Split(' ');
		signatureParts.Should().HaveCount(4, "Signature should have 4 parts");
		signatureParts[0].Should().Be("siderov1", "First part should be signature version");
		signatureParts[1].Should().Be("david-bond", "Second part should be identity");
		signatureParts[2].Should().MatchRegex("^[a-f0-9]+$", "Third part should be key fingerprint in hex");
		signatureParts[3].Should().NotBeNullOrEmpty("Fourth part should be base64 signature");

		// Verify payload is valid JSON containing method and headers
		var payloadJson = payloadHeader.Value;
		var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);
		payload.GetProperty("method").GetString().Should().Be(method);
		payload.GetProperty("headers").Should().NotBeNull();

		// Log the headers for inspection
		Logger.LogInformation("Generated Headers:");
		foreach (var header in metadata)
		{
			Logger.LogInformation("  {Key}: {Value}", header.Key, header.Value);
		}
	}
}
