using FluentValidation;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Features.Hazards.Commands;

public class CreateRiskAssessmentCommandValidator : AbstractValidator<CreateRiskAssessmentCommand>
{
    public CreateRiskAssessmentCommandValidator()
    {
        RuleFor(x => x.HazardId)
            .GreaterThan(0).WithMessage("Valid hazard ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid risk assessment type is required.");

        RuleFor(x => x.AssessorId)
            .GreaterThan(0).WithMessage("Valid assessor ID is required.");

        RuleFor(x => x.ProbabilityScore)
            .InclusiveBetween(1, 5).WithMessage("Probability score must be between 1 and 5.");

        RuleFor(x => x.SeverityScore)
            .InclusiveBetween(1, 5).WithMessage("Severity score must be between 1 and 5.");

        RuleFor(x => x.PotentialConsequences)
            .NotEmpty().WithMessage("Potential consequences are required.")
            .MaximumLength(1000).WithMessage("Potential consequences must not exceed 1000 characters.");

        RuleFor(x => x.ExistingControls)
            .NotEmpty().WithMessage("Existing controls description is required.")
            .MaximumLength(1000).WithMessage("Existing controls must not exceed 1000 characters.");

        RuleFor(x => x.RecommendedActions)
            .NotEmpty().WithMessage("Recommended actions are required.")
            .MaximumLength(1000).WithMessage("Recommended actions must not exceed 1000 characters.");

        RuleFor(x => x.AdditionalNotes)
            .MaximumLength(2000).WithMessage("Additional notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.AdditionalNotes));

        RuleFor(x => x.NextReviewDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Next review date must be in the future.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(2)).WithMessage("Next review date cannot be more than 2 years in the future.");

        // Business rules validation
        RuleFor(x => x)
            .Must(ValidateRiskMatrix).WithMessage("Risk assessment scores should be consistent with standard risk matrix guidelines.")
            .Must(ValidateReviewDateBasedOnRisk).WithMessage("High-risk assessments should have more frequent review dates.");
    }

    private static bool ValidateRiskMatrix(CreateRiskAssessmentCommand command)
    {
        // Calculate risk score
        var riskScore = command.ProbabilityScore * command.SeverityScore;
        
        // Validate that high-risk scenarios have appropriate controls
        if (riskScore >= 15) // High or Critical risk
        {
            // For high-risk scenarios, ensure detailed controls and actions are provided
            return !string.IsNullOrWhiteSpace(command.ExistingControls) &&
                   command.ExistingControls.Length >= 50 &&
                   !string.IsNullOrWhiteSpace(command.RecommendedActions) &&
                   command.RecommendedActions.Length >= 50;
        }

        return true;
    }

    private static bool ValidateReviewDateBasedOnRisk(CreateRiskAssessmentCommand command)
    {
        var riskScore = command.ProbabilityScore * command.SeverityScore;
        var daysDifference = (command.NextReviewDate - DateTime.UtcNow).TotalDays;

        return riskScore switch
        {
            >= 20 => daysDifference <= 30,    // Critical: Monthly review
            >= 15 => daysDifference <= 90,    // High: Quarterly review
            >= 10 => daysDifference <= 180,   // Medium: Semi-annual review
            >= 5 => daysDifference <= 365,    // Low: Annual review
            _ => daysDifference <= 730        // Very Low: Biennial review
        };
    }
}