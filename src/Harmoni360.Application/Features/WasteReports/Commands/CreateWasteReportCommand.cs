using MediatR;
using Microsoft.AspNetCore.Http;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record CreateWasteReportCommand : IRequest<WasteReportDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public WasteCategory Category { get; init; }
    public DateTime GeneratedDate { get; init; }
    public string Location { get; init; } = string.Empty;
    public int? ReporterId { get; init; }
    public List<IFormFile> Attachments { get; init; } = new();
}
