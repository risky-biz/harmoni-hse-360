using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class RecordVaccinationCommandHandler : IRequestHandler<RecordVaccinationCommand, VaccinationRecordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RecordVaccinationCommandHandler> _logger;
    private readonly ICacheService _cache;

    public RecordVaccinationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<RecordVaccinationCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<VaccinationRecordDto> Handle(RecordVaccinationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording vaccination {VaccineName} for health record ID: {HealthRecordId} by user {UserEmail}",
            request.VaccineName, request.HealthRecordId, _currentUserService.Email);

        // Verify the health record exists and is active
        var healthRecord = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.Id == request.HealthRecordId && hr.IsActive, cancellationToken);

        if (healthRecord == null)
        {
            throw new ArgumentException($"Active health record with ID {request.HealthRecordId} not found", nameof(request.HealthRecordId));
        }

        // Check for existing vaccination record for the same vaccine
        var existingVaccination = await _context.VaccinationRecords
            .FirstOrDefaultAsync(vr => vr.HealthRecordId == request.HealthRecordId 
                && vr.VaccineName.ToLower() == request.VaccineName.ToLower()
                && vr.Status != VaccinationStatus.Expired, cancellationToken);

        if (existingVaccination != null && existingVaccination.Status == VaccinationStatus.Administered)
        {
            _logger.LogWarning("Vaccination {VaccineName} already administered for health record {HealthRecordId}. Creating new record for booster/renewal.",
                request.VaccineName, request.HealthRecordId);
        }

        // Create vaccination record
        var vaccination = VaccinationRecord.Create(
            request.HealthRecordId,
            request.VaccineName,
            request.IsRequired,
            request.DateAdministered,
            request.ExpiryDate,
            request.BatchNumber,
            request.AdministeredBy,
            request.AdministrationLocation,
            request.Notes
        );

        // Record the administration
        vaccination.RecordAdministration(
            request.DateAdministered,
            request.AdministeredBy,
            request.BatchNumber,
            request.AdministrationLocation,
            request.ExpiryDate,
            request.Notes
        );

        _context.VaccinationRecords.Add(vaccination);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vaccination recorded successfully with ID: {VaccinationId}. Vaccine: {VaccineName}, Expires: {ExpiryDate}",
            vaccination.Id, vaccination.VaccineName, vaccination.ExpiryDate?.ToString("yyyy-MM-dd") ?? "No expiry");

        // Log compliance status
        if (vaccination.IsRequired)
        {
            _logger.LogInformation("Required vaccination {VaccineName} recorded for health record {HealthRecordId}. Compliance status updated.",
                vaccination.VaccineName, request.HealthRecordId);
        }

        // Invalidate health-related caches
        await InvalidateHealthCaches(request.HealthRecordId);

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
        _logger.LogInformation("Cache invalidated for vaccination recording in health record: {HealthRecordId}", healthRecordId);
    }
}