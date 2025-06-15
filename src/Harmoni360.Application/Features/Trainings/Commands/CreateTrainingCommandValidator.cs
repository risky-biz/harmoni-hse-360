using FluentValidation;

namespace Harmoni360.Application.Features.Trainings.Commands;

public class CreateTrainingCommandValidator : AbstractValidator<CreateTrainingCommand>
{
    public CreateTrainingCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Training title is required.")
            .MaximumLength(200).WithMessage("Training title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Training description is required.")
            .MaximumLength(2000).WithMessage("Training description must not exceed 2000 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid training type is required.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Valid training category is required.");

        RuleFor(x => x.DeliveryMethod)
            .IsInEnum().WithMessage("Valid delivery method is required.");

        RuleFor(x => x.ScheduledStartDate)
            .NotEmpty().WithMessage("Scheduled start date is required.")
            .GreaterThan(DateTime.Now).WithMessage("Scheduled start date must be in the future.");

        RuleFor(x => x.ScheduledEndDate)
            .NotEmpty().WithMessage("Scheduled end date is required.")
            .GreaterThan(x => x.ScheduledStartDate).WithMessage("Scheduled end date must be after start date.");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 hours.")
            .LessThanOrEqualTo(720).WithMessage("Duration cannot exceed 720 hours (30 days).");

        RuleFor(x => x.Venue)
            .NotEmpty().WithMessage("Training venue is required.")
            .MaximumLength(500).WithMessage("Venue must not exceed 500 characters.");

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).WithMessage("Maximum participants must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Maximum participants cannot exceed 1000.");

        RuleFor(x => x.MinParticipants)
            .GreaterThan(0).WithMessage("Minimum participants must be greater than 0.")
            .LessThanOrEqualTo(x => x.MaxParticipants).WithMessage("Minimum participants cannot exceed maximum participants.");

        RuleFor(x => x.InstructorName)
            .NotEmpty().WithMessage("Instructor name is required.")
            .MaximumLength(100).WithMessage("Instructor name must not exceed 100 characters.");

        RuleFor(x => x.PassingScore)
            .GreaterThanOrEqualTo(0).WithMessage("Passing score cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Passing score cannot exceed 100.");

        RuleFor(x => x.CostPerParticipant)
            .GreaterThanOrEqualTo(0).WithMessage("Cost per participant cannot be negative.");

        RuleFor(x => x.TotalBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Total budget cannot be negative.");

        // Online training validations
        RuleFor(x => x.OnlineLink)
            .NotEmpty()
            .When(x => x.DeliveryMethod == Domain.Enums.TrainingDeliveryMethod.Online || 
                      x.DeliveryMethod == Domain.Enums.TrainingDeliveryMethod.Hybrid)
            .WithMessage("Online link is required for online/hybrid training.")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.OnlineLink))
            .WithMessage("Online link must be a valid URL.");

        RuleFor(x => x.OnlinePlatform)
            .NotEmpty()
            .When(x => x.DeliveryMethod == Domain.Enums.TrainingDeliveryMethod.Online || 
                      x.DeliveryMethod == Domain.Enums.TrainingDeliveryMethod.Hybrid)
            .WithMessage("Online platform is required for online/hybrid training.");

        // Location validation for in-person training
        When(x => x.DeliveryMethod == Domain.Enums.TrainingDeliveryMethod.InPerson ||
                 x.DeliveryMethod == Domain.Enums.TrainingDeliveryMethod.Hybrid, () =>
        {
            RuleFor(x => x.VenueAddress)
                .NotEmpty().WithMessage("Venue address is required for in-person/hybrid training.");
        });

        // K3 compliance validation
        When(x => x.IsK3MandatoryTraining, () =>
        {
            RuleFor(x => x.K3RegulationReference)
                .NotEmpty().WithMessage("K3 regulation reference is required for K3 mandatory training.");
        });

        // Certification validation
        When(x => x.IssuesCertificate, () =>
        {
            RuleFor(x => x.CertificationType)
                .IsInEnum().WithMessage("Valid certification type is required when issuing certificates.");

            RuleFor(x => x.CertificateValidityPeriod)
                .IsInEnum().WithMessage("Valid certificate validity period is required when issuing certificates.");
        });

        // Requirement validation
        RuleForEach(x => x.Requirements).SetValidator(new CreateTrainingRequirementCommandValidator());
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class CreateTrainingRequirementCommandValidator : AbstractValidator<CreateTrainingRequirementCommand>
{
    public CreateTrainingRequirementCommandValidator()
    {
        RuleFor(x => x.RequirementDescription)
            .NotEmpty().WithMessage("Requirement description is required.")
            .MaximumLength(1000).WithMessage("Requirement description must not exceed 1000 characters.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Now)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future.");

        RuleFor(x => x.CompetencyLevel)
            .MaximumLength(100).WithMessage("Competency level must not exceed 100 characters.");

        RuleFor(x => x.VerificationMethod)
            .MaximumLength(500).WithMessage("Verification method must not exceed 500 characters.");
    }
}