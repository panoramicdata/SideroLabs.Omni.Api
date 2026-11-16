using System.Text;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Models;

namespace SideroLabs.Omni.Api.Examples;

/// <summary>
/// Service account and provisioning examples
/// </summary>
public static partial class OmniClientExample
{
	/// <summary>
	/// Example demonstrating service account lifecycle management
	/// </summary>
	public static async Task ServiceAccountManagementExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "admin-user",
			PgpPrivateKey = "<your armored PGP private key>",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Service Account Lifecycle ===");
			var samplePgpPublicKey = GetSamplePgpPublicKey();

			await DemonstrateServiceAccountLifecycle(client, samplePgpPublicKey, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Operation was cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets a sample PGP public key for demonstration purposes
	/// </summary>
	private static string GetSamplePgpPublicKey()
	{
		return """
			-----BEGIN PGP PUBLIC KEY BLOCK-----

			mQENBGJxyz4BCADGn5n1...sample...key...content
			-----END PGP PUBLIC KEY BLOCK-----
			""";
	}

	/// <summary>
	/// Demonstrates the complete service account lifecycle
	/// </summary>
	private static async Task DemonstrateServiceAccountLifecycle(OmniClient client, string pgpPublicKey, CancellationToken cancellationToken)
	{
		// Create a new service account - NEW API!
		Console.WriteLine("Creating service account...");
		var publicKeyId = await client.ServiceAccounts.CreateAsync(
			armoredPgpPublicKey: pgpPublicKey,
			useUserRole: true, // Use the role of the creating user
			cancellationToken: cancellationToken);

		Console.WriteLine($"✅ Created service account with public key ID: {publicKeyId}");

		// List service accounts to see the new one - NEW API!
		var serviceAccounts = await client.ServiceAccounts.ListAsync(cancellationToken);
		var newAccount = serviceAccounts.FirstOrDefault(sa =>
			sa.PgpPublicKeys.Any(key => key.Id == publicKeyId));

		if (newAccount != null)
		{
			await ProcessNewServiceAccount(client, newAccount, pgpPublicKey, cancellationToken);
		}
	}

	/// <summary>
	/// Processes operations on a newly created service account
	/// </summary>
	private static async Task ProcessNewServiceAccount(OmniClient client, ServiceAccountInfo account, string pgpPublicKey, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Found new service account: {account.Name}");
		Console.WriteLine($"  Role: {account.Role}");

		// Renew the service account with a new key - NEW API!
		Console.WriteLine("Renewing service account...");
		var newPublicKeyId = await client.ServiceAccounts.RenewAsync(
			name: account.Name,
			armoredPgpPublicKey: pgpPublicKey, // In practice, use a new key
			cancellationToken: cancellationToken);

		Console.WriteLine($"✅ Renewed service account with new public key ID: {newPublicKeyId}");

		// Clean up - destroy the service account - NEW API!
		Console.WriteLine("Cleaning up - destroying service account...");
		await client.ServiceAccounts.DestroyAsync(
			name: account.Name,
			cancellationToken: cancellationToken);

		Console.WriteLine($"✅ Destroyed service account: {account.Name}");
	}

	/// <summary>
	/// Example demonstrating machine provisioning with schematics
	/// </summary>
	public static async Task MachineProvisioningExample()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://your-omni-instance.example.com"),
			Identity = "provisioning-user",
			PgpPrivateKey = "<your armored PGP private key>",
			TimeoutSeconds = 30,
			UseTls = true,
			ValidateCertificate = true
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var cancellationToken = cts.Token;

		try
		{
			Console.WriteLine("=== Machine Provisioning with Schematics ===");
			await CreateAndDisplaySchematic(client, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Operation was cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	/// <summary>
	/// Creates a schematic and displays the results
	/// </summary>
	private static async Task CreateAndDisplaySchematic(OmniClient client, CancellationToken cancellationToken)
	{
		// Create a schematic for provisioning machines with all available options - NEW API!
		var options = new CreateSchematicOptions
		{
			Extensions =
			[
				"siderolabs/iscsi-tools",        // iSCSI storage support
				"siderolabs/util-linux-tools",   // Additional Linux utilities
				"siderolabs/gvisor"              // Container runtime security
			],
			ExtraKernelArgs =
			[
				"console=ttyS0,115200",          // Serial console
				"net.ifnames=0",                 // Predictable network names
				"systemd.unified_cgroup_hierarchy=0"
			],
			MetaValues = new Dictionary<uint, string>
			{
				{ 0x0a, "datacenter-1" },       // Datacenter location
				{ 0x0b, "rack-a1" },            // Rack identifier
				{ 0x0c, "production" }          // Environment
			},
			TalosVersion = "v1.7.0",
			MediaId = "installer",
			SecureBoot = true,
			SiderolinkGrpcTunnelMode = SiderolinkGrpcTunnelMode.Auto
		};

		var schematicResult = await client.Schematics.CreateAsync(options, cancellationToken);

		DisplaySchematicResults(schematicResult);
	}

	/// <summary>
	/// Displays the schematic creation results
	/// </summary>
	private static void DisplaySchematicResults(SchematicResult schematicResult)
	{
		Console.WriteLine($"✅ Created schematic: {schematicResult.SchematicId}");
		Console.WriteLine($"📦 PXE Boot URL: {schematicResult.PxeUrl}");
		Console.WriteLine($"🔌 gRPC Tunnel: {(schematicResult.GrpcTunnelEnabled ? "Enabled" : "Disabled")}");
		Console.WriteLine();
		Console.WriteLine("Use this PXE URL to boot machines with the configured extensions and settings.");
		Console.WriteLine("The schematic includes:");
		Console.WriteLine("  - iSCSI tools for storage");
		Console.WriteLine("  - Additional Linux utilities");
		Console.WriteLine("  - gVisor for container security");
		Console.WriteLine("  - Custom kernel arguments for console and networking");
		Console.WriteLine("  - Metadata tags for datacenter, rack, and environment");
		Console.WriteLine("  - Talos v1.7.0 with secure boot enabled");
		Console.WriteLine($"  - gRPC tunnel mode configured for optimal connectivity");
	}

}
