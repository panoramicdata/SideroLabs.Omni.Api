using FluentValidation;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources.Validation;

public class MachineValidatorTests
{
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
