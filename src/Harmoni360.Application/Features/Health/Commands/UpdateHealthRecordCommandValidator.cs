using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class UpdateHealthRecordCommandValidator : AbstractValidator<UpdateHealthRecordCommand>
{
    public UpdateHealthRecordCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid health record ID is required.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future.")
            .When(x => x.DateOfBirth.HasValue);

        // Validate reasonable age ranges
        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || IsValidAge(dob.Value))
            .WithMessage("Date of birth must result in an age between 0 and 120 years.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.BloodType)
            .IsInEnum().WithMessage("Valid blood type is required.")
            .When(x => x.BloodType.HasValue);

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Medical notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.MedicalNotes));
    }

    private static bool IsValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (DateTime.UtcNow < dateOfBirth.AddYears(age))
            age--;

        return age >= 0 && age <= 120;
    }
}