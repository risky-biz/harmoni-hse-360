using FluentValidation;
using Harmoni360.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public class CreateDisposalProviderCommandValidator : AbstractValidator<CreateDisposalProviderCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateDisposalProviderCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Provider name is required")
            .MaximumLength(200)
            .WithMessage("Provider name cannot exceed 200 characters")
            .MustAsync(BeUniqueName)
            .WithMessage("A provider with this name already exists");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .WithMessage("License number is required")
            .MaximumLength(100)
            .WithMessage("License number cannot exceed 100 characters")
            .MustAsync(BeUniqueLicenseNumber)
            .WithMessage("A provider with this license number already exists");

        RuleFor(x => x.LicenseExpiryDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("License expiry date must be in the future");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _context.DisposalProviders
            .AnyAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    private async Task<bool> BeUniqueLicenseNumber(string licenseNumber, CancellationToken cancellationToken)
    {
        return !await _context.DisposalProviders
            .AnyAsync(p => p.LicenseNumber.ToLower() == licenseNumber.ToLower(), cancellationToken);
    }
}