using AwesomeAssertions;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Services;
using SideroLabs.Omni.Api.Tests.Infrastructure;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Services;

/// <summary>
/// Offline tests for the user management, template and resource mapping services
/// </summary>
public class ServiceBehaviourTests
{
	private const string TwoDocumentTemplate = """
		kind: Cluster
		metadata:
		  id: c1
		spec:
		  kubernetesVersion: v1.29.0
		---
		kind: Machine
		metadata:
		  id: m1
		---
		kind: Unknown
		metadata:
		  id: u1
		""";

	private static (UserManagement Users, FakeResourceClient Fake) CreateUsers()
	{
		var (client, fake) = FakeResourceClient.Create();
		return (new UserManagement(client, NullLogger.Instance), fake);
	}

	/// <summary>
	/// Creating a user stores both the user and its identity.
	/// </summary>
	[Fact]
	public async Task UserManagement_Create_StoresUserAndIdentity()
	{
		var (users, fake) = CreateUsers();

		var (user, identity) = await users.CreateAsync("a@example.com", "Admin", TestContext.Current.CancellationToken);

		user.Spec.Role.Should().Be("Admin");
		identity.Spec.UserId.Should().Be(user.Metadata.Id);
		fake.Contains<Identity>("a@example.com").Should().BeTrue();
	}

	/// <summary>
	/// Blank arguments are rejected with the parameter name.
	/// </summary>
	[Theory]
	[InlineData("", "Admin", "email")]
	[InlineData("a@example.com", " ", "role")]
	public async Task UserManagement_Create_WithBlankArgument_Throws(string email, string role, string parameter)
	{
		var (users, _) = CreateUsers();

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => users.CreateAsync(email, role, TestContext.Current.CancellationToken));

