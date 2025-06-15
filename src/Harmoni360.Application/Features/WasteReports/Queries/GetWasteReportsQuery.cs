using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetWasteReportsQuery(
    WasteCategory? Category = null,
    WasteReportStatus? Status = null,
    WasteClassification? Classification = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Location = null,
    int? ReporterId = null,
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedList<WasteReportDto>>;
