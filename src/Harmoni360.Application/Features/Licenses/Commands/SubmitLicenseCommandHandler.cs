using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class SubmitLicenseCommandHandler : IRequestHandler<SubmitLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SubmitLicenseCommandHandler> _logger;

    public SubmitLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<SubmitLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(SubmitLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Get the license
            var license = await _context.Licenses
                .Include(l => l.Attachments)
                .Include(l => l.Renewals)
                .Include(l => l.LicenseConditions)
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (license == null)
            {
                throw new InvalidOperationException($"License with ID {request.Id} not found.");
            }

            // Check if license can be submitted (only Draft licenses can be submitted)
            if (license.Status != Domain.Enums.LicenseStatus.Draft)
            {
                throw new InvalidOperationException($"License with status '{license.Status}' cannot be submitted. Only Draft licenses can be submitted.");
            }

            // Submit the license
            license.Submit(currentUser.Name);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License submitted successfully. ID: {LicenseId}, Number: {LicenseNumber}, SubmittedBy: {SubmittedBy}", 
                license.Id, license.LicenseNumber, currentUser.Name);

            // Return DTO using the same mapping logic
            return await MapToDto(license);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting license {LicenseId} for user {UserId}", request.Id, _currentUserService.UserId);
            throw;
        }
    }

    private async Task<LicenseDto> MapToDto(Domain.Entities.License license)
    {
        // Simple mapping using only properties that exist in the License entity
        return new LicenseDto
        {
            Id = license.Id,
            LicenseNumber = license.LicenseNumber,
            Title = license.Title,
            Description = license.Description,
            Type = license.Type.ToString(),
            Status = license.Status.ToString(),
            Priority = license.Priority.ToString(),
            
            IssuingAuthority = license.IssuingAuthority,
            IssuingAuthorityContact = license.IssuingAuthorityContact,
            IssuedLocation = license.IssuedLocation,
            HolderId = license.HolderId,
            HolderName = license.HolderName,
            Department = license.Department,
            
            IssuedDate = license.IssuedDate,
            ExpiryDate = license.ExpiryDate,
            SubmittedDate = license.SubmittedDate,
            ApprovedDate = license.ApprovedDate,
            ActivatedDate = license.ActivatedDate,
            SuspendedDate = license.SuspendedDate,
            RevokedDate = license.RevokedDate,
            
            RenewalRequired = license.RenewalRequired,
            RenewalPeriodDays = license.RenewalPeriodDays,
            NextRenewalDate = license.NextRenewalDate,
            AutoRenewal = license.AutoRenewal,
            RenewalProcedure = license.RenewalProcedure,
            
            RegulatoryFramework = license.RegulatoryFramework,
            ApplicableRegulations = license.ApplicableRegulations,
            ComplianceStandards = license.ComplianceStandards,
            
            Scope = license.Scope,
            CoverageAreas = license.CoverageAreas,
            Restrictions = license.Restrictions,
            ConditionsText = license.Conditions,
            
            RiskLevel = license.RiskLevel.ToString(),
            IsCriticalLicense = license.IsCriticalLicense,
            RequiresInsurance = license.RequiresInsurance,
            RequiredInsuranceAmount = license.RequiredInsuranceAmount,
            
            LicenseFee = license.LicenseFee,
            Currency = license.Currency,
            
            StatusNotes = license.StatusNotes,
            
            CreatedAt = license.CreatedAt,
            CreatedBy = license.CreatedBy,
            UpdatedAt = license.LastModifiedAt,
            UpdatedBy = license.LastModifiedBy
        };
    }
}