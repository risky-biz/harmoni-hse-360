using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Hazards.Commands;

public class CreateHazardCommandValidator : AbstractValidator<CreateHazardCommand>
{
    public CreateHazardCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Valid hazard category is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid hazard type is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Valid severity level is required.");

        RuleFor(x => x.ReporterId)
            .GreaterThan(0).WithMessage("Valid reporter ID is required.");

        RuleFor(x => x.ReporterDepartment)
            .NotEmpty().WithMessage("Reporter department is required.")
            .MaximumLength(100).WithMessage("Reporter department must not exceed 100 characters.");

        // Validate coordinates if provided
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.")
            .When(x => x.Longitude.HasValue);

        // If one coordinate is provided, both must be provided
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) || (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Both latitude and longitude must be provided together.");

        // Expected resolution date validation
        RuleFor(x => x.ExpectedResolutionDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expected resolution date must be in the future.")
            .When(x => x.ExpectedResolutionDate.HasValue);

        // File attachment validation
        RuleForEach(x => x.Attachments)
            .ChildRules(attachment =>
            {
                attachment.RuleFor(x => x.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB max
                    .WithMessage("File size must not exceed 10MB.");

                attachment.RuleFor(x => x.ContentType)
                    .Must(contentType => IsValidContentType(contentType))
                    .WithMessage("File type not allowed. Supported types: images, PDF, Word documents, Excel files.");
            })
            .When(x => x.Attachments?.Any() == true);

        // Maximum number of attachments
        RuleFor(x => x.Attachments)
            .Must(attachments => attachments == null || attachments.Count <= 10)
            .WithMessage("Maximum 10 attachments allowed per hazard report.");
    }

    private static bool IsValidContentType(string contentType)
    {
        var allowedTypes = new[]
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp",
            "application/pdf",
            "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain", "text/csv"
        };

        return allowedTypes.Contains(contentType?.ToLowerInvariant());
    }
}