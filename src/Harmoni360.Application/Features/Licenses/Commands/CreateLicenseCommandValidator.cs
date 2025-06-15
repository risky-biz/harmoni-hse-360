using FluentValidation;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class CreateLicenseCommandValidator : AbstractValidator<CreateLicenseCommand>
{
    public CreateLicenseCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Valid license type is required.");

        RuleFor(v => v.IssuingAuthority)
            .NotEmpty().WithMessage("Issuing authority is required.")
            .MaximumLength(200).WithMessage("Issuing authority must not exceed 200 characters.");

        RuleFor(v => v.HolderName)
            .NotEmpty().WithMessage("License holder name is required.")
            .MaximumLength(200).WithMessage("License holder name must not exceed 200 characters.");

        RuleFor(v => v.Department)
            .MaximumLength(200).WithMessage("Department must not exceed 200 characters.");

        RuleFor(v => v.ExpiryDate)
            .NotEmpty().WithMessage("Expiry date is required.")
            .GreaterThan(v => v.IssuedDate).WithMessage("Expiry date must be after issued date.");

        RuleFor(v => v.IssuedDate)
            .NotEmpty().WithMessage("Issued date is required.");

        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Valid priority is required.");

        RuleFor(v => v.RiskLevel)
            .IsInEnum().WithMessage("Valid risk level is required.");

        RuleFor(v => v.LicenseFee)
            .GreaterThanOrEqualTo(0).When(v => v.LicenseFee.HasValue)
            .WithMessage("License fee must be greater than or equal to 0.");

        RuleFor(v => v.RequiredInsuranceAmount)
            .GreaterThan(0).When(v => v.RequiresInsurance && v.RequiredInsuranceAmount.HasValue)
            .WithMessage("Required insurance amount must be greater than 0 when insurance is required.");

        RuleFor(v => v.RenewalPeriodDays)
            .GreaterThan(0).WithMessage("Renewal period days must be greater than 0.");

        RuleFor(v => v.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-character code.");
    }
}