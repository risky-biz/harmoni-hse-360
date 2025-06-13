using FluentValidation;

namespace Harmoni360.Application.Features.Health.Commands;

public class SetVaccinationExemptionCommandValidator : AbstractValidator<SetVaccinationExemptionCommand>
{
    public SetVaccinationExemptionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid vaccination record ID is required.");

        RuleFor(x => x.ExemptionReason)
            .NotEmpty().WithMessage("Exemption reason is required when setting an exemption.")
            .MaximumLength(500).WithMessage("Exemption reason must not exceed 500 characters.")
            .Must(reason => IsValidExemptionReason(reason))
            .WithMessage("Exemption reason must be medically or legally valid.")
            .When(x => !x.RemoveExemption);

        RuleFor(x => x.ExemptionReason)
            .Empty().WithMessage("Exemption reason should be empty when removing exemption.")
            .When(x => x.RemoveExemption);

        // Business rule: Exemption reasons should be substantial
        RuleFor(x => x.ExemptionReason)
            .MinimumLength(10).WithMessage("Exemption reason should provide sufficient detail for audit purposes.")
            .When(x => !x.RemoveExemption && !string.IsNullOrEmpty(x.ExemptionReason));
    }

    private static bool IsValidExemptionReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return false;

        // Check for valid exemption keywords
        var validReasons = new[]
        {
            "medical", "allergy", "contraindication", "religious", "philosophical", 
            "previous adverse reaction", "immune compromised", "pregnancy", 
            "temporary deferral", "physician recommendation"
        };

        var lowerReason = reason.ToLowerInvariant();
        return validReasons.Any(validReason => lowerReason.Contains(validReason));
    }
}