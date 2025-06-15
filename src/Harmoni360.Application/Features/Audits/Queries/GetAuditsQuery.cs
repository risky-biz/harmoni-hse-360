using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Audits.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Audits.Queries;

public record GetAuditsQuery : IRequest<PagedList<AuditSummaryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public AuditStatus? Status { get; init; }
    public AuditType? Type { get; init; }
    public AuditCategory? Category { get; init; }
    public AuditPriority? Priority { get; init; }
    public RiskLevel? RiskLevel { get; init; }
    public int? AuditorId { get; init; }
    public int? DepartmentId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}