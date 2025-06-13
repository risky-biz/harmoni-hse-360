using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class AddMedicalConditionCommandValidator : AbstractValidator<AddMedicalConditionCommand>
{
    public AddMedicalConditionCommandValidator()
    {
        RuleFor(x => x.HealthRecordId)
            .GreaterThan(0).WithMessage("Valid health record ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid condition type is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Condition name is required.")
            .MaximumLength(200).WithMessage("Condition name must not exceed 200 characters.")
            .Must(name => !ContainsForbiddenWords(name))
            .WithMessage("Condition name contains inappropriate content.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Condition description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Valid severity level is required.");

        RuleFor(x => x.TreatmentPlan)
            .MaximumLength(1000).WithMessage("Treatment plan must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.TreatmentPlan));

        RuleFor(x => x.DiagnosedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Diagnosed date cannot be in the future.")
            .When(x => x.DiagnosedDate.HasValue);

        // Emergency action validation
        RuleFor(x => x.EmergencyInstructions)
            .NotEmpty().WithMessage("Emergency instructions are required for conditions requiring emergency action.")
            .MaximumLength(1000).WithMessage("Emergency instructions must not exceed 1000 characters.")
            .When(x => x.RequiresEmergencyAction);

        RuleFor(x => x.EmergencyInstructions)
            .MaximumLength(1000).WithMessage("Emergency instructions must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.EmergencyInstructions));

        // Business rules for life-threatening conditions
        RuleFor(x => x)
            .Must(x => !x.RequiresEmergencyAction || x.Severity >= ConditionSeverity.Severe)
            .WithMessage("Conditions requiring emergency action must have severity of Severe or Life Threatening.");

        // Allergy-specific validation
        RuleFor(x => x.Severity)
            .Must(severity => severity >= ConditionSeverity.Moderate)
            .WithMessage("Allergies should typically be classified as Moderate or higher severity.")
            .When(x => x.Type == ConditionType.Allergy);

        // Life-threatening conditions must have emergency action
        RuleFor(x => x.RequiresEmergencyAction)
            .Equal(true).WithMessage("Life threatening conditions must require emergency action.")
            .When(x => x.Severity == ConditionSeverity.LifeThreatening);

        // Medication dependency validation
        RuleFor(x => x.TreatmentPlan)
            .NotEmpty().WithMessage("Treatment plan is required for medication dependencies.")
            .When(x => x.Type == ConditionType.MedicationDependency);
    }

    private static bool ContainsForbiddenWords(string text)
    {
        // Basic content filtering - can be expanded as needed
        var forbiddenWords = new[] { "test", "fake", "dummy" };
        return forbiddenWords.Any(word => text.ToLowerInvariant().Contains(word));
    }
}