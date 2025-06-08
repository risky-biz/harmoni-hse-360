using FluentValidation;

namespace Harmoni360.Application.Features.Health.Commands;

public class RecordVaccinationCommandValidator : AbstractValidator<RecordVaccinationCommand>
{
    public RecordVaccinationCommandValidator()
    {
        RuleFor(x => x.HealthRecordId)
            .GreaterThan(0).WithMessage("Valid health record ID is required.");

        RuleFor(x => x.VaccineName)
            .NotEmpty().WithMessage("Vaccine name is required.")
            .MaximumLength(200).WithMessage("Vaccine name must not exceed 200 characters.")
            .Must(name => IsValidVaccineName(name))
            .WithMessage("Vaccine name must contain only alphanumeric characters, spaces, hyphens, and parentheses.");

        RuleFor(x => x.DateAdministered)
            .NotEmpty().WithMessage("Date administered is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date administered cannot be in the future.")
            .GreaterThan(DateTime.UtcNow.AddYears(-10)).WithMessage("Date administered cannot be more than 10 years ago.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.DateAdministered).WithMessage("Expiry date must be after the date administered.")
            .LessThan(DateTime.UtcNow.AddYears(20)).WithMessage("Expiry date cannot be more than 20 years in the future.")
            .When(x => x.ExpiryDate.HasValue);

        RuleFor(x => x.BatchNumber)
            .MaximumLength(100).WithMessage("Batch number must not exceed 100 characters.")
            .Must(batch => IsValidBatchNumber(batch))
            .WithMessage("Batch number contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.BatchNumber));

        RuleFor(x => x.AdministeredBy)
            .NotEmpty().WithMessage("Administrator name is required.")
            .MaximumLength(200).WithMessage("Administrator name must not exceed 200 characters.")
            .Must(name => IsValidPersonName(name))
            .WithMessage("Administrator name contains invalid characters.");

        RuleFor(x => x.AdministrationLocation)
            .MaximumLength(200).WithMessage("Administration location must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.AdministrationLocation));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business rules for required vaccinations
        RuleFor(x => x.ExpiryDate)
            .NotNull().WithMessage("Expiry date is required for mandatory vaccinations.")
            .When(x => x.IsRequired && IsRequiredVaccineWithExpiry(x.VaccineName));

        // Batch number should be provided for most vaccines
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is strongly recommended for vaccination tracking.")
            .When(x => string.IsNullOrEmpty(x.BatchNumber))
            .WithSeverity(Severity.Warning);

        // Administration location validation
        RuleFor(x => x.AdministrationLocation)
            .NotEmpty().WithMessage("Administration location is recommended for audit purposes.")
            .When(x => string.IsNullOrEmpty(x.AdministrationLocation))
            .WithSeverity(Severity.Warning);
    }

    private static bool IsValidVaccineName(string name)
    {
        // Allow alphanumeric characters, spaces, hyphens, parentheses, and common vaccine name patterns
        return !string.IsNullOrEmpty(name) && 
               name.All(c => char.IsLetterOrDigit(c) || 
                           char.IsWhiteSpace(c) || 
                           c == '-' || c == '(' || c == ')' || c == '/');
    }

    private static bool IsValidBatchNumber(string? batchNumber)
    {
        if (string.IsNullOrEmpty(batchNumber)) return true;
        
        // Allow alphanumeric characters, hyphens, and common batch number patterns
        return batchNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '.');
    }

    private static bool IsValidPersonName(string name)
    {
        // Allow letters, spaces, hyphens, apostrophes, and periods for names
        return !string.IsNullOrEmpty(name) &&
               name.All(c => char.IsLetter(c) || 
                           char.IsWhiteSpace(c) || 
                           c == '-' || c == '\'' || c == '.');
    }

    private static bool IsRequiredVaccineWithExpiry(string vaccineName)
    {
        // List of vaccines that typically require expiry tracking
        var vaccinesWithExpiry = new[]
        {
            "tetanus", "diphtheria", "hepatitis", "influenza", "covid", "measles", "mumps", "rubella"
        };

        return vaccinesWithExpiry.Any(vaccine => 
            vaccineName.ToLowerInvariant().Contains(vaccine));
    }
}