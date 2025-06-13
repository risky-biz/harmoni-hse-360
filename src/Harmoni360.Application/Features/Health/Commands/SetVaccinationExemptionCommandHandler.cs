using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public class SetVaccinationExemptionCommandHandler : IRequestHandler<SetVaccinationExemptionCommand, VaccinationRecordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetVaccinationExemptionCommandHandler> _logger;
    private readonly ICacheService _cache;

    public SetVaccinationExemptionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<SetVaccinationExemptionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<VaccinationRecordDto> Handle(SetVaccinationExemptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting vaccination exemption for vaccination ID: {VaccinationId} by user {UserEmail}. Remove exemption: {RemoveExemption}",
            request.Id, _currentUserService.Email, request.RemoveExemption);

        var vaccination = await _context.VaccinationRecords
            .FirstOrDefaultAsync(vr => vr.Id == request.Id, cancellationToken);

        if (vaccination == null)
        {
            throw new ArgumentException($"Vaccination record with ID {request.Id} not found", nameof(request.Id));
        }

        var previousStatus = vaccination.Status;
        var previousExemptionReason = vaccination.ExemptionReason;

        if (request.RemoveExemption)
        {
            vaccination.RemoveExemption();
            _logger.LogInformation("Vaccination exemption removed for vaccination ID: {VaccinationId}. Previous reason: {PreviousReason}",
                request.Id, previousExemptionReason ?? "None");
        }
        else
        {
            vaccination.SetExemption(request.ExemptionReason);
            _logger.LogWarning("Vaccination exemption set for vaccination ID: {VaccinationId}. Vaccine: {VaccineName}, Reason: {Reason}",
                request.Id, vaccination.VaccineName, request.ExemptionReason);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Log compliance impact
        if (vaccination.IsRequired)
        {
            var complianceStatus = vaccination.IsCompliant() ? "compliant" : "non-compliant";
            _logger.LogInformation("Required vaccination {VaccineName} exemption status changed. New compliance status: {ComplianceStatus}",
                vaccination.VaccineName, complianceStatus);
        }

        _logger.LogInformation("Vaccination exemption status updated successfully for vaccination ID: {VaccinationId}", vaccination.Id);

        // Invalidate health-related caches
        await InvalidateHealthCaches(vaccination.HealthRecordId);

        // Calculate derived properties
        var isExpired = vaccination.IsExpired();
        var isExpiringSoon = vaccination.IsExpiringSoon(30);
        var daysUntilExpiry = vaccination.DaysUntilExpiry();
        var isCompliant = vaccination.IsCompliant();

        // Return DTO
        return new VaccinationRecordDto
        {
            Id = vaccination.Id,
            HealthRecordId = vaccination.HealthRecordId,
            VaccineName = vaccination.VaccineName,
            DateAdministered = vaccination.DateAdministered,
            ExpiryDate = vaccination.ExpiryDate,
            BatchNumber = vaccination.BatchNumber,
            AdministeredBy = vaccination.AdministeredBy,
            AdministrationLocation = vaccination.AdministrationLocation,
            Status = vaccination.Status.ToString(),
            Notes = vaccination.Notes,
            IsRequired = vaccination.IsRequired,
            ExemptionReason = vaccination.ExemptionReason,
            IsExpired = isExpired,
            IsExpiringSoon = isExpiringSoon,
            DaysUntilExpiry = daysUntilExpiry,
            IsCompliant = isCompliant,
            CreatedAt = vaccination.CreatedAt,
            CreatedBy = vaccination.CreatedBy,
            LastModifiedAt = vaccination.LastModifiedAt,
            LastModifiedBy = vaccination.LastModifiedBy
        };
    }

    private async Task InvalidateHealthCaches(int healthRecordId)
    {
        await _cache.RemoveByTagAsync("health");
        await _cache.RemoveByTagAsync("health-records");
        await _cache.RemoveByTagAsync("vaccination-records");
        await _cache.RemoveByTagAsync("vaccination-compliance");
        await _cache.RemoveAsync($"health-record-{healthRecordId}");
        _logger.LogInformation("Cache invalidated for vaccination exemption change in health record: {HealthRecordId}", healthRecordId);
    }
}