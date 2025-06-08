using MediatR;
using Microsoft.AspNetCore.Http;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Hazards.DTOs;

namespace Harmoni360.Application.Features.Hazards.Commands;

public record UpdateHazardCommand : IRequest<HazardDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public HazardCategory Category { get; init; }
    public HazardType Type { get; init; }
    public string Location { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public HazardStatus Status { get; init; }
    public HazardSeverity Severity { get; init; }
    public DateTime? ExpectedResolutionDate { get; init; }
    public string? StatusChangeReason { get; init; }
    public List<IFormFile> NewAttachments { get; init; } = new();
    public List<int> AttachmentsToRemove { get; init; } = new();
}