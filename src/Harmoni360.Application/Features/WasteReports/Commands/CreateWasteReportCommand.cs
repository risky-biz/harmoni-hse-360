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
    
    // Additional fields for comprehensive waste report creation
    public decimal? EstimatedQuantity { get; init; }
    public string? QuantityUnit { get; init; }
    public string? DisposalMethod { get; init; }
    public DateTime? DisposalDate { get; init; }
    public decimal? DisposalCost { get; init; }
    public string? ContractorName { get; init; }
    public string? ManifestNumber { get; init; }
    public string? Treatment { get; init; }
    public string? Notes { get; init; }
}
