using FluentValidation;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class CreateSecurityIncidentCommandValidator : AbstractValidator<CreateSecurityIncidentCommand>
{
    public CreateSecurityIncidentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.IncidentType)
            .IsInEnum().WithMessage("Valid incident type is required");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Valid category is required");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Valid severity is required");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.IncidentDateTime)
            .NotEmpty().WithMessage("Incident date and time is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("Incident date cannot be more than 1 hour in the future");

        // Geolocation validation
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180");

        // Threat actor validation
        RuleFor(x => x.ThreatActorDescription)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.ThreatActorDescription))
            .WithMessage("Threat actor description must not exceed 1000 characters");

        // Impact validation
        RuleFor(x => x.AffectedPersonsCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AffectedPersonsCount.HasValue)
            .WithMessage("Affected persons count must be zero or positive");

        RuleFor(x => x.EstimatedLoss)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedLoss.HasValue)
            .WithMessage("Estimated loss must be zero or positive");

        // Category-specific validation
        RuleFor(x => x.Category)
            .Must((command, category) => IsValidCategoryForType(command.IncidentType, category))
            .WithMessage("Category must be valid for the selected incident type");

        // Data breach validation
        RuleFor(x => x.DataBreachSuspected)
            .Equal(false)
            .When(x => x.IncidentType == SecurityIncidentType.PhysicalSecurity)
            .WithMessage("Data breach flag is not applicable for physical security incidents");

        // File validation
        RuleForEach(x => x.Attachments)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.Length)
                    .LessThanOrEqualTo(50 * 1024 * 1024) // 50MB limit
                    .WithMessage("File size must not exceed 50MB");
            })
            .When(x => x.Attachments != null);

        // Detection time validation
        RuleFor(x => x.DetectionDateTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .When(x => x.DetectionDateTime.HasValue)
            .WithMessage("Detection time cannot be more than 1 hour in the future");

        RuleFor(x => x.DetectionDateTime)
            .LessThanOrEqualTo(x => x.IncidentDateTime.AddDays(30))
            .When(x => x.DetectionDateTime.HasValue)
            .WithMessage("Detection time cannot be more than 30 days after incident time");
    }

    private static bool IsValidCategoryForType(SecurityIncidentType type, SecurityIncidentCategory category)
    {
        return type switch
        {
            SecurityIncidentType.PhysicalSecurity => IsPhysicalSecurityCategory(category),
            SecurityIncidentType.Cybersecurity => IsCybersecurityCategory(category),
            SecurityIncidentType.PersonnelSecurity => IsPersonnelSecurityCategory(category),
            SecurityIncidentType.InformationSecurity => IsInformationSecurityCategory(category),
            _ => false
        };
    }

    private static bool IsPhysicalSecurityCategory(SecurityIncidentCategory category)
    {
        return category is 
            SecurityIncidentCategory.UnauthorizedAccess or 
            SecurityIncidentCategory.Theft or 
            SecurityIncidentCategory.Vandalism or 
            SecurityIncidentCategory.PerimeterBreach or 
            SecurityIncidentCategory.SuspiciousActivity or 
            SecurityIncidentCategory.PhysicalThreat;
    }

    private static bool IsCybersecurityCategory(SecurityIncidentCategory category)
    {
        return category is 
            SecurityIncidentCategory.DataBreach or 
            SecurityIncidentCategory.MalwareInfection or 
            SecurityIncidentCategory.PhishingAttempt or 
            SecurityIncidentCategory.SystemIntrusion or 
            SecurityIncidentCategory.ServiceDisruption or 
            SecurityIncidentCategory.UnauthorizedChange;
    }

    private static bool IsPersonnelSecurityCategory(SecurityIncidentCategory category)
    {
        return category is 
            SecurityIncidentCategory.BackgroundCheckFailure or 
            SecurityIncidentCategory.PolicyViolation or 
            SecurityIncidentCategory.InsiderThreat or 
            SecurityIncidentCategory.CredentialMisuse or 
            SecurityIncidentCategory.SecurityTrainingFailure;
    }

    private static bool IsInformationSecurityCategory(SecurityIncidentCategory category)
    {
        return category is 
            SecurityIncidentCategory.DataBreach or 
            SecurityIncidentCategory.UnauthorizedChange or 
            SecurityIncidentCategory.PolicyViolation;
    }
}