using MediatR;
using Microsoft.AspNetCore.Http;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Application.Features.Incidents.DTOs;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public record CreateIncidentCommand : IRequest<IncidentDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IncidentSeverity Severity { get; init; }
    public DateTime IncidentDate { get; init; }
    public string Location { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int ReporterId { get; init; }
    public List<IFormFile> Attachments { get; init; } = new();
    public List<int> InvolvedPersonIds { get; init; } = new();
}