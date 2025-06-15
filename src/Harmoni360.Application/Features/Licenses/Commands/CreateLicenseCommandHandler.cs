using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class CreateLicenseCommandHandler : IRequestHandler<CreateLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateLicenseCommandHandler> _logger;

    public CreateLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(CreateLicenseCommand request, CancellationToken cancellationToken)
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

            // Generate license number if not provided
            var licenseNumber = string.IsNullOrEmpty(request.LicenseNumber) 
                ? await GenerateLicenseNumber(request.Type, cancellationToken)
                : request.LicenseNumber;

            // Create the license using the actual Create method
            var license = License.Create(
                request.Title,
                request.Description,
                request.Type,
                licenseNumber,
                request.IssuingAuthority,
                request.IssuedDate,
                request.ExpiryDate,
                _currentUserService.UserId,
                request.HolderName,
                request.Department,
                request.Priority);

            // Update additional properties using available methods
            if (!string.IsNullOrEmpty(request.Scope) || 
                !string.IsNullOrEmpty(request.Restrictions) || 
                !string.IsNullOrEmpty(request.Conditions))
            {
                license.UpdateDetails(
                    request.Title,
                    request.Description,
                    request.Scope,
                    request.Restrictions,
                    request.Conditions,
                    request.Priority);
            }

            // Set additional properties that have setters in the entity
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License created successfully. ID: {LicenseId}, Number: {LicenseNumber}", 
                license.Id, license.LicenseNumber);

            // Return DTO using the same mapping logic as other handlers
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating license for user {UserId}", _currentUserService.UserId);
            throw;
        }
    }

    private async Task<string> GenerateLicenseNumber(Domain.Enums.LicenseType type, CancellationToken cancellationToken)
    {
        var prefix = type.ToString().Substring(0, Math.Min(3, type.ToString().Length)).ToUpper();
        var year = DateTime.UtcNow.Year.ToString()[2..];
        
        var count = await _context.Licenses
            .Where(l => l.Type == type && l.CreatedAt.Year == DateTime.UtcNow.Year)
            .CountAsync(cancellationToken);
            
        return $"{prefix}-{year}-{(count + 1):D4}";
    }
}