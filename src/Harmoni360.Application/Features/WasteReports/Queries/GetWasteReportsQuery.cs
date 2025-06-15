using MediatR;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetWasteReportsQuery(
    WasteCategory? Category = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<List<WasteReportDto>>;
