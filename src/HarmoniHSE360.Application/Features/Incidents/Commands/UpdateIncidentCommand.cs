using MediatR;
using HarmoniHSE360.Application.Features.Incidents.DTOs;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class UpdateIncidentCommand : IRequest<IncidentDto?>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}