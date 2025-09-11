using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Security;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Security;

public class JwtGeneratorTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task GenerateAsync_WithValidPgpKeyFile_ReturnsSignedJwt()
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

		// Read and log file content for debugging
		var fileContent = await File.ReadAllTextAsync(testKeyFile.FullName, CancellationToken);
		Logger.LogInformation("File Content Length: {FileContentLength}", fileContent.Length);
		Logger.LogInformation("File Content (first 100 chars): {FileContentStart}", fileContent.Substring(0, Math.Min(100, fileContent.Length)));

		var logger = new LoggerFactory().CreateLogger<JwtGenerator>();
		var jwtGenerator = new JwtGenerator(testKeyFile, logger);

		// Act
		Logger.LogInformation("Calling GenerateAsync...");
		var jwt = await jwtGenerator.GenerateAsync(CancellationToken);
		Logger.LogInformation("GenerateAsync completed successfully");

		// Assert
		jwt.Should().NotBeNullOrEmpty();

		// JWT should have 3 parts separated by dots (header.payload.signature)
		var jwtParts = jwt.Split('.');
		Logger.LogInformation("JWT Parts Count: {JwtPartsCount}", jwtParts.Length);
		
		jwtParts.Should().HaveCount(3);

		// Each part should be non-empty
		jwtParts[0].Should().NotBeNullOrEmpty("Header should not be empty");
		jwtParts[1].Should().NotBeNullOrEmpty("Payload should not be empty");
		jwtParts[2].Should().NotBeNullOrEmpty("Signature should not be empty");
		
		// Log each part length for debugging
		Logger.LogInformation("Header Length: {HeaderLength}", jwtParts[0].Length);
		Logger.LogInformation("Payload Length: {PayloadLength}", jwtParts[1].Length);
		Logger.LogInformation("Signature Length: {SignatureLength}", jwtParts[2].Length);

		// Log the generated JWT for inspection
		Logger.LogInformation("Generated JWT: {Jwt}", jwt);
	}

	[Fact]
	public async Task GenerateAsync_WithNonExistentFile_ThrowsException()
	{
		// Arrange
		var nonExistentFile = new FileInfo("non-existent-file.txt");
		var logger = new LoggerFactory().CreateLogger<JwtGenerator>();
		var jwtGenerator = new JwtGenerator(nonExistentFile, logger);

		// Act & Assert
		var action = () => jwtGenerator.GenerateAsync(CancellationToken);
		await action.Should().ThrowAsync<FileNotFoundException>();
	}

	[Fact]
	public async Task GenerateAsync_WithInvalidBase64Content_ThrowsInvalidOperationException()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllTextAsync(tempFile, "invalid-base64-content!", CancellationToken);
			var testKeyFile = new FileInfo(tempFile);

			var logger = new LoggerFactory().CreateLogger<JwtGenerator>();
			var jwtGenerator = new JwtGenerator(testKeyFile, logger);

			// Act & Assert
			var action = () => jwtGenerator.GenerateAsync(CancellationToken);
			await action.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Invalid Base64 content in the file");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task GenerateAsync_WithMissingNameProperty_ThrowsInvalidOperationException()
	{
		// Arrange
		var jsonContent = """{"pgp_key":"-----BEGIN PGP PRIVATE KEY BLOCK-----\ntest\n-----END PGP PRIVATE KEY BLOCK-----"}""";
		var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));

		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllTextAsync(tempFile, base64Content, CancellationToken);
			var testKeyFile = new FileInfo(tempFile);

			var logger = new LoggerFactory().CreateLogger<JwtGenerator>();
			var jwtGenerator = new JwtGenerator(testKeyFile, logger);

			// Act & Assert
			var action = () => jwtGenerator.GenerateAsync(CancellationToken);
			await action.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Missing 'name' property in JSON content");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task GenerateAsync_WithMissingPgpKeyProperty_ThrowsInvalidOperationException()
	{
		// Arrange
		var jsonContent = """{"name":"test-user"}""";
		var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));

		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllTextAsync(tempFile, base64Content, CancellationToken);
			var testKeyFile = new FileInfo(tempFile);

			var logger = new LoggerFactory().CreateLogger<JwtGenerator>();
			var jwtGenerator = new JwtGenerator(testKeyFile, logger);

			// Act & Assert
			var action = () => jwtGenerator.GenerateAsync(CancellationToken);
			await action.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Missing 'pgp_key' property in JSON content");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task GenerateOmniTokenAsync_WithValidPgpKeyFile_ReturnsDifferentFormats()
	{
		// Arrange
		var testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
		var testKeyFile = new FileInfo(Path.Combine(testDataDirectory, "pgp-key-test.txt"));
		
		testKeyFile.Exists.Should().BeTrue("Test PGP key file should exist for this test");

		var logger = new LoggerFactory().CreateLogger<OmniAuthTokenGenerator>();
		var tokenGenerator = new OmniAuthTokenGenerator(testKeyFile, logger);

		// Act
		Logger.LogInformation("Calling GenerateOmniTokenAsync...");
		var token = await tokenGenerator.GenerateOmniTokenAsync(CancellationToken);
		Logger.LogInformation("GenerateOmniTokenAsync completed successfully");

		// Assert
		token.Should().NotBeNullOrEmpty();
		
		// Log the generated token for inspection
		Logger.LogInformation("Generated Omni Token: {Token}", token);
	}
}
