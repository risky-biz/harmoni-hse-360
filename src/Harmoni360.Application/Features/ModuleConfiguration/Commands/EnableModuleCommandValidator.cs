using FluentValidation;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class EnableModuleCommandValidator : AbstractValidator<EnableModuleCommand>
{
    public EnableModuleCommandValidator()
    {
        RuleFor(x => x.ModuleType)
            .IsInEnum().WithMessage("Module type must be a valid module type.");

        RuleFor(x => x.Context)
            .MaximumLength(1000).WithMessage("Context cannot exceed 1000 characters.");
    }
}