using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class CreateHealthRecordCommandValidator : AbstractValidator<CreateHealthRecordCommand>
{
    public CreateHealthRecordCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .GreaterThan(0).WithMessage("Valid person ID is required.");

        RuleFor(x => x.PersonType)
            .IsInEnum().WithMessage("Valid person type is required.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future.")
            .When(x => x.DateOfBirth.HasValue);

        // For students, date of birth is typically required
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required for student records.")
            .When(x => x.PersonType == PersonType.Student);

        // Validate age ranges based on person type
        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || IsValidStudentAge(dob.Value))
            .WithMessage("Student age must be between 3 and 25 years.")
            .When(x => x.PersonType == PersonType.Student && x.DateOfBirth.HasValue);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || IsValidStaffAge(dob.Value))
            .WithMessage("Staff age must be between 18 and 80 years.")
            .When(x => x.PersonType == PersonType.Staff && x.DateOfBirth.HasValue);

        RuleFor(x => x.BloodType)
            .IsInEnum().WithMessage("Valid blood type is required.")
            .When(x => x.BloodType.HasValue);

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Medical notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.MedicalNotes));

        // Business rule: Medical notes should be provided for certain person types
        RuleFor(x => x.MedicalNotes)
            .NotEmpty().WithMessage("Medical notes are recommended for visitor and contractor health records.")
            .When(x => x.PersonType == PersonType.Visitor || x.PersonType == PersonType.Contractor);
    }

    private static bool IsValidStudentAge(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (DateTime.UtcNow < dateOfBirth.AddYears(age))
            age--;

        return age >= 3 && age <= 25;
    }

    private static bool IsValidStaffAge(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (DateTime.UtcNow < dateOfBirth.AddYears(age))
            age--;

        return age >= 18 && age <= 80;
    }
}