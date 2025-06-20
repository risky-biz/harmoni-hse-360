using FluentValidation;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class DisableModuleCommandValidator : AbstractValidator<DisableModuleCommand>
{
    public DisableModuleCommandValidator()
    {
        RuleFor(x => x.ModuleType)
            .IsInEnum().WithMessage("Module type must be a valid module type.");

        RuleFor(x => x.Context)
            .MaximumLength(1000).WithMessage("Context cannot exceed 1000 characters.");

        // Business rule: Critical modules cannot be disabled unless forced
        RuleFor(x => x.ModuleType)
            .Must((command, moduleType) => command.ForceDisable || 
                  (moduleType != ModuleType.Dashboard && 
                   moduleType != ModuleType.UserManagement && 
                   moduleType != ModuleType.ApplicationSettings))
            .WithMessage("Critical modules (Dashboard, User Management, Application Settings) cannot be disabled unless forced.");
    }
}