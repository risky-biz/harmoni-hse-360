using FluentValidation;
using Harmoni360.Domain.Enums;
using System.Text.Json;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class UpdateModuleSettingsCommandValidator : AbstractValidator<UpdateModuleSettingsCommand>
{
    public UpdateModuleSettingsCommandValidator()
    {
        RuleFor(x => x.ModuleType)
            .IsInEnum().WithMessage("Module type must be a valid module type.");

        RuleFor(x => x.Context)
            .MaximumLength(1000).WithMessage("Context cannot exceed 1000 characters.");

        RuleFor(x => x.Settings)
            .MaximumLength(10000).WithMessage("Settings cannot exceed 10000 characters.");

        // Validate JSON format if settings are provided
        RuleFor(x => x.Settings)
            .Must(BeValidJsonOrNull)
            .WithMessage("Settings must be valid JSON format.")
            .When(x => !string.IsNullOrEmpty(x.Settings));
    }

    private bool BeValidJsonOrNull(string? settings)
    {
        if (string.IsNullOrEmpty(settings))
            return true;

        try
        {
            JsonDocument.Parse(settings);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}