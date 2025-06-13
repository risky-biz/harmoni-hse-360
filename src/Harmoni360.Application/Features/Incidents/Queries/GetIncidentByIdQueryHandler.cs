using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Incidents.DTOs;

namespace Harmoni360.Application.Features.Incidents.Queries;

public class GetIncidentByIdQueryHandler : IRequestHandler<GetIncidentByIdQuery, IncidentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IncidentDto?> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .Include(i => i.Attachments)
            .Include(i => i.InvolvedPersons)
            .Include(i => i.CorrectiveActions)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
            return null;

        return new IncidentDto
        {
            Id = incident.Id,
            Title = incident.Title,
            Description = incident.Description,
            Severity = incident.Severity.ToString(),
            Status = incident.Status.ToString(),
            IncidentDate = incident.IncidentDate,
            Location = incident.Location,
            Latitude = incident.GeoLocation?.Latitude,
            Longitude = incident.GeoLocation?.Longitude,
            ReporterName = incident.ReporterName,
            ReporterEmail = incident.ReporterEmail,
            ReporterDepartment = incident.ReporterDepartment,
            InjuryType = incident.InjuryType?.ToString(),
            MedicalTreatmentProvided = incident.MedicalTreatmentProvided,
            EmergencyServicesContacted = incident.EmergencyServicesContacted,
            WitnessNames = incident.WitnessNames,
            ImmediateActionsTaken = incident.ImmediateActionsTaken,
            AttachmentsCount = incident.Attachments.Count,
            InvolvedPersonsCount = incident.InvolvedPersons.Count,
            CorrectiveActionsCount = incident.CorrectiveActions.Count,
            CreatedAt = incident.CreatedAt,
            CreatedBy = incident.CreatedBy,
            LastModifiedAt = incident.LastModifiedAt,
            LastModifiedBy = incident.LastModifiedBy
        };
    }
}