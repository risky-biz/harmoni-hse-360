using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;

namespace Harmoni360.Application.Features.Audits.Queries;

public record GetAuditDashboardQuery : IRequest<AuditDashboardDto>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? DepartmentId { get; init; }
}