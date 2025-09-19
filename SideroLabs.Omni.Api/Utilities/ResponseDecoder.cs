using System.Text;
using Google.Protobuf;

namespace SideroLabs.Omni.Api.Utilities;

/// <summary>
/// Utility class for decoding gRPC responses
/// </summary>
internal static class ResponseDecoder
{
	/// <summary>
	/// Decodes a configuration response from ByteString to UTF-8 string
	/// </summary>
	/// <param name="byteString">The ByteString containing the encoded configuration</param>
	/// <returns>Decoded configuration as string</returns>
	internal static string DecodeConfigResponse(ByteString byteString) =>
		Encoding.UTF8.GetString(byteString.ToByteArray());
}
