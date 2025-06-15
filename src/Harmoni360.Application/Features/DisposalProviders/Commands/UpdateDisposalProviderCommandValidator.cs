using FluentValidation;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public class UpdateDisposalProviderCommandValidator : AbstractValidator<UpdateDisposalProviderCommand>
{
    public UpdateDisposalProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Provider ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Provider name is required")
            .MaximumLength(200)
            .WithMessage("Provider name cannot exceed 200 characters");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .WithMessage("License number is required")
            .MaximumLength(100)
            .WithMessage("License number cannot exceed 100 characters");

        RuleFor(x => x.LicenseExpiryDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("License expiry date must be in the future");
    }
}