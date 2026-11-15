using AwesomeAssertions;
using Grpc.Core;
using SideroLabs.Omni.Api.Utilities;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Utilities;

/// <summary>
/// Offline tests for gRPC error parsing
/// </summary>
public class GrpcErrorParserTests
{
	private static RpcException Error(string detail, Metadata? trailers = null) =>
		new(new Status(StatusCode.Unavailable, detail), trailers ?? []);

	/// <summary>
	/// The HTTP status code is read from the detail text.
	/// </summary>
	[Theory]
	[InlineData("upstream said HTTP status code: 503", 503)]
	[InlineData("no code here", null)]
	[InlineData("", null)]
	public void ExtractHttpStatusCode_ReadsDetail(string detail, int? expected) =>
		GrpcErrorParser.ExtractHttpStatusCode(Error(detail)).Should().Be(expected);

	/// <summary>
	/// A text trailer carries the response body.
	/// </summary>
	[Fact]
	public void ExtractHttpResponseBody_FromTextTrailer_ReturnsValue() =>
		GrpcErrorParser.ExtractHttpResponseBody(Error("x", [new Metadata.Entry("http-body", "oops")])).Should().Be("oops");

	/// <summary>
	/// A binary trailer is decoded as UTF-8.
	/// </summary>
	[Fact]
	public void ExtractHttpResponseBody_FromBinaryTrailer_ReturnsDecodedText() =>
		GrpcErrorParser.ExtractHttpResponseBody(Error("x", [new Metadata.Entry("grpc-status-details-bin", "hello"u8.ToArray())])).Should().Be("hello");

	/// <summary>
	/// A long detail is used as the body when no trailer matches.
	/// </summary>
	[Fact]
	public void ExtractHttpResponseBody_FallsBackToLongDetail()
	{
		var detail = new string('d', 60);

		GrpcErrorParser.ExtractHttpResponseBody(Error(detail)).Should().Be(detail);
		GrpcErrorParser.ExtractHttpResponseBody(Error("short")).Should().BeNull();
	}

	/// <summary>
	/// The detailed error includes status, body and trailers.
	/// </summary>
	[Fact]
	public void FormatDetailedError_IncludesAllParts()
	{
		var trailers = new Metadata
		{
			{ "http-body", "bad gateway" },
			{ "blob-bin", new byte[] { 1, 2 } }
		};

		var message = GrpcErrorParser.FormatDetailedError(Error("HTTP status code: 502", trailers));

		message.Should().Contain("HTTP Status Code: 502")
			.And.Contain("HTTP Response Body:\nbad gateway")
			.And.Contain("blob-bin: [binary data, 2 bytes]")
			.And.Contain("http-body: bad gateway");
	}

	/// <summary>
	/// Only 4xx and 5xx codes count as HTTP errors.
	/// </summary>
	[Theory]
	[InlineData("HTTP status code: 404", true)]
	[InlineData("HTTP status code: 200", false)]
	[InlineData("nothing", false)]
	public void IsHttpError_ChecksRange(string detail, bool expected) =>
		GrpcErrorParser.IsHttpError(Error(detail)).Should().Be(expected);
}
