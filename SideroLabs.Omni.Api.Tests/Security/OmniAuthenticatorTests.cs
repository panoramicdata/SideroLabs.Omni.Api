using System.Text.Json;
using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SideroLabs.Omni.Api.Security;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Security;

/// <summary>
/// Unit tests for OmniAuthenticator PGP key loading and authentication info generation.
/// </summary>
public class OmniAuthenticatorTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Verifies that a valid base64-encoded PGP key file is loaded correctly and produces the expected identity and fingerprint.
	/// </summary>
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

		testKeyFile.Exists.Should().Be(TestExpectations.ExpectedKeyFileExists, "Test PGP key file existence should match configuration");

		if (!TestExpectations.ExpectedKeyFileExists)
		{
			// Skip the test if the key file is not expected to exist
			return;
		}

		using var loggerFactory = new LoggerFactory();
		var logger = loggerFactory.CreateLogger<OmniAuthenticator>();

		// Act
		Logger.LogInformation("Calling FromFileAsync...");
		var authenticator = await OmniAuthenticator.FromFileAsync(testKeyFile, logger, CancellationToken);
		Logger.LogInformation("FromFileAsync completed successfully");

		// Assert
		authenticator.Should().NotBeNull();
		authenticator.Identity.Should().Be(TestExpectations.ExpectedIdentity, "Should contain the expected identity from configuration");
		authenticator.KeyFingerprint.Should().MatchRegex("^[a-f0-9]+$", "Key fingerprint should be lowercase hex");

		var authInfo = authenticator.GetAuthenticationInfo();
		authInfo.Should().Contain(TestExpectations.ExpectedIdentity);
		authInfo.Should().MatchRegex("Fingerprint: [a-f0-9]+");

		// Log the generated info for inspection
		Logger.LogInformation("Generated Auth Info: {AuthInfo}", authInfo);
	}

	/// <summary>
	/// Verifies that providing a non-existent file path throws FileNotFoundException.
	/// </summary>
	[Fact]
	public async Task FromFileAsync_WithNonExistentFile_ThrowsException()
	{
		var action = () => OmniAuthenticator.FromFileAsync(new FileInfo("non-existent-file.txt"), NullLogger<OmniAuthenticator>.Instance, CancellationToken);

		await action.Should().ThrowAsync<FileNotFoundException>();
	}

	/// <summary>
	/// Verifies that a file containing invalid Base64 content throws InvalidOperationException.
	/// </summary>
	[Fact]
	public Task FromFileAsync_WithInvalidBase64Content_ThrowsInvalidOperationException() =>
		AssertRejectedAsync("invalid-base64-content!", "Invalid Base64 content in the file");

	/// <summary>
	/// Verifies that a JSON payload missing the 'name' property throws InvalidOperationException.
	/// </summary>
	[Fact]
	public Task FromFileAsync_WithMissingNameProperty_ThrowsInvalidOperationException() =>
		AssertRejectedAsync(ToBase64("""{"pgp_key":"test-pgp-key"}"""), "Missing 'name' property in JSON content");

	/// <summary>
	/// Verifies that loading an auth token file that is missing the <c>pgp_key</c> JSON property throws an <see cref="InvalidOperationException"/>.
	/// </summary>
	[Fact]
	public Task FromFileAsync_WithMissingPgpKeyProperty_ThrowsInvalidOperationException() =>
		AssertRejectedAsync(ToBase64("""{"name":"test-user"}"""), "Missing 'pgp_key' property in JSON content");

	private static string ToBase64(string json) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

	private async Task AssertRejectedAsync(string fileContent, string expectedMessage)
	{
		var tempFile = Path.GetTempFileName();

		try
		{
			await File.WriteAllTextAsync(tempFile, fileContent, CancellationToken);

			var action = () => OmniAuthenticator.FromFileAsync(new FileInfo(tempFile), NullLogger<OmniAuthenticator>.Instance, CancellationToken);

			await action.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedMessage);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}
	/// <summary>
	/// Verifies that signing a gRPC request with a valid PGP key file populates the expected authentication headers.
	/// </summary>
	[Fact]
	public async Task SignRequest_WithValidPgpKeyFile_SignsGrpcRequest()
	{
		// Skip if the test key file is not expected to exist
		if (!TestExpectations.ExpectedKeyFileExists)
		{
			return;
		}

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
		timestampHeader!.Value.Should().NotBeNullOrEmpty("Timestamp should not be empty");

		var payloadHeader = metadata.FirstOrDefault(m => m.Key == "x-sidero-payload");
		payloadHeader.Should().NotBeNull("Payload header should be present");
		payloadHeader!.Value.Should().NotBeNullOrEmpty("Payload should not be empty");

		var signatureHeader = metadata.FirstOrDefault(m => m.Key == "x-sidero-signature");
		signatureHeader.Should().NotBeNull("Signature header should be present");
		signatureHeader!.Value.Should().NotBeNullOrEmpty("Signature should not be empty");

		// Verify signature format: "siderov1 {identity} {fingerprint} {base64_signature}"
		var signatureParts = signatureHeader.Value.Split(' ');
		signatureParts.Should().HaveCount(4, "Signature should have 4 parts");
		signatureParts[0].Should().Be("siderov1", "First part should be signature version");
		signatureParts[1].Should().Be(TestExpectations.ExpectedIdentity, "Second part should be expected identity from configuration");
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
