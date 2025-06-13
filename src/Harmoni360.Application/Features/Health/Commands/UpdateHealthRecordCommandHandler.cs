using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public class UpdateHealthRecordCommandHandler : IRequestHandler<UpdateHealthRecordCommand, HealthRecordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateHealthRecordCommandHandler> _logger;
    private readonly ICacheService _cache;

    public UpdateHealthRecordCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateHealthRecordCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<HealthRecordDto> Handle(UpdateHealthRecordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating health record ID: {HealthRecordId} by user {UserEmail}",
            request.Id, _currentUserService.Email);

        // Get the health record with person information
        var healthRecord = await _context.HealthRecords
            .Include(hr => hr.Person)
            .Include(hr => hr.MedicalConditions.Where(mc => mc.IsActive))
            .Include(hr => hr.Vaccinations)
            .Include(hr => hr.EmergencyContacts.Where(ec => ec.IsActive))
            .FirstOrDefaultAsync(hr => hr.Id == request.Id, cancellationToken);

        if (healthRecord == null)
        {
            throw new ArgumentException($"Health record with ID {request.Id} not found", nameof(request.Id));
        }

        if (!healthRecord.IsActive)
        {
            throw new InvalidOperationException("Cannot update an inactive health record");
        }

        // Update basic health information
        healthRecord.UpdateBasicInfo(
            request.DateOfBirth,
            request.BloodType,
            request.MedicalNotes
        );

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Health record updated successfully with ID: {HealthRecordId}", healthRecord.Id);

        // Invalidate health-related caches
        await InvalidateHealthCaches(healthRecord.Id);

        // Calculate derived properties
        var criticalConditions = healthRecord.MedicalConditions
            .Where(mc => mc.IsActive && mc.RequiresEmergencyAction)
            .ToList();

        var expiringVaccinations = healthRecord.Vaccinations
            .Where(v => v.IsExpiringSoon(30))
            .ToList();

        var criticalAllergyAlerts = healthRecord.MedicalConditions
            .Where(mc => mc.IsActive && mc.Type == Domain.Entities.ConditionType.Allergy && mc.RequiresEmergencyAction)
            .Select(mc => mc.Name)
            .ToList();

        // Return updated DTO
        return new HealthRecordDto
        {
            Id = healthRecord.Id,
            PersonId = healthRecord.PersonId,
            PersonType = healthRecord.PersonType.ToString(),
            DateOfBirth = healthRecord.DateOfBirth,
            BloodType = healthRecord.BloodType?.ToString(),
            MedicalNotes = healthRecord.MedicalNotes,
            IsActive = healthRecord.IsActive,
            PersonName = healthRecord.Person.Name,
            PersonEmail = healthRecord.Person.Email,
            PersonDepartment = healthRecord.Person.Department,
            PersonPosition = healthRecord.Person.Position,
            HasCriticalConditions = criticalConditions.Any(),
            ExpiringVaccinationsCount = expiringVaccinations.Count,
            MedicalConditionsCount = healthRecord.MedicalConditions.Count(mc => mc.IsActive),
            VaccinationsCount = healthRecord.Vaccinations.Count,
            HealthIncidentsCount = healthRecord.HealthIncidents.Count,
            EmergencyContactsCount = healthRecord.EmergencyContacts.Count(ec => ec.IsActive),
            CriticalAllergyAlerts = criticalAllergyAlerts,
            CreatedAt = healthRecord.CreatedAt,
            CreatedBy = healthRecord.CreatedBy,
            LastModifiedAt = healthRecord.LastModifiedAt,
            LastModifiedBy = healthRecord.LastModifiedBy
        };
    }

    private async Task InvalidateHealthCaches(int healthRecordId)
    {
        await _cache.RemoveByTagAsync("health");
        await _cache.RemoveByTagAsync("health-records");
        await _cache.RemoveAsync($"health-record-{healthRecordId}");
        _logger.LogInformation("Cache invalidated for health record update: {HealthRecordId}", healthRecordId);
    }
}