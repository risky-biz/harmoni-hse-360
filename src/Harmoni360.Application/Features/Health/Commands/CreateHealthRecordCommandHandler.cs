using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class CreateHealthRecordCommandHandler : IRequestHandler<CreateHealthRecordCommand, HealthRecordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateHealthRecordCommandHandler> _logger;
    private readonly ICacheService _cache;

    public CreateHealthRecordCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateHealthRecordCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<HealthRecordDto> Handle(CreateHealthRecordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating health record for person ID: {PersonId} by user {UserEmail}",
            request.PersonId, _currentUserService.Email);

        // Verify the person exists
        var person = await _context.Users.FindAsync(request.PersonId, cancellationToken);
        if (person == null)
        {
            throw new ArgumentException($"Person with ID {request.PersonId} not found", nameof(request.PersonId));
        }

        // Check if health record already exists for this person
        var existingRecord = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.PersonId == request.PersonId, cancellationToken);

        if (existingRecord != null)
        {
            throw new InvalidOperationException($"Health record already exists for person ID {request.PersonId}");
        }

        // Create health record
        var healthRecord = HealthRecord.Create(
            request.PersonId,
            request.PersonType,
            request.DateOfBirth,
            request.BloodType,
            request.MedicalNotes
        );

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Health record created successfully with ID: {HealthRecordId}", healthRecord.Id);

        // Invalidate health-related caches
        await InvalidateHealthCaches();

        // Return DTO
        return new HealthRecordDto
        {
            Id = healthRecord.Id,
            PersonId = healthRecord.PersonId,
            PersonType = healthRecord.PersonType.ToString(),
            DateOfBirth = healthRecord.DateOfBirth,
            BloodType = healthRecord.BloodType?.ToString(),
            MedicalNotes = healthRecord.MedicalNotes,
            IsActive = healthRecord.IsActive,
            PersonName = person.Name,
            PersonEmail = person.Email,
            PersonDepartment = person.Department,
            PersonPosition = person.Position,
            HasCriticalConditions = false, // No conditions yet
            ExpiringVaccinationsCount = 0,
            MedicalConditionsCount = 0,
            VaccinationsCount = 0,
            HealthIncidentsCount = 0,
            EmergencyContactsCount = 0,
            CriticalAllergyAlerts = new List<string>(),
            CreatedAt = healthRecord.CreatedAt,
            CreatedBy = healthRecord.CreatedBy,
            LastModifiedAt = healthRecord.LastModifiedAt,
            LastModifiedBy = healthRecord.LastModifiedBy
        };
    }

    private async Task InvalidateHealthCaches()
    {
        await _cache.RemoveByTagAsync("health");
        await _cache.RemoveByTagAsync("health-records");
        _logger.LogInformation("Cache invalidated for health record creation");
    }
}