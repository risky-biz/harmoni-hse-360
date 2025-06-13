using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public class UpdateVaccinationCommandHandler : IRequestHandler<UpdateVaccinationCommand, VaccinationRecordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateVaccinationCommandHandler> _logger;
    private readonly ICacheService _cache;

    public UpdateVaccinationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateVaccinationCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<VaccinationRecordDto> Handle(UpdateVaccinationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating vaccination record ID: {VaccinationId} by user {UserEmail}",
            request.Id, _currentUserService.Email);

        var vaccination = await _context.VaccinationRecords
            .FirstOrDefaultAsync(vr => vr.Id == request.Id, cancellationToken);

        if (vaccination == null)
        {
            throw new ArgumentException($"Vaccination record with ID {request.Id} not found", nameof(request.Id));
        }

        // Log important changes
        var previousStatus = vaccination.Status;
        var previousExpiryDate = vaccination.ExpiryDate;

        // Update vaccination record if administered
        if (request.DateAdministered.HasValue && !string.IsNullOrEmpty(request.AdministeredBy))
        {
            vaccination.RecordAdministration(
                request.DateAdministered.Value,
                request.AdministeredBy,
                request.BatchNumber,
                request.AdministrationLocation,
                request.ExpiryDate,
                request.Notes
            );

            _logger.LogInformation("Vaccination administration recorded for vaccination ID: {VaccinationId}", request.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Log status changes
        if (previousStatus != vaccination.Status)
        {
            _logger.LogInformation("Vaccination status changed from {PreviousStatus} to {NewStatus} for vaccination {VaccinationId}",
                previousStatus, vaccination.Status, request.Id);
        }

        if (previousExpiryDate != vaccination.ExpiryDate)
        {
            _logger.LogInformation("Vaccination expiry date changed from {PreviousDate} to {NewDate} for vaccination {VaccinationId}",
                previousExpiryDate?.ToString("yyyy-MM-dd") ?? "None", 
                vaccination.ExpiryDate?.ToString("yyyy-MM-dd") ?? "None", 
                request.Id);
        }

        _logger.LogInformation("Vaccination record updated successfully: {VaccinationId}", vaccination.Id);

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
        _logger.LogInformation("Cache invalidated for vaccination update in health record: {HealthRecordId}", healthRecordId);
    }
}