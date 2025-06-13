using FluentValidation;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(255).WithMessage("Department must not exceed 255 characters");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required")
            .MaximumLength(255).WithMessage("Position must not exceed 255 characters");

        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("Role IDs cannot be null");

        RuleForEach(x => x.RoleIds)
            .GreaterThan(0).WithMessage("Role ID must be greater than 0");
    }
}