using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Statistics.Queries;

public class GetHsseTrendQueryHandler : IRequestHandler<GetHsseTrendQuery, List<HsseTrendPointDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHsseTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HsseTrendPointDto>> Handle(GetHsseTrendQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;
        if (startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        // Build month list
        var months = Enumerable.Range(0, ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1)
            .Select(i => new DateTime(startDate.Year, startDate.Month, 1).AddMonths(i))
            .ToList();
        var results = months.Select(m => new HsseTrendPointDto
        {
            Period = $"{m.Year}-{m.Month:D2}",
            PeriodLabel = m.ToString("MMM yyyy")
        }).ToList();

        if (!request.Module.HasValue || request.Module.Value == ModuleType.IncidentManagement)
        {
            var incidentData = await _context.Incidents
                .Where(i => i.IncidentDate >= startDate && i.IncidentDate <= endDate)
                .GroupBy(i => new { i.IncidentDate.Year, i.IncidentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(cancellationToken);
            foreach (var d in incidentData)
            {
                var key = $"{d.Year}-{d.Month:D2}";
                var item = results.FirstOrDefault(r => r.Period == key);
                if (item != null) item.IncidentCount = d.Count;
            }
        }

        if (!request.Module.HasValue || request.Module.Value == ModuleType.RiskManagement)
        {
            var hazardData = await _context.Hazards
                .Where(h => h.IdentifiedDate >= startDate && h.IdentifiedDate <= endDate)
                .GroupBy(h => new { h.IdentifiedDate.Year, h.IdentifiedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(cancellationToken);
            foreach (var d in hazardData)
            {
                var key = $"{d.Year}-{d.Month:D2}";
                var item = results.FirstOrDefault(r => r.Period == key);
                if (item != null) item.HazardCount = d.Count;
            }
        }

        if (!request.Module.HasValue || request.Module.Value == ModuleType.SecurityIncidentManagement)
        {
            var secData = await _context.SecurityIncidents
                .Where(s => s.IncidentDateTime >= startDate && s.IncidentDateTime <= endDate)
                .GroupBy(s => new { s.IncidentDateTime.Year, s.IncidentDateTime.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(cancellationToken);
            foreach (var d in secData)
            {
                var key = $"{d.Year}-{d.Month:D2}";
                var item = results.FirstOrDefault(r => r.Period == key);
                if (item != null) item.SecurityIncidentCount = d.Count;
            }
        }

        if (!request.Module.HasValue || request.Module.Value == ModuleType.HealthMonitoring)
        {
            var healthData = await _context.HealthIncidents
                .Where(h => h.IncidentDateTime >= startDate && h.IncidentDateTime <= endDate)
                .GroupBy(h => new { h.IncidentDateTime.Year, h.IncidentDateTime.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(cancellationToken);
            foreach (var d in healthData)
            {
                var key = $"{d.Year}-{d.Month:D2}";
                var item = results.FirstOrDefault(r => r.Period == key);
                if (item != null) item.HealthIncidentCount = d.Count;
            }
        }

        return results;
    }
}
