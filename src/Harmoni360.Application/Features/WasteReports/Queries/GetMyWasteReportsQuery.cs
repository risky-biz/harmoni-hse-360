using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetMyWasteReportsQuery(
    int UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedList<WasteReportDto>>;