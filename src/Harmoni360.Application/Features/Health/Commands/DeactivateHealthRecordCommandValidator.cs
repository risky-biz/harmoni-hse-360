using FluentValidation;

namespace Harmoni360.Application.Features.Health.Commands;

public class DeactivateHealthRecordCommandValidator : AbstractValidator<DeactivateHealthRecordCommand>
{
    public DeactivateHealthRecordCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid health record ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}