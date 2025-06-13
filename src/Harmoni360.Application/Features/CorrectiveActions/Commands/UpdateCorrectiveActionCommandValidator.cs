using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.CorrectiveActions.Commands;

public class UpdateCorrectiveActionCommandValidator : AbstractValidator<UpdateCorrectiveActionCommand>
{
    public UpdateCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invalid corrective action ID");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.AssignedToDepartment)
            .NotEmpty()
            .WithMessage("Assigned department is required")
            .MaximumLength(100)
            .WithMessage("Assigned department must not exceed 100 characters");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required")
            .Must(BeAFutureOrTodayDate)
            .WithMessage("Due date cannot be in the past");

        RuleFor(x => x.CompletionNotes)
            .MaximumLength(1000)
            .WithMessage("Completion notes must not exceed 1000 characters");

        RuleFor(x => x.Status)
            .Must(status => status == ActionStatus.Completed)
            .When(x => !string.IsNullOrEmpty(x.CompletionNotes))
            .WithMessage("Completion notes can only be provided when status is Completed");
    }

    private static bool BeAFutureOrTodayDate(DateTime dueDate)
    {
        return dueDate.Date >= DateTime.UtcNow.Date;
    }
}