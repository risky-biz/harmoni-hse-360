using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Queries;

public class GetHealthRecordsQueryHandler : IRequestHandler<GetHealthRecordsQuery, GetHealthRecordsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetHealthRecordsQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetHealthRecordsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetHealthRecordsQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<GetHealthRecordsResponse> Handle(GetHealthRecordsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting health records with filters: PersonType={PersonType}, Department={Department}, Page={PageNumber}, Size={PageSize}",
            request.PersonType, request.Department, request.PageNumber, request.PageSize);

        // Build base query
        var query = _context.HealthRecords
            .Include(hr => hr.Person)
            .Include(hr => hr.MedicalConditions.Where(mc => mc.IsActive))
            .Include(hr => hr.Vaccinations)
            .Include(hr => hr.EmergencyContacts.Where(ec => ec.IsActive))
            .AsQueryable();

        // Apply filters
        if (!request.IncludeInactive)
        {
            query = query.Where(hr => hr.IsActive);
        }

        if (request.PersonType.HasValue)
        {
            query = query.Where(hr => hr.PersonType == request.PersonType.Value);
        }

        if (!string.IsNullOrEmpty(request.Department))
        {
            query = query.Where(hr => hr.Person.Department != null && 
                                    hr.Person.Department.ToLower().Contains(request.Department.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(hr => 
                hr.Person.Name.ToLower().Contains(searchTerm) ||
                hr.Person.Email.ToLower().Contains(searchTerm) ||
                (hr.Person.Department != null && hr.Person.Department.ToLower().Contains(searchTerm)) ||
                (hr.MedicalNotes != null && hr.MedicalNotes.ToLower().Contains(searchTerm)));
        }

        if (request.HasCriticalConditions.HasValue)
        {
            if (request.HasCriticalConditions.Value)
            {
                query = query.Where(hr => hr.MedicalConditions.Any(mc => mc.IsActive && mc.RequiresEmergencyAction));
            }
            else
            {
                query = query.Where(hr => !hr.MedicalConditions.Any(mc => mc.IsActive && mc.RequiresEmergencyAction));
            }
        }

        if (request.HasExpiringVaccinations.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(30);
            if (request.HasExpiringVaccinations.Value)
            {
                query = query.Where(hr => hr.Vaccinations.Any(v => v.ExpiryDate.HasValue && v.ExpiryDate <= cutoffDate));
            }
            else
            {
                query = query.Where(hr => !hr.Vaccinations.Any(v => v.ExpiryDate.HasValue && v.ExpiryDate <= cutoffDate));
            }
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending 
                ? query.OrderByDescending(hr => hr.Person.Name)
                : query.OrderBy(hr => hr.Person.Name),
            "department" => request.SortDescending
                ? query.OrderByDescending(hr => hr.Person.Department)
                : query.OrderBy(hr => hr.Person.Department),
            "persontype" => request.SortDescending
                ? query.OrderByDescending(hr => hr.PersonType)
                : query.OrderBy(hr => hr.PersonType),
            "lastmodified" => request.SortDescending
                ? query.OrderByDescending(hr => hr.LastModifiedAt ?? hr.CreatedAt)
                : query.OrderBy(hr => hr.LastModifiedAt ?? hr.CreatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(hr => hr.CreatedAt)
                : query.OrderBy(hr => hr.CreatedAt)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate summary statistics
        var allRecords = await _context.HealthRecords
            .Include(hr => hr.MedicalConditions.Where(mc => mc.IsActive))
            .Include(hr => hr.Vaccinations)
            .Where(hr => !request.IncludeInactive ? hr.IsActive : true)
            .ToListAsync(cancellationToken);

        var activeRecords = allRecords.Count(hr => hr.IsActive);
        var studentRecords = allRecords.Count(hr => hr.PersonType == PersonType.Student);
        var staffRecords = allRecords.Count(hr => hr.PersonType == PersonType.Staff);
        var criticalConditionsCount = allRecords.Count(hr => hr.MedicalConditions.Any(mc => mc.IsActive && mc.RequiresEmergencyAction));
        
        var cutoffDateForExpiry = DateTime.UtcNow.AddDays(30);
        var expiringVaccinationsCount = allRecords.Count(hr => 
            hr.Vaccinations.Any(v => v.ExpiryDate.HasValue && v.ExpiryDate <= cutoffDateForExpiry));

        // Apply pagination
        var records = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var recordDtos = records.Select(hr =>
        {
            var criticalConditions = hr.MedicalConditions
                .Where(mc => mc.IsActive && mc.RequiresEmergencyAction)
                .ToList();

            var expiringVaccinations = hr.Vaccinations
                .Where(v => v.IsExpiringSoon(30))
                .ToList();

            var criticalAllergyAlerts = hr.MedicalConditions
                .Where(mc => mc.IsActive && mc.Type == ConditionType.Allergy && mc.RequiresEmergencyAction)
                .Select(mc => mc.Name)
                .ToList();

            return new HealthRecordDto
            {
                Id = hr.Id,
                PersonId = hr.PersonId,
                PersonType = hr.PersonType.ToString(),
                DateOfBirth = hr.DateOfBirth,
                BloodType = hr.BloodType?.ToString(),
                MedicalNotes = hr.MedicalNotes,
                IsActive = hr.IsActive,
                PersonName = hr.Person.Name,
                PersonEmail = hr.Person.Email,
                PersonDepartment = hr.Person.Department,
                PersonPosition = hr.Person.Position,
                HasCriticalConditions = criticalConditions.Any(),
                ExpiringVaccinationsCount = expiringVaccinations.Count,
                MedicalConditionsCount = hr.MedicalConditions.Count(mc => mc.IsActive),
                VaccinationsCount = hr.Vaccinations.Count,
                HealthIncidentsCount = hr.HealthIncidents.Count,
                EmergencyContactsCount = hr.EmergencyContacts.Count(ec => ec.IsActive),
                CriticalAllergyAlerts = criticalAllergyAlerts,
                CreatedAt = hr.CreatedAt,
                CreatedBy = hr.CreatedBy,
                LastModifiedAt = hr.LastModifiedAt,
                LastModifiedBy = hr.LastModifiedBy
            };
        }).ToList();

        var response = new GetHealthRecordsResponse
        {
            Records = recordDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            ActiveRecords = activeRecords,
            StudentRecords = studentRecords,
            StaffRecords = staffRecords,
            CriticalConditionsCount = criticalConditionsCount,
            ExpiringVaccinationsCount = expiringVaccinationsCount
        };

        _logger.LogInformation("Retrieved {Count} health records out of {Total} total records", 
            recordDtos.Count, totalCount);

        return response;
    }
}