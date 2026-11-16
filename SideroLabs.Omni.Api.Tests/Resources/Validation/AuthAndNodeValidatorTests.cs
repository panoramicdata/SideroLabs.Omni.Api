using AwesomeAssertions;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Resources.Validation;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

/// <summary>
/// Unit tests for the user, identity and cluster machine validators and metadata mapping.
/// </summary>
public class AuthAndNodeValidatorTests
{
	private static Identity ValidIdentity() => new IdentityBuilder("a@example.com").ForUser("u1").AsUserType().Build();

	private static ClusterMachine ValidClusterMachine() => new()
	{
		Metadata = new ResourceMetadata { Id = "cm1", Namespace = "default" },
		Spec = new ClusterMachineSpec { ClusterId = "cluster-1", Role = "worker" }
	};

	/// <summary>
	/// A well formed user passes validation.
	/// </summary>
	[Fact]
	public void UserValidator_WithValidUser_Passes() =>
		new UserValidator().Validate(new UserBuilder().AsAdmin().Build()).IsValid.Should().BeTrue();

	/// <summary>
	/// Unknown roles and a missing namespace are reported.
	/// </summary>
	[Fact]
	public void UserValidator_WithBadRoleAndNamespace_Fails()
	{
		var user = new UserBuilder().WithRole("Root").WithNamespace("").Build();

		var messages = new UserValidator().Validate(user).Errors.Select(e => e.ErrorMessage).ToList();

		messages.Should().Contain("Role must be one of: Admin, Operator, Reader, None").And.Contain("Namespace is required");
	}

	/// <summary>
	/// A well formed identity passes validation.
	/// </summary>
	[Fact]
	public void IdentityValidator_WithValidIdentity_Passes() =>
		new IdentityValidator().Validate(ValidIdentity()).IsValid.Should().BeTrue();

	/// <summary>
	/// A bad email, missing label and mismatched label are all reported.
	/// </summary>
	[Fact]
	public void IdentityValidator_WithBrokenIdentity_Fails()
	{
		var identity = new IdentityBuilder("not-an-email").Build();
		identity.Metadata.Labels[Identity.LabelType] = "robot";

		var messages = new IdentityValidator().Validate(identity).Errors.Select(e => e.ErrorMessage).ToList();

		messages.Should().Contain("Must be a valid email address")
			.And.Contain("User ID is required")
			.And.Contain($"Identity must have '{Identity.LabelUserID}' label")
			.And.Contain($"Identity type must be '{Identity.TypeUser}' or '{Identity.TypeServiceAccount}'");
	}

	/// <summary>
	/// A label that disagrees with the spec user id is rejected.
	/// </summary>
	[Fact]
	public void IdentityValidator_WithMismatchedLabel_Fails()
	{
		var identity = ValidIdentity();
		identity.Metadata.Labels[Identity.LabelUserID] = "other";

		new IdentityValidator().Validate(identity).Errors.Should().Contain(e => e.ErrorMessage == "Label user ID must match spec user ID");
	}

	/// <summary>
	/// A well formed cluster machine passes validation.
	/// </summary>
	[Fact]
	public void ClusterMachineValidator_WithValidMachine_Passes() =>
		new ClusterMachineValidator().Validate(ValidClusterMachine()).IsValid.Should().BeTrue();

	/// <summary>
	/// A bad cluster id and role are reported.
	/// </summary>
	[Fact]
	public void ClusterMachineValidator_WithBadClusterIdAndRole_Fails()
	{
		var machine = ValidClusterMachine();
		machine.Spec.ClusterId = "Not_Valid";
		machine.Spec.Role = "boss";

		var messages = new ClusterMachineValidator().Validate(machine).Errors.Select(e => e.ErrorMessage).ToList();

		messages.Should().Contain("Cluster ID must be a valid DNS-1123 subdomain")
			.And.Contain("Role must be either 'controlplane' or 'worker'");
	}

	/// <summary>
	/// Metadata survives a round trip through the COSI proto form.
	/// </summary>
	[Fact]
	public void ResourceMetadata_ProtoRoundTrip_PreservesFields()
	{
		var created = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
		var metadata = new ResourceMetadata
		{
			Id = "r1",
			Namespace = "ns",
			Type = "T",
			Version = "3",
			Owner = "me",
			Phase = "running",
			Created = created,
			Updated = created.AddHours(1),
			Finalizers = ["f1"],
			Labels = { ["l"] = "1" },
			Annotations = { ["a"] = "2" }
		};

		var restored = ResourceMetadata.FromProto(metadata.ToProto());

		restored.Id.Should().Be("r1");
		restored.Name.Should().Be("r1");
		restored.Created.Should().Be(created);
		restored.Updated.Should().Be(created.AddHours(1));
		restored.Finalizers.Should().Equal("f1");
		restored.Labels["l"].Should().Be("1");
		restored.Annotations["a"].Should().Be("2");
	}
}
