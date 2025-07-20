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
        _logger.LogInformation("Creating health record by user {UserEmail}", _currentUserService.Email);

        Person? person;

        if (request.PersonId.HasValue)
        {
            // Use existing person
            _logger.LogInformation("Using existing person ID: {PersonId}", request.PersonId.Value);
            
            person = await _context.Persons.FindAsync(request.PersonId.Value, cancellationToken);
            if (person == null)
            {
                throw new ArgumentException($"Person with ID {request.PersonId.Value} not found", nameof(request.PersonId));
            }
        }
        else
        {
            // Create new person
            _logger.LogInformation("Creating new person: {PersonName} ({PersonEmail})", request.PersonName, request.PersonEmail);
            
            // Check if person with this email already exists
            var existingPerson = await _context.Persons
                .FirstOrDefaultAsync(p => p.Email == request.PersonEmail!.ToLowerInvariant(), cancellationToken);
            
            if (existingPerson != null)
            {
                throw new InvalidOperationException($"Person with email '{request.PersonEmail}' already exists");
            }

            person = Person.Create(
                name: request.PersonName!,
                email: request.PersonEmail!,
                personType: request.PersonType,
                phoneNumber: request.PersonPhoneNumber,
                department: request.PersonDepartment,
                position: request.PersonPosition,
                employeeId: request.PersonEmployeeId
            );

            _context.Persons.Add(person);
            await _context.SaveChangesAsync(cancellationToken); // Save to get the PersonId
        }

        // Check if health record already exists for this person
        var existingRecord = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.PersonId == person.Id, cancellationToken);

        if (existingRecord != null)
        {
            throw new InvalidOperationException($"Health record already exists for person ID {person.Id}");
        }

        // Create health record
        var healthRecord = HealthRecord.Create(
            person.Id,
            request.PersonType,
            request.DateOfBirth,
            request.BloodType,
            request.MedicalNotes
        );

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Health record created successfully with ID: {HealthRecordId} for person ID: {PersonId}", 
            healthRecord.Id, person.Id);

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