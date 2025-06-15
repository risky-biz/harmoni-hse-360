using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Entities.Waste;
using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record UpdateWasteReportCommand : IRequest<WasteReportDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public WasteCategory Category { get; init; }
    public DateTime GeneratedDate { get; init; }
    public string Location { get; init; } = string.Empty;
}