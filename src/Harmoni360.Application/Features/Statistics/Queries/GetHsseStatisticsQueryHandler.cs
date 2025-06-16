using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Utils;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Statistics.Queries;

public class GetHsseStatisticsQueryHandler : IRequestHandler<GetHsseStatisticsQuery, HsseStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetHsseStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HsseStatisticsDto> Handle(GetHsseStatisticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var incidentsQuery = _context.Incidents.Where(i => i.IncidentDate >= startDate && i.IncidentDate <= endDate);
        var hazardsQuery = _context.Hazards.Where(h => h.IdentifiedDate >= startDate && h.IdentifiedDate <= endDate);
        var securityIncidentsQuery = _context.SecurityIncidents.Where(s => s.IncidentDateTime >= startDate && s.IncidentDateTime <= endDate);
        var healthIncidentsQuery = _context.HealthIncidents.Where(h => h.IncidentDateTime >= startDate && h.IncidentDateTime <= endDate);

        var dto = new HsseStatisticsDto();

        if (request.Module.HasValue)
        {
            switch (request.Module.Value)
            {
                case ModuleType.IncidentManagement:
                    dto.TotalIncidents = await incidentsQuery.CountAsync(cancellationToken);
                    break;
                case ModuleType.RiskManagement:
                    dto.TotalHazards = await hazardsQuery.CountAsync(cancellationToken);
                    break;
                case ModuleType.SecurityIncidentManagement:
                    dto.TotalSecurityIncidents = await securityIncidentsQuery.CountAsync(cancellationToken);
                    break;
                case ModuleType.HealthMonitoring:
                    dto.TotalHealthIncidents = await healthIncidentsQuery.CountAsync(cancellationToken);
                    break;
            }
        }
        else
        {
            dto.TotalIncidents = await incidentsQuery.CountAsync(cancellationToken);
            dto.TotalHazards = await hazardsQuery.CountAsync(cancellationToken);
            dto.TotalSecurityIncidents = await securityIncidentsQuery.CountAsync(cancellationToken);
            dto.TotalHealthIncidents = await healthIncidentsQuery.CountAsync(cancellationToken);
        }

        dto.Trir = HsseKpiCalculator.CalculateTrir(dto.TotalIncidents, request.HoursWorked);
        dto.Ltifr = HsseKpiCalculator.CalculateLtifr(request.LostTimeInjuries, request.HoursWorked);
        dto.SeverityRate = HsseKpiCalculator.CalculateSeverityRate(request.DaysLost, request.HoursWorked);
        dto.ComplianceRate = HsseKpiCalculator.CalculateComplianceRate(request.CompliantRecords, request.TotalRecords);

        return dto;
    }
}
