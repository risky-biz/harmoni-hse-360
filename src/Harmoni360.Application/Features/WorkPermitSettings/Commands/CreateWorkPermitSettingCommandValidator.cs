using FluentValidation;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class CreateWorkPermitSettingCommandValidator : AbstractValidator<CreateWorkPermitSettingCommand>
{
    public CreateWorkPermitSettingCommandValidator()
    {
        RuleFor(x => x.MaxAttachmentSizeMB)
            .GreaterThan(0).WithMessage("Maximum attachment size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Maximum attachment size cannot exceed 100 MB.");

        RuleFor(x => x.FormInstructions)
            .MaximumLength(2000).WithMessage("Form instructions cannot exceed 2000 characters.");

        // Business rule: If safety induction is required, provide clear instructions
        RuleFor(x => x.FormInstructions)
            .NotEmpty().WithMessage("Form instructions are required when safety induction is enabled.")
            .When(x => x.RequireSafetyInduction);
    }
}