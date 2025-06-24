using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record UpdateWasteReportCommand : IRequest<WasteReportDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public WasteClassification Classification { get; init; }
    public string? Location { get; init; }
    public decimal? EstimatedQuantity { get; init; }
    public string? QuantityUnit { get; init; }
    public string? DisposalMethod { get; init; }
    public DateTime? DisposalDate { get; init; }
    public string? DisposedBy { get; init; }
    public decimal? DisposalCost { get; init; }
    public string? ContractorName { get; init; }
    public string? ManifestNumber { get; init; }
    public string? Treatment { get; init; }
    public string? Notes { get; init; }
}