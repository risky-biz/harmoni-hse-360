using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public class GetHealthRecordByIdQueryHandler : IRequestHandler<GetHealthRecordByIdQuery, HealthRecordDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetHealthRecordByIdQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetHealthRecordByIdQueryHandler(
        IApplicationDbContext context,
        ILogger<GetHealthRecordByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<HealthRecordDetailDto?> Handle(GetHealthRecordByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting health record by ID: {HealthRecordId}", request.Id);

        // Try cache first
        var cacheKey = $"health-record-detail-{request.Id}";
        var cachedResult = await _cache.GetAsync<HealthRecordDetailDto>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Health record found in cache: {HealthRecordId}", request.Id);
            return cachedResult;
        }

        // Query from database with all related data
        var healthRecord = await _context.HealthRecords
            .Include(hr => hr.Person)
            .Include(hr => hr.MedicalConditions.Where(mc => request.IncludeInactive || mc.IsActive))
            .Include(hr => hr.Vaccinations)
            .Include(hr => hr.HealthIncidents)
            .Include(hr => hr.EmergencyContacts.Where(ec => request.IncludeInactive || ec.IsActive))
            .Where(hr => hr.Id == request.Id && (request.IncludeInactive || hr.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (healthRecord == null)
        {
            _logger.LogWarning("Health record not found: {HealthRecordId}", request.Id);
            return null;
        }

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

        // Build detailed DTO
        var result = new HealthRecordDetailDto
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
            LastModifiedBy = healthRecord.LastModifiedBy,

            // Related entities
            MedicalConditions = healthRecord.MedicalConditions.Select(mc => new MedicalConditionDto
            {
                Id = mc.Id,
                HealthRecordId = mc.HealthRecordId,
                Type = mc.Type.ToString(),
                Name = mc.Name,
                Description = mc.Description,
                Severity = mc.Severity.ToString(),
                TreatmentPlan = mc.TreatmentPlan,
                DiagnosedDate = mc.DiagnosedDate,
                RequiresEmergencyAction = mc.RequiresEmergencyAction,
                EmergencyInstructions = mc.EmergencyInstructions,
                IsActive = mc.IsActive,
                CreatedAt = mc.CreatedAt,
                CreatedBy = mc.CreatedBy,
                LastModifiedAt = mc.LastModifiedAt,
                LastModifiedBy = mc.LastModifiedBy
            }).ToList(),

            Vaccinations = healthRecord.Vaccinations.Select(v => new VaccinationRecordDto
            {
                Id = v.Id,
                HealthRecordId = v.HealthRecordId,
                VaccineName = v.VaccineName,
                DateAdministered = v.DateAdministered,
                ExpiryDate = v.ExpiryDate,
                BatchNumber = v.BatchNumber,
                AdministeredBy = v.AdministeredBy,
                AdministrationLocation = v.AdministrationLocation,
                Status = v.Status.ToString(),
                Notes = v.Notes,
                IsRequired = v.IsRequired,
                ExemptionReason = v.ExemptionReason,
                IsExpired = v.IsExpired(),
                IsExpiringSoon = v.IsExpiringSoon(30),
                DaysUntilExpiry = v.DaysUntilExpiry(),
                IsCompliant = v.IsCompliant(),
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedBy,
                LastModifiedAt = v.LastModifiedAt,
                LastModifiedBy = v.LastModifiedBy
            }).ToList(),

            HealthIncidents = healthRecord.HealthIncidents.Select(hi => new HealthIncidentDto
            {
                Id = hi.Id,
                IncidentId = hi.IncidentId,
                HealthRecordId = hi.HealthRecordId,
                Type = hi.Type.ToString(),
                Severity = hi.Severity.ToString(),
                Symptoms = hi.Symptoms,
                TreatmentProvided = hi.TreatmentProvided,
                TreatmentLocation = hi.TreatmentLocation.ToString(),
                RequiredHospitalization = hi.RequiredHospitalization,
                ParentsNotified = hi.ParentsNotified,
                ParentNotificationTime = hi.ParentNotificationTime,
                ReturnToSchoolDate = hi.ReturnToSchoolDate,
                FollowUpRequired = hi.FollowUpRequired,
                TreatedBy = hi.TreatedBy,
                IncidentDateTime = hi.IncidentDateTime,
                IsResolved = hi.IsResolved,
                ResolutionNotes = hi.ResolutionNotes,
                IsCritical = hi.IsCritical(),
                RequiresParentNotification = hi.RequiresParentNotification(),
                IsOverdue = hi.IsOverdue(),
                CreatedAt = hi.CreatedAt,
                CreatedBy = hi.CreatedBy,
                LastModifiedAt = hi.LastModifiedAt,
                LastModifiedBy = hi.LastModifiedBy
            }).ToList(),

            EmergencyContacts = healthRecord.EmergencyContacts.Select(ec => new EmergencyContactDto
            {
                Id = ec.Id,
                HealthRecordId = ec.HealthRecordId,
                Name = ec.Name,
                Relationship = ec.Relationship.ToString(),
                CustomRelationship = ec.CustomRelationship,
                PrimaryPhone = ec.PrimaryPhone,
                SecondaryPhone = ec.SecondaryPhone,
                Email = ec.Email,
                Address = ec.Address,
                IsPrimaryContact = ec.IsPrimaryContact,
                AuthorizedForPickup = ec.AuthorizedForPickup,
                AuthorizedForMedicalDecisions = ec.AuthorizedForMedicalDecisions,
                ContactOrder = ec.ContactOrder,
                IsActive = ec.IsActive,
                Notes = ec.Notes,
                DisplayRelationship = ec.GetDisplayRelationship(),
                FullContactInfo = ec.GetFullContactInfo(),
                HasValidContactMethod = ec.HasValidContactMethod(),
                CreatedAt = ec.CreatedAt,
                CreatedBy = ec.CreatedBy,
                LastModifiedAt = ec.LastModifiedAt,
                LastModifiedBy = ec.LastModifiedBy
            }).ToList()
        };

        // Cache the result for 30 minutes
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), new[] { "health", "health-records" });

        _logger.LogInformation("Health record retrieved successfully: {HealthRecordId}", request.Id);
        return result;
    }
}