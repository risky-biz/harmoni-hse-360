using FluentValidation;

namespace HarmoniHSE360.Application.Features.CorrectiveActions.Commands;

public class CreateCorrectiveActionCommandValidator : AbstractValidator<CreateCorrectiveActionCommand>
{
    public CreateCorrectiveActionCommandValidator()
    {
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
            .Must(BeATodayOrFutureDate)
            .WithMessage("Due date must be today or in the future");

        RuleFor(x => x.IncidentId)
            .GreaterThan(0)
            .WithMessage("Invalid incident ID");
    }

    private static bool BeATodayOrFutureDate(DateTime dueDate)
    {
        return dueDate.Date >= DateTime.UtcNow.Date;
    }
}