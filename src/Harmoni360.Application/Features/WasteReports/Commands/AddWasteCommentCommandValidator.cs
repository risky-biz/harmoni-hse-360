using FluentValidation;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class AddWasteCommentCommandValidator : AbstractValidator<AddWasteCommentCommand>
{
    public AddWasteCommentCommandValidator()
    {
        RuleFor(x => x.WasteReportId)
            .GreaterThan(0).WithMessage("Waste report ID must be valid");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid comment type");

        RuleFor(x => x.CommentedById)
            .GreaterThan(0).WithMessage("Commenter ID must be valid");
    }
}