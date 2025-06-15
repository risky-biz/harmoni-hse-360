using FluentValidation;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WorkPermits.Commands;

public class CreateWorkPermitCommandValidator : AbstractValidator<CreateWorkPermitCommand>
{
    public CreateWorkPermitCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid work permit type is required.");

        RuleFor(x => x.WorkLocation)
            .NotEmpty().WithMessage("Work location is required.")
            .MaximumLength(300).WithMessage("Work location must not exceed 300 characters.");

        RuleFor(x => x.PlannedStartDate)
            .NotEmpty().WithMessage("Planned start date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Planned start date cannot be in the past.");

        RuleFor(x => x.PlannedEndDate)
            .NotEmpty().WithMessage("Planned end date is required.")
            .GreaterThan(x => x.PlannedStartDate).WithMessage("Planned end date must be after start date.");

        RuleFor(x => x.WorkScope)
            .NotEmpty().WithMessage("Work scope is required.")
            .MaximumLength(2000).WithMessage("Work scope must not exceed 2000 characters.");

        RuleFor(x => x.NumberOfWorkers)
            .GreaterThan(0).WithMessage("Number of workers must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Number of workers seems unrealistic. Please verify.");

        RuleFor(x => x.ContactPhone)
            .NotEmpty().WithMessage("Contact phone is required.")
            .Matches(@"^[\+]?[0-9\-\(\)\s]{7,20}$").WithMessage("Please provide a valid phone number.");

        // Conditional validations for specific permit types
        RuleFor(x => x.WorkSupervisor)
            .NotEmpty().WithMessage("Work supervisor is required for this type of work.")
            .When(x => x.Type == WorkPermitType.HotWork || 
                      x.Type == WorkPermitType.ConfinedSpace || 
                      x.Type == WorkPermitType.ElectricalWork ||
                      x.Type == WorkPermitType.Special);

        RuleFor(x => x.SafetyOfficer)
            .NotEmpty().WithMessage("Safety officer assignment is required for high-risk work.")
            .When(x => x.Type == WorkPermitType.HotWork || 
                      x.Type == WorkPermitType.ConfinedSpace || 
                      x.Type == WorkPermitType.Special);

        // Indonesian K3 compliance validations
        RuleFor(x => x.K3LicenseNumber)
            .NotEmpty().WithMessage("K3 License Number is required for compliance.")
            .When(x => x.Type == WorkPermitType.HotWork || 
                      x.Type == WorkPermitType.ConfinedSpace || 
                      x.Type == WorkPermitType.ElectricalWork ||
                      x.Type == WorkPermitType.Special);

        RuleFor(x => x.CompanyWorkPermitNumber)
            .NotEmpty().WithMessage("Company Work Permit Number is required.")
            .When(x => x.ContractorCompany != null && !string.IsNullOrEmpty(x.ContractorCompany));

        // Hot work specific validations
        RuleFor(x => x.RequiresFireWatch)
            .Equal(true).WithMessage("Fire watch is mandatory for hot work.")
            .When(x => x.Type == WorkPermitType.HotWork || x.RequiresHotWorkPermit);

        // Confined space specific validations
        RuleFor(x => x.RequiresGasMonitoring)
            .Equal(true).WithMessage("Gas monitoring is mandatory for confined space work.")
            .When(x => x.Type == WorkPermitType.ConfinedSpace || x.RequiresConfinedSpaceEntry);

        // Location validations
        When(x => x.Latitude.HasValue || x.Longitude.HasValue, () => {
            RuleFor(x => x.Latitude)
                .NotNull().WithMessage("Latitude is required when longitude is provided.")
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(x => x.Longitude)
                .NotNull().WithMessage("Longitude is required when latitude is provided.")
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");
        });

        // Duration validation
        RuleFor(x => x)
            .Must(HaveReasonableDuration)
            .WithMessage("Work duration seems unrealistic. Please verify dates.")
            .WithName("Duration");

        // Hazards validation
        RuleForEach(x => x.Hazards).SetValidator(new CreateWorkPermitHazardValidator());

        // Precautions validation
        RuleForEach(x => x.Precautions).SetValidator(new CreateWorkPermitPrecautionValidator());

        // Risk assessment requirement for high-risk work
        RuleFor(x => x.RiskAssessmentSummary)
            .NotEmpty().WithMessage("Risk assessment summary is required for high-risk work permits.")
            .When(x => x.Type == WorkPermitType.HotWork || 
                      x.Type == WorkPermitType.ConfinedSpace || 
                      x.Type == WorkPermitType.Special ||
                      x.RequiresHotWorkPermit ||
                      x.RequiresConfinedSpaceEntry ||
                      x.RequiresRadiationWork);

        RuleFor(x => x.EmergencyProcedures)
            .NotEmpty().WithMessage("Emergency procedures are required for high-risk work permits.")
            .When(x => x.Type == WorkPermitType.HotWork || 
                      x.Type == WorkPermitType.ConfinedSpace || 
                      x.Type == WorkPermitType.Special ||
                      x.RequiresHotWorkPermit ||
                      x.RequiresConfinedSpaceEntry ||
                      x.RequiresRadiationWork);
    }

    private bool HaveReasonableDuration(CreateWorkPermitCommand command)
    {
        var duration = command.PlannedEndDate - command.PlannedStartDate;
        return duration.TotalDays <= 365; // Max 1 year duration
    }
}

public class CreateWorkPermitHazardValidator : AbstractValidator<CreateWorkPermitHazardCommand>
{
    public CreateWorkPermitHazardValidator()
    {
        RuleFor(x => x.HazardDescription)
            .NotEmpty().WithMessage("Hazard description is required.")
            .MaximumLength(1000).WithMessage("Hazard description must not exceed 1000 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Valid hazard category is required.");

        RuleFor(x => x.Likelihood)
            .InclusiveBetween(1, 5).WithMessage("Likelihood must be between 1 and 5.");

        RuleFor(x => x.Severity)
            .InclusiveBetween(1, 5).WithMessage("Severity must be between 1 and 5.");

        RuleFor(x => x.ControlMeasures)
            .NotEmpty().WithMessage("Control measures are required.")
            .MaximumLength(2000).WithMessage("Control measures must not exceed 2000 characters.");

        RuleFor(x => x.ResponsiblePerson)
            .MaximumLength(100).WithMessage("Responsible person name must not exceed 100 characters.");
    }
}

public class CreateWorkPermitPrecautionValidator : AbstractValidator<CreateWorkPermitPrecautionCommand>
{
    public CreateWorkPermitPrecautionValidator()
    {
        RuleFor(x => x.PrecautionDescription)
            .NotEmpty().WithMessage("Precaution description is required.")
            .MaximumLength(1000).WithMessage("Precaution description must not exceed 1000 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Valid precaution category is required.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5.");

        RuleFor(x => x.ResponsiblePerson)
            .MaximumLength(100).WithMessage("Responsible person name must not exceed 100 characters.");

        RuleFor(x => x.VerificationMethod)
            .MaximumLength(500).WithMessage("Verification method must not exceed 500 characters.");

        RuleFor(x => x.K3StandardReference)
            .MaximumLength(200).WithMessage("K3 standard reference must not exceed 200 characters.");

        // K3 requirement validation
        RuleFor(x => x.K3StandardReference)
            .NotEmpty().WithMessage("K3 standard reference is required for K3 compliance requirements.")
            .When(x => x.IsK3Requirement);
    }
}