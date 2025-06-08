using FluentValidation;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class CreateThreatAssessmentCommandValidator : AbstractValidator<CreateThreatAssessmentCommand>
{
    public CreateThreatAssessmentCommandValidator()
    {
        RuleFor(x => x.SecurityIncidentId)
            .GreaterThan(0).WithMessage("Valid security incident ID is required");

        RuleFor(x => x.ThreatLevel)
            .IsInEnum().WithMessage("Valid threat level is required");

        RuleFor(x => x.AssessmentRationale)
            .NotEmpty().WithMessage("Assessment rationale is required")
            .MaximumLength(2000).WithMessage("Assessment rationale must not exceed 2000 characters");

        // Risk factor validations (1-5 scale)
        RuleFor(x => x.ThreatCapability)
            .InclusiveBetween(1, 5).WithMessage("Threat capability must be between 1 and 5");

        RuleFor(x => x.ThreatIntent)
            .InclusiveBetween(1, 5).WithMessage("Threat intent must be between 1 and 5");

        RuleFor(x => x.TargetVulnerability)
            .InclusiveBetween(1, 5).WithMessage("Target vulnerability must be between 1 and 5");

        RuleFor(x => x.ImpactPotential)
            .InclusiveBetween(1, 5).WithMessage("Impact potential must be between 1 and 5");

        // Threat intelligence validation
        RuleFor(x => x.ThreatIntelSource)
            .NotEmpty()
            .When(x => x.ExternalThreatIntelUsed)
            .WithMessage("Threat intelligence source is required when external threat intel is used");

        RuleFor(x => x.ThreatIntelSource)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ThreatIntelSource))
            .WithMessage("Threat intelligence source must not exceed 200 characters");

        RuleFor(x => x.ThreatIntelDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ThreatIntelDetails))
            .WithMessage("Threat intelligence details must not exceed 2000 characters");

        // Assessment datetime validation
        RuleFor(x => x.AssessmentDateTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .When(x => x.AssessmentDateTime.HasValue)
            .WithMessage("Assessment date cannot be more than 5 minutes in the future");

        RuleFor(x => x.AssessmentDateTime)
            .GreaterThan(DateTime.UtcNow.AddYears(-1))
            .When(x => x.AssessmentDateTime.HasValue)
            .WithMessage("Assessment date cannot be more than 1 year in the past");
    }
}