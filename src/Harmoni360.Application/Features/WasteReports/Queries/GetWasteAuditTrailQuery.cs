using MediatR;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries
{
    public record GetWasteAuditTrailQuery(int WasteReportId) : IRequest<IEnumerable<WasteAuditLogDto>>;
}