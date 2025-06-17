using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class UpdateLicenseCommandHandler : IRequestHandler<UpdateLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateLicenseCommandHandler> _logger;

    public UpdateLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(UpdateLicenseCommand request, CancellationToken cancellationToken)
    {
        var license = await _context.Licenses
            .Include(l => l.Attachments)
            .Include(l => l.Renewals)
            .Include(l => l.LicenseConditions)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (license == null)
        {
            throw new KeyNotFoundException($"License with ID {request.Id} not found.");
        }

        // Check if license can be edited (only Draft or Rejected status)
        if (license.Status != LicenseStatus.Draft && license.Status != LicenseStatus.Rejected)
        {
            throw new InvalidOperationException($"Cannot edit license in {license.Status} status. Only Draft or Rejected licenses can be edited.");
        }

        // Update basic details using the available method
        license.UpdateDetails(
            request.Title,
            request.Description,
            request.Scope,
            request.Restrictions,
            request.Conditions,
            request.Priority);

        // Update regulatory information using available method
        if (!string.IsNullOrWhiteSpace(request.RegulatoryFramework) ||
            !string.IsNullOrWhiteSpace(request.ApplicableRegulations) ||
            !string.IsNullOrWhiteSpace(request.ComplianceStandards))
        {
            license.SetRegulatoryInformation(
                request.RegulatoryFramework,
                request.ApplicableRegulations,
                request.ComplianceStandards);
        }

        // Update risk and compliance information
        if (request.RiskLevel.HasValue || request.IsCriticalLicense.HasValue || 
            request.RequiresInsurance.HasValue || request.RequiredInsuranceAmount.HasValue)
        {
            license.SetRiskAndCompliance(
                request.RiskLevel ?? license.RiskLevel,
                request.IsCriticalLicense ?? license.IsCriticalLicense,
                request.RequiresInsurance ?? license.RequiresInsurance,
                request.RequiredInsuranceAmount ?? license.RequiredInsuranceAmount);
        }

        // Update renewal information
        if (request.RenewalRequired.HasValue || request.RenewalPeriodDays.HasValue || 
            request.AutoRenewal.HasValue || !string.IsNullOrWhiteSpace(request.RenewalProcedure))
        {
            license.SetRenewalInformation(
                request.RenewalRequired ?? license.RenewalRequired,
                request.RenewalPeriodDays ?? license.RenewalPeriodDays,
                request.AutoRenewal ?? license.AutoRenewal,
                request.RenewalProcedure ?? license.RenewalProcedure);
        }

        // Add audit log for license update
        var updatedBy = _currentUserService.Email ?? "System";
        license.LogAuditAction(
            LicenseAuditAction.Updated,
            "License details updated",
            updatedBy);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "License {LicenseNumber} updated by {UpdatedBy} at {UpdatedAt}",
            license.LicenseNumber,
            updatedBy,
            DateTime.UtcNow);

        return _mapper.Map<LicenseDto>(license);
    }
}