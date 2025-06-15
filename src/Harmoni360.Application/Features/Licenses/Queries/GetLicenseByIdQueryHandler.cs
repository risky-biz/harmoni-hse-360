using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Queries;

public class GetLicenseByIdQueryHandler : IRequestHandler<GetLicenseByIdQuery, LicenseDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetLicenseByIdQueryHandler> _logger;

    public GetLicenseByIdQueryHandler(
        IApplicationDbContext context,
        ILogger<GetLicenseByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LicenseDto?> Handle(GetLicenseByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var license = await _context.Licenses
                .Include(l => l.Attachments)
                .Include(l => l.Renewals)
                .Include(l => l.LicenseConditions)
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (license == null)
            {
                _logger.LogWarning("License with ID {LicenseId} not found", request.Id);
                return null;
            }

            return new LicenseDto
            {
                Id = license.Id,
                LicenseNumber = license.LicenseNumber,
                Title = license.Title,
                Description = license.Description,
                Type = license.Type.ToString(),
                Status = license.Status.ToString(),
                Priority = license.Priority.ToString(),

                // Basic License Information
                IssuingAuthority = license.IssuingAuthority,
                IssuingAuthorityContact = license.IssuingAuthorityContact,
                IssuedLocation = license.IssuedLocation,
                HolderId = license.HolderId,
                HolderName = license.HolderName,
                Department = license.Department,

                // Dates
                IssuedDate = license.IssuedDate,
                ExpiryDate = license.ExpiryDate,
                SubmittedDate = license.SubmittedDate,
                ApprovedDate = license.ApprovedDate,
                ActivatedDate = license.ActivatedDate,
                SuspendedDate = license.SuspendedDate,
                RevokedDate = license.RevokedDate,

                // Renewal Information
                RenewalRequired = license.RenewalRequired,
                RenewalPeriodDays = license.RenewalPeriodDays,
                NextRenewalDate = license.NextRenewalDate,
                AutoRenewal = license.AutoRenewal,
                RenewalProcedure = license.RenewalProcedure,

                // Regulatory Information
                RegulatoryFramework = license.RegulatoryFramework,
                ApplicableRegulations = license.ApplicableRegulations,
                ComplianceStandards = license.ComplianceStandards,

                // Scope and Coverage
                Scope = license.Scope,
                CoverageAreas = license.CoverageAreas,
                Restrictions = license.Restrictions,
                ConditionsText = license.Conditions,

                // Risk and Compliance
                RiskLevel = license.RiskLevel.ToString(),
                IsCriticalLicense = license.IsCriticalLicense,
                RequiresInsurance = license.RequiresInsurance,
                RequiredInsuranceAmount = license.RequiredInsuranceAmount,

                // Financial Information
                LicenseFee = license.LicenseFee,
                Currency = license.Currency,

                // Status Information
                StatusNotes = license.StatusNotes,

                // Collections (simplified)
                Attachments = license.Attachments.Select(a => new LicenseAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    UploadedBy = a.UploadedBy,
                    UploadedAt = a.UploadedAt,
                    AttachmentType = a.AttachmentType.ToString(),
                    Description = a.Description,
                    IsRequired = a.IsRequired,
                    ValidUntil = a.ValidUntil
                }).ToList(),

                Renewals = license.Renewals.Select(r => new LicenseRenewalDto
                {
                    Id = r.Id,
                    RenewalNumber = r.RenewalNumber,
                    ApplicationDate = r.ApplicationDate,
                    SubmittedDate = r.SubmittedDate,
                    ApprovedDate = r.ApprovedDate,
                    RejectedDate = r.RejectedDate,
                    NewExpiryDate = r.NewExpiryDate,
                    Status = r.Status.ToString(),
                    RenewalNotes = r.RenewalNotes,
                    RenewalFee = r.RenewalFee,
                    DocumentsRequired = r.DocumentsRequired,
                    InspectionRequired = r.InspectionRequired,
                    InspectionDate = r.InspectionDate,
                    ProcessedBy = r.ProcessedBy,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy
                }).ToList(),

                Conditions = license.LicenseConditions.Select(c => new LicenseConditionDto
                {
                    Id = c.Id,
                    ConditionType = c.ConditionType,
                    Description = c.Description,
                    IsMandatory = c.IsMandatory,
                    DueDate = c.DueDate,
                    Status = c.Status.ToString(),
                    ComplianceEvidence = c.ComplianceEvidence,
                    ComplianceDate = c.ComplianceDate,
                    VerifiedBy = c.VerifiedBy,
                    ResponsiblePerson = c.ResponsiblePerson,
                    Notes = c.Notes
                }).ToList(),

                // Audit Information
                CreatedAt = license.CreatedAt,
                CreatedBy = license.CreatedBy,
                UpdatedAt = license.LastModifiedAt,
                UpdatedBy = license.LastModifiedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving license with ID {LicenseId}", request.Id);
            throw;
        }
    }
}