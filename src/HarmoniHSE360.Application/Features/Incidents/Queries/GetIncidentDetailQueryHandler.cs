using MediatR;
using Microsoft.EntityFrameworkCore;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Incidents.DTOs;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetIncidentDetailQueryHandler : IRequestHandler<GetIncidentDetailQuery, IncidentDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IIncidentAuditService _auditService;

    public GetIncidentDetailQueryHandler(IApplicationDbContext context, IIncidentAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IncidentDetailDto?> Handle(GetIncidentDetailQuery request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .Include(i => i.Attachments)
            .Include(i => i.InvolvedPersons)
                .ThenInclude(ip => ip.Person)
            .Include(i => i.CorrectiveActions)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
            return null;

        // Removed view logging to reduce noise in audit trail
        // Only significant actions should be logged

        return new IncidentDetailDto
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
            InvolvedPersons = incident.InvolvedPersons.Select(ip =>
            {
                var nameParts = ip.Person.Name.Split(' ');
                return new InvolvedPersonDto
                {
                    Id = ip.Id,
                    Person = new UserDto
                    {
                        Id = ip.Person.Id,
                        FirstName = nameParts.FirstOrDefault() ?? "",
                        LastName = string.Join(" ", nameParts.Skip(1)),
                        Email = ip.Person.Email,
                        FullName = ip.Person.Name
                    },
                    InvolvementType = ip.InvolvementType.ToString(),
                    InjuryDescription = ip.InjuryDescription
                };
            }).ToList(),
            AttachmentsCount = incident.Attachments.Count,
            CorrectiveActionsCount = incident.CorrectiveActions.Count,
            CreatedAt = incident.CreatedAt,
            CreatedBy = incident.CreatedBy,
            LastModifiedAt = incident.LastModifiedAt,
            LastModifiedBy = incident.LastModifiedBy
        };
    }
}