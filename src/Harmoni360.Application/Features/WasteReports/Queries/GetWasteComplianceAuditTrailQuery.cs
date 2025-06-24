using MediatR;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries
{
    public record GetWasteComplianceAuditTrailQuery(DateTime FromDate, DateTime ToDate) : IRequest<IEnumerable<WasteAuditLogDto>>;
}