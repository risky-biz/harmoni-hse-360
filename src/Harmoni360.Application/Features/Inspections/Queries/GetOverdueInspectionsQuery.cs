using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Inspections.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Inspections.Queries;

public record GetOverdueInspectionsQuery : IRequest<PagedList<InspectionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? DepartmentId { get; init; }
    public int? InspectorId { get; init; }
    public string SortBy { get; init; } = "ScheduledDate";
    public bool SortDescending { get; init; } = false;
}