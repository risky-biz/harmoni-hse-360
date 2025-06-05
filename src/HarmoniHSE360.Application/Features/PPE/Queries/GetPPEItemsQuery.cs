using HarmoniHSE360.Application.Features.PPE.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.PPE.Queries;

public record GetPPEItemsQuery : IRequest<GetPPEItemsResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public string? Status { get; init; }
    public string? Condition { get; init; }
    public string? Location { get; init; }
    public int? AssignedToId { get; init; }
    public bool? IsExpired { get; init; }
    public bool? IsExpiringSoon { get; init; }
    public bool? IsMaintenanceDue { get; init; }
    public bool? IsInspectionDue { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}

public class GetPPEItemsResponse
{
    public List<PPEItemSummaryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}