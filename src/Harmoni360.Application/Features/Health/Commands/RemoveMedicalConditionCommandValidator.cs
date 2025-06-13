using FluentValidation;

namespace Harmoni360.Application.Features.Health.Commands;

public class RemoveMedicalConditionCommandValidator : AbstractValidator<RemoveMedicalConditionCommand>
{
    public RemoveMedicalConditionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid medical condition ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        // Business rule: Recommend providing a reason for removal
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason for removal is strongly recommended for audit purposes.")
            .WithSeverity(Severity.Warning);
    }
}