using FluentValidation;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class UpdateWasteReportCommandValidator : AbstractValidator<UpdateWasteReportCommand>
{
    public UpdateWasteReportCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be valid");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid waste classification");

        RuleFor(x => x.EstimatedQuantity)
            .GreaterThan(0).When(x => x.EstimatedQuantity.HasValue)
            .WithMessage("Quantity must be greater than zero when specified");

        RuleFor(x => x.DisposalCost)
            .GreaterThanOrEqualTo(0).When(x => x.DisposalCost.HasValue)
            .WithMessage("Disposal cost must be non-negative when specified");
    }
}