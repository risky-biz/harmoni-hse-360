using MediatR;
using Microsoft.EntityFrameworkCore;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Incidents.DTOs;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetIncidentsQueryHandler : IRequestHandler<GetIncidentsQuery, GetIncidentsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;
    private const string INCIDENTS_CACHE_KEY_PREFIX = "incidents";
    private const string INCIDENTS_CACHE_TAG = "incidents_list";

    public GetIncidentsQueryHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<GetIncidentsResponse> Handle(GetIncidentsQuery request, CancellationToken cancellationToken)
    {
        // Generate cache key based on query parameters
        var cacheKey = _cache.GenerateKey(INCIDENTS_CACHE_KEY_PREFIX,
            request.PageNumber, request.PageSize, request.Status ?? "", request.Severity ?? "", request.SearchTerm ?? "");

        // Try to get from cache first
        var cachedResponse = await _cache.GetAsync<GetIncidentsResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var query = _context.Incidents
            .Include(i => i.Attachments)
            .Include(i => i.InvolvedPersons)
            .Include(i => i.CorrectiveActions)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(i =>
                i.Title.ToLower().Contains(searchTermLower) ||
                i.Description.ToLower().Contains(searchTermLower) ||
                i.Location.ToLower().Contains(searchTermLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<IncidentStatus>(request.Status, out var status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Severity) && Enum.TryParse<IncidentSeverity>(request.Severity, out var severity))
        {
            query = query.Where(i => i.Severity == severity);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var incidents = await query
            .OrderByDescending(i => i.IncidentDate)
            .ThenByDescending(i => i.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var incidentDtos = incidents.Select(i => new IncidentDto
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            Severity = i.Severity.ToString(),
            Status = i.Status.ToString(),
            IncidentDate = i.IncidentDate,
            Location = i.Location,
            ReporterName = i.ReporterName,
            ReporterEmail = i.ReporterEmail,
            ReporterDepartment = i.ReporterDepartment,
            InjuryType = i.InjuryType?.ToString(),
            MedicalTreatmentProvided = i.MedicalTreatmentProvided,
            EmergencyServicesContacted = i.EmergencyServicesContacted,
            WitnessNames = i.WitnessNames,
            ImmediateActionsTaken = i.ImmediateActionsTaken,
            AttachmentsCount = i.Attachments.Count,
            InvolvedPersonsCount = i.InvolvedPersons.Count,
            CorrectiveActionsCount = i.CorrectiveActions.Count,
            CreatedAt = i.CreatedAt,
            CreatedBy = i.CreatedBy,
            LastModifiedAt = i.LastModifiedAt,
            LastModifiedBy = i.LastModifiedBy
        }).ToList();

        var response = new GetIncidentsResponse
        {
            Incidents = incidentDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        // Cache the response with tag for easy invalidation
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), INCIDENTS_CACHE_TAG);

        return response;
    }
}