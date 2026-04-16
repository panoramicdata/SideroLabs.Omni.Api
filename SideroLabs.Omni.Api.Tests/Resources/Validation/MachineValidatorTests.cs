using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;
using Machine = SideroLabs.Omni.Api.Resources.Machine;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

/// <summary>
/// Unit tests for MachineValidator FluentValidation rules.
/// </summary>
public class MachineValidatorTests
{
	/// <summary>
	/// Verifies that a Machine with a valid UUID ID and a recognized role passes all validation rules.
	/// </summary>
	[Fact]
	public void Validate_WithValidMachine_ReturnsSuccess()
	{
		// Arrange
		var machine = new Machine
		{
			Metadata = new ResourceMetadata 
			{ 
				Id = "550e8400-e29b-41d4-a716-446655440000",
				Namespace = "default" 
			},
			Spec = new MachineSpec
			{
				Role = "controlplane"
			}
		};

		// Act
		var result = machine.Validate();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	/// <summary>
	/// Verifies that a Machine without a role fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithoutRole_ReturnsError()
	{
		// Arrange
		var machine = new Machine
		{
			Metadata = new ResourceMetadata 
			{ 
				Id = "550e8400-e29b-41d4-a716-446655440000",
				Namespace = "default" 
			},
			Spec = new MachineSpec()
		};

		// Act
		var result = machine.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Spec.Role");
	}

	/// <summary>
	/// Verifies that a Machine with an unrecognized role value fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidRole_ReturnsError()
	{
		// Arrange
		var machine = new Machine
		{
			Metadata = new ResourceMetadata 
			{ 
				Id = "550e8400-e29b-41d4-a716-446655440000",
				Namespace = "default" 
			},
			Spec = new MachineSpec
			{
				Role = "invalid-role"
			}
		};

		// Act
		var result = machine.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Spec.Role" && 
			e.ErrorMessage.Contains("controlplane") &&
			e.ErrorMessage.Contains("worker"));
	}

	/// <summary>
	/// Verifies that a Machine with a non-UUID ID fails validation.
	/// </summary>
	[Fact]
	public void Validate_WithInvalidMachineId_ReturnsError()
	{
		// Arrange
		var machine = new Machine
		{
			Metadata = new ResourceMetadata 
			{ 
				Id = "not-a-uuid",
				Namespace = "default" 
			},
			Spec = new MachineSpec
			{
				Role = "worker"
			}
		};

		// Act
		var result = machine.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => 
			e.PropertyName == "Metadata.Id" && 
			e.ErrorMessage.Contains("UUID"));
	}

	/// <summary>
	/// Verifies that machines with 'controlplane' and 'worker' roles pass validation.
	/// </summary>
	[Theory]
	[InlineData("controlplane")]
	[InlineData("worker")]
	public void Validate_WithValidRoles_ReturnsSuccess(string role)
	{
		// Arrange
		var machine = new Machine
		{
			Metadata = new ResourceMetadata 
			{ 
				Id = "550e8400-e29b-41d4-a716-446655440000",
				Namespace = "default" 
			},
			Spec = new MachineSpec
			{
				Role = role
			}
		};

		// Act
		var result = machine.Validate();

		// Assert
		Assert.True(result.IsValid);
	}

	/// <summary>
	/// Verifies that ValidateAndThrow throws ValidationException for an invalid Machine.
	/// </summary>
	[Fact]
	public void ValidateAndThrow_WithInvalidMachine_ThrowsValidationException()
	{
		// Arrange
		var machine = new Machine
		{
			Metadata = new ResourceMetadata 
			{ 
				Id = "550e8400-e29b-41d4-a716-446655440000",
				Namespace = "default" 
			},
			Spec = new MachineSpec()
			// Missing Role
		};

		// Act & Assert
		Assert.Throws<ValidationException>(() => machine.ValidateAndThrow());
	}
}
