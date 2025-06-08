using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Hazards.Commands;

public class CreateMitigationActionCommandValidator : AbstractValidator<CreateMitigationActionCommand>
{
    public CreateMitigationActionCommandValidator()
    {
        RuleFor(x => x.HazardId)
            .GreaterThan(0).WithMessage("Valid hazard ID is required.");

        RuleFor(x => x.ActionDescription)
            .NotEmpty().WithMessage("Action description is required.")
            .MinimumLength(10).WithMessage("Action description must be at least 10 characters.")
            .MaximumLength(1000).WithMessage("Action description must not exceed 1000 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid mitigation action type is required.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Valid priority level is required.");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Target date must be in the future.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(2)).WithMessage("Target date cannot be more than 2 years in the future.");

        RuleFor(x => x.AssignedToId)
            .GreaterThan(0).WithMessage("Valid assigned user ID is required.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated cost cannot be negative.")
            .LessThanOrEqualTo(1000000).WithMessage("Estimated cost cannot exceed $1,000,000.")
            .When(x => x.EstimatedCost.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business rules validation
        RuleFor(x => x)
            .Must(ValidateTargetDateBasedOnPriority).WithMessage("Target date should align with action priority level.")
            .Must(ValidateActionTypeForHighRiskHazards).WithMessage("High-risk hazards should prioritize elimination or engineering controls over PPE.");
    }

    private static bool ValidateTargetDateBasedOnPriority(CreateMitigationActionCommand command)
    {
        var daysDifference = (command.TargetDate - DateTime.UtcNow).TotalDays;

        return command.Priority switch
        {
            MitigationPriority.Critical => daysDifference <= 7,    // Within 1 week
            MitigationPriority.High => daysDifference <= 30,       // Within 1 month
            MitigationPriority.Medium => daysDifference <= 90,     // Within 3 months
            MitigationPriority.Low => daysDifference <= 365,       // Within 1 year
            _ => true
        };
    }

    private static bool ValidateActionTypeForHighRiskHazards(CreateMitigationActionCommand command)
    {
        // This is a business rule validation - for high-priority actions,
        // prefer elimination/substitution/engineering controls over PPE (hierarchy of controls)
        if (command.Priority == MitigationPriority.Critical || 
            command.Priority == MitigationPriority.High)
        {
            // PPE should not be the primary control for critical/high priority items
            // unless it's temporary while implementing other controls
            if (command.Type == MitigationActionType.PPE)
            {
                // Allow PPE for high-priority if it includes "temporary" or "interim" in description
                return command.ActionDescription.ToLowerInvariant().Contains("temporary") ||
                       command.ActionDescription.ToLowerInvariant().Contains("interim") ||
                       command.ActionDescription.ToLowerInvariant().Contains("immediate");
            }
        }

        return true;
    }
}