using FluentValidation;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class CreateHealthRecordCommandValidator : AbstractValidator<CreateHealthRecordCommand>
{
    public CreateHealthRecordCommandValidator()
    {
        // Either PersonId OR PersonName+PersonEmail must be provided
        RuleFor(x => x)
            .Must(HaveValidPersonIdentification)
            .WithMessage("Either PersonId (for existing person) or PersonName + PersonEmail (for new person) must be provided");

        // If PersonId is provided, validate it
        When(x => x.PersonId.HasValue, () =>
        {
            RuleFor(x => x.PersonId!.Value)
                .GreaterThan(0)
                .WithMessage("PersonId must be greater than 0");
        });

        // If creating new person, validate required fields
        When(x => !x.PersonId.HasValue, () =>
        {
            RuleFor(x => x.PersonName)
                .NotEmpty()
                .WithMessage("PersonName is required when creating a new person")
                .MaximumLength(200)
                .WithMessage("PersonName cannot exceed 200 characters");

            RuleFor(x => x.PersonEmail)
                .NotEmpty()
                .WithMessage("PersonEmail is required when creating a new person")
                .EmailAddress()
                .WithMessage("PersonEmail must be a valid email address")
                .MaximumLength(256)
                .WithMessage("PersonEmail cannot exceed 256 characters");
        });

        // Validate PersonType
        RuleFor(x => x.PersonType)
            .IsInEnum()
            .WithMessage("PersonType must be a valid value");

        // Optional person field validations
        When(x => !string.IsNullOrWhiteSpace(x.PersonPhoneNumber), () =>
        {
            RuleFor(x => x.PersonPhoneNumber)
                .MaximumLength(50)
                .WithMessage("PersonPhoneNumber cannot exceed 50 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PersonDepartment), () =>
        {
            RuleFor(x => x.PersonDepartment)
                .MaximumLength(100)
                .WithMessage("PersonDepartment cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PersonPosition), () =>
        {
            RuleFor(x => x.PersonPosition)
                .MaximumLength(100)
                .WithMessage("PersonPosition cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PersonEmployeeId), () =>
        {
            RuleFor(x => x.PersonEmployeeId)
                .MaximumLength(50)
                .WithMessage("PersonEmployeeId cannot exceed 50 characters");
        });

        // Health record validations
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date of birth cannot be in the future")
            .When(x => x.DateOfBirth.HasValue);

        // For students, date of birth is typically required
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required for student records")
            .When(x => x.PersonType == PersonType.Student);

        // Validate age ranges based on person type
        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || IsValidStudentAge(dob.Value))
            .WithMessage("Student age must be between 3 and 25 years")
            .When(x => x.PersonType == PersonType.Student && x.DateOfBirth.HasValue);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || IsValidStaffAge(dob.Value))
            .WithMessage("Staff age must be between 18 and 80 years")
            .When(x => x.PersonType == PersonType.Staff && x.DateOfBirth.HasValue);

        RuleFor(x => x.BloodType)
            .IsInEnum()
            .WithMessage("Valid blood type is required")
            .When(x => x.BloodType.HasValue);

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000)
            .WithMessage("Medical notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.MedicalNotes));

        // Business rule: Medical notes should be provided for certain person types
        RuleFor(x => x.MedicalNotes)
            .NotEmpty()
            .WithMessage("Medical notes are recommended for visitor and contractor health records")
            .When(x => x.PersonType == PersonType.Visitor || x.PersonType == PersonType.Contractor);
    }

    private static bool HaveValidPersonIdentification(CreateHealthRecordCommand command)
    {
        // Either PersonId is provided OR (PersonName AND PersonEmail) are provided
        return command.PersonId.HasValue ||
               (!string.IsNullOrWhiteSpace(command.PersonName) && !string.IsNullOrWhiteSpace(command.PersonEmail));
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