		exception.ParamName.Should().Be(parameter);
	}

	/// <summary>
	/// Get, list, set role and delete work against the stored resources.
	/// </summary>
	[Fact]
	public async Task UserManagement_FullLifecycle_Works()
	{
		var (users, fake) = CreateUsers();
		var token = TestContext.Current.CancellationToken;
		var (user, _) = await users.CreateAsync("b@example.com", "Reader", token);

		(await users.GetAsync("b@example.com", token)).Role.Should().Be("Reader");
		(await users.ListAsync(token)).Should().ContainSingle().Which.Email.Should().Be("b@example.com");
		(await users.SetRoleAsync("b@example.com", "Operator", token)).Spec.Role.Should().Be("Operator");

		await users.DeleteAsync("b@example.com", token);

		fake.Contains<Identity>("b@example.com").Should().BeFalse();
		fake.Contains<User>(user.Metadata.Id).Should().BeFalse();
	}

	/// <summary>
	/// Rendering parses known kinds and skips unknown ones.
	/// </summary>
	[Fact]
	public async Task TemplateOperations_Render_ParsesKnownKinds()
	{
		var (client, _) = FakeResourceClient.Create();
		var templates = new TemplateOperations(client, NullLogger.Instance);

		var rendered = await templates.RenderAsync(TwoDocumentTemplate, [], TestContext.Current.CancellationToken);

		rendered.Select(r => r.Kind).Should().Equal("Cluster", "Machine");
	}

	/// <summary>
	/// A dry run sync reports each rendered resource as applied.
	/// </summary>
	[Fact]
	public async Task TemplateOperations_Sync_AppliesEachResource()
	{
		var (client, fake) = FakeResourceClient.Create();
		var templates = new TemplateOperations(client, NullLogger.Instance);
		var results = new List<Api.Models.SyncResult>();

		await foreach (var result in templates.SyncAsync(TwoDocumentTemplate, [], true, TestContext.Current.CancellationToken))
		{
			results.Add(result);
		}

		results.Should().HaveCount(2).And.OnlyContain(r => r.Action == "DryRunApply");
		fake.Calls.Should().Contain("ApplyAsync");
	}

	/// <summary>
	/// Unimplemented template operations report that clearly.
	/// </summary>
	[Fact]
	public async Task TemplateOperations_UnimplementedOperations_BehaveAsDocumented()
	{
		var (client, _) = FakeResourceClient.Create();
		var templates = new TemplateOperations(client, NullLogger.Instance);
		var token = TestContext.Current.CancellationToken;

		await templates.ValidateAsync("x", token);
		(await templates.DiffAsync("x", [], token)).Should().BeEmpty();
		await Assert.ThrowsAsync<NotImplementedException>(() => templates.ExportAsync("c", token));
	}

	/// <summary>
	/// Loading reads the file text.
	/// </summary>
	[Fact]
	public async Task TemplateOperations_Load_ReadsFile()
	{
		var (client, _) = FakeResourceClient.Create();
		var templates = new TemplateOperations(client, NullLogger.Instance);
		var path = Path.GetTempFileName();

		try
		{
			await File.WriteAllTextAsync(path, "kind: Cluster", TestContext.Current.CancellationToken);

			(await templates.LoadAsync(path, TestContext.Current.CancellationToken)).Should().Be("kind: Cluster");
		}
		finally
		{
			File.Delete(path);
		}
	}

	/// <summary>
	/// A resource survives the COSI wire round trip.
	/// </summary>
	[Fact]
	public void CosiResourceMapper_RoundTrip_PreservesSpec()
	{
		ResourceTypes.Initialize();
		var cluster = new ClusterBuilder("c1").WithKubernetesVersion("v1.29.0").WithTalosVersion("v1.7.0").Build();

		var wire = CosiResourceMapper.Serialize(cluster);
		var restored = CosiResourceMapper.Deserialize<Cluster>(wire, NullLogger.Instance);

		wire.Metadata.Id.Should().Be("c1");
		restored.Metadata.Id.Should().Be("c1");
		restored.Spec.KubernetesVersion.Should().Be("v1.29.0");
	}

	/// <summary>
	/// Resources without a usable spec come back with metadata only.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void CosiResourceMapper_Deserialize_WithoutUsableSpec_ReturnsMetadataOnly(bool withProtoSpec)
	{
		ResourceTypes.Initialize();
		var wire = new Cosi.Resource.Resource
		{
			Metadata = new Cosi.Resource.Metadata { Id = "c2", Namespace = "default", Type = "Clusters.omni.sidero.dev" },
			Spec = new Cosi.Resource.Spec { ProtoSpec = withProtoSpec ? ByteString.CopyFrom(1, 2, 3) : ByteString.Empty }
		};

		var restored = CosiResourceMapper.Deserialize<Cluster>(wire, NullLogger.Instance);

		restored.Metadata.Id.Should().Be("c2");
		restored.Spec.KubernetesVersion.Should().BeEmpty();
	}

	/// <summary>
	/// The identity and user builders produce linked, labelled resources.
	/// </summary>
	[Fact]
	public void IdentityAndUserBuilders_ProduceLinkedResources()
	{
		var user = new UserBuilder().WithUserId("u1").WithNamespace("ns").AsAdmin().WithLabel("k", "v").Build();
		var identity = new IdentityBuilder("a@example.com").ForUser(user).AsServiceAccount().WithNamespace("ns").Build();

		user.Spec.Role.Should().Be("Admin");
		user.Metadata.Labels["k"].Should().Be("v");
		identity.Spec.UserId.Should().Be("u1");
		identity.Metadata.Labels[Identity.LabelUserID].Should().Be("u1");
		identity.Metadata.Labels[Identity.LabelType].Should().Be(Identity.TypeServiceAccount);
		new IdentityBuilder(identity).WithEmail("b@example.com").AsUserType().Build().Metadata.Labels[Identity.LabelType].Should().Be(Identity.TypeUser);
	}

	/// <summary>
	/// Role shortcuts on the user builder set the expected role names.
	/// </summary>
	[Fact]
	public void UserBuilder_RoleShortcuts_SetRoles()
	{
		new UserBuilder().AsOperator().Build().Spec.Role.Should().Be("Operator");
		new UserBuilder().AsReader().Build().Spec.Role.Should().Be("Reader");
		new UserBuilder().WithRole("None").Build().Spec.Role.Should().Be("None");
		Assert.Throws<ArgumentNullException>(() => new UserBuilder(null!));
		Assert.Throws<ArgumentNullException>(() => new IdentityBuilder((Identity)null!));
		Assert.Throws<ArgumentNullException>(() => new IdentityBuilder("a@example.com").ForUser((User)null!));
	}
}
