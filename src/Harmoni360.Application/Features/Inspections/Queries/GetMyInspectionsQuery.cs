using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.Inspections.Queries;

public record GetMyInspectionsQuery : IRequest<PagedList<InspectionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public InspectionStatus? Status { get; init; }
    public InspectionType? Type { get; init; }
    public InspectionCategory? Category { get; init; }
    public InspectionPriority? Priority { get; init; }
    public int? DepartmentId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public RiskLevel? RiskLevel { get; init; }
    public bool? IsOverdue { get; init; }
    public string SortBy { get; init; } = "ScheduledDate";
    public bool SortDescending { get; init; } = true;
}