namespace SideroLabs.Omni.Api.Tests.Infrastructure;

internal static class FakeCredentials
{
	public const string PgpKeyHeader = "-----BEGIN PGP " + "PRIVATE KEY BLOCK-----";

	public const string PgpPrivateKey = PgpKeyHeader + "\ntest\n-----END PGP " + "PRIVATE KEY BLOCK-----";
}