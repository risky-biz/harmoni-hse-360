using MediatR;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WorkPermits.Queries;

public class GetWorkPermitsQuery : IRequest<GetWorkPermitsResponse>
{
    public string? SearchTerm { get; set; }
    public WorkPermitType? Type { get; set; }
    public WorkPermitStatus? Status { get; set; }
    public WorkPermitPriority? Priority { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public int? RequestedById { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public string? Department { get; set; }
    public string? WorkLocation { get; set; }
    public bool? IsOverdue { get; set; }
    public bool? RequiresApproval { get; set; }
    public string? ApprovalLevel { get; set; }
    
    // Indonesian Compliance Filters
    public bool? IsK3Compliant { get; set; }
    public bool? IsJamsostekCompliant { get; set; }
    public bool? HasSMK3Compliance { get; set; }
    
    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "desc"; // asc or desc
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetWorkPermitsResponse
{
    public List<WorkPermitDto> WorkPermits { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    
    // Summary Statistics
    public WorkPermitSummaryDto Summary { get; set; } = new();
}

public class WorkPermitSummaryDto
{
    public int TotalPermits { get; set; }
    public int DraftPermits { get; set; }
    public int PendingApprovalPermits { get; set; }
    public int ApprovedPermits { get; set; }
    public int InProgressPermits { get; set; }
    public int CompletedPermits { get; set; }
    public int RejectedPermits { get; set; }
    public int CancelledPermits { get; set; }
    public int ExpiredPermits { get; set; }
    public int OverduePermits { get; set; }
    public int HighRiskPermits { get; set; }
    public int CriticalRiskPermits { get; set; }
}