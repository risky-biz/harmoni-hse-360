using FluentValidation;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class UpdateSecurityIncidentCommandValidator : AbstractValidator<UpdateSecurityIncidentCommand>
{
    public UpdateSecurityIncidentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid incident ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Valid severity is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Valid status is required");

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

        // Temporal validation
        RuleFor(x => x.DetectionDateTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .When(x => x.DetectionDateTime.HasValue)
            .WithMessage("Detection time cannot be more than 1 hour in the future");

        RuleFor(x => x.DetectionDateTime)
            .LessThanOrEqualTo(x => x.IncidentDateTime.AddDays(30))
            .When(x => x.DetectionDateTime.HasValue)
            .WithMessage("Detection time cannot be more than 30 days after incident time");

        RuleFor(x => x.ContainmentDateTime)
            .GreaterThanOrEqualTo(x => x.IncidentDateTime)
            .When(x => x.ContainmentDateTime.HasValue)
            .WithMessage("Containment time must be after incident time");

        RuleFor(x => x.ResolutionDateTime)
            .GreaterThanOrEqualTo(x => x.ContainmentDateTime ?? x.IncidentDateTime)
            .When(x => x.ResolutionDateTime.HasValue)
            .WithMessage("Resolution time must be after containment or incident time");

        // Status-specific validation
        RuleFor(x => x.ContainmentActions)
            .NotEmpty()
            .When(x => x.Status is SecurityIncidentStatus.Contained or SecurityIncidentStatus.Eradicating or SecurityIncidentStatus.Recovering or SecurityIncidentStatus.Resolved or SecurityIncidentStatus.Closed)
            .WithMessage("Containment actions are required for contained or resolved incidents");

        RuleFor(x => x.RootCause)
            .NotEmpty()
            .When(x => x.Status is SecurityIncidentStatus.Resolved or SecurityIncidentStatus.Closed)
            .WithMessage("Root cause is required for resolved or closed incidents");

        RuleFor(x => x.ResolutionDateTime)
            .NotNull()
            .When(x => x.Status is SecurityIncidentStatus.Resolved or SecurityIncidentStatus.Closed)
            .WithMessage("Resolution date is required for resolved or closed incidents");

        // Assignment validation
        RuleFor(x => x.AssignedToId)
            .GreaterThan(0)
            .When(x => x.AssignedToId.HasValue)
            .WithMessage("Valid assigned user ID is required");

        RuleFor(x => x.InvestigatorId)
            .GreaterThan(0)
            .When(x => x.InvestigatorId.HasValue)
            .WithMessage("Valid investigator user ID is required");

        // Business rule validation
        RuleFor(x => x.Status)
            .NotEqual(SecurityIncidentStatus.Closed)
            .When(x => string.IsNullOrEmpty(x.RootCause))
            .WithMessage("Cannot close incident without root cause analysis");
    }
}