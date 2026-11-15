using System.Linq.Expressions;
using FluentValidation;

namespace SideroLabs.Omni.Api.Resources.Validation;

internal static class ValidatorRules
{
	public static void RequireNamespace<T>(this AbstractValidator<T> validator) where T : IOmniResource =>
		validator.RuleFor(x => x.Metadata.Namespace)
			.NotEmpty()
			.WithMessage("Namespace is required");

	public static void RequireKind<T>(this AbstractValidator<T> validator, string kind) where T : IOmniResource
	{
		validator.RuleFor(x => x.Kind)
			.Equal(kind)
			.WithMessage($"Kind must be '{kind}'");

		validator.RuleFor(x => x.ApiVersion)
			.Equal("omni.sidero.dev/v1alpha1")
			.WithMessage("ApiVersion must be 'omni.sidero.dev/v1alpha1'");
	}

	public static void RequireNodeRole<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> role) =>
		validator.RuleFor(role)
			.NotEmpty()
			.WithMessage("Role is required")
			.Must(value => value == "controlplane" || value == "worker")
			.WithMessage("Role must be either 'controlplane' or 'worker'");
}