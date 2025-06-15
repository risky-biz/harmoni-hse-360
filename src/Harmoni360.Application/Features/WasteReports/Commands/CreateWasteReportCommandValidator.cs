using FluentValidation;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class CreateWasteReportCommandValidator : AbstractValidator<CreateWasteReportCommand>
{
    public CreateWasteReportCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid waste category");

        RuleFor(x => x.GeneratedDate)
            .NotEmpty().WithMessage("Generated date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Generated date cannot be in the future");

        RuleFor(x => x.ReporterId)
            .GreaterThan(0).WithMessage("Reporter ID must be valid")
            .When(x => x.ReporterId.HasValue);
    }
}