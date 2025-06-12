using MediatR;
using Harmoni360.Application.Features.RiskAssessments.DTOs;

namespace Harmoni360.Application.Features.RiskAssessments.Queries;

public record GetRiskAssessmentsQuery : IRequest<GetRiskAssessmentsResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Status { get; init; }
    public string? RiskLevel { get; init; }
    public string? AssessmentType { get; init; }
    public int? HazardId { get; init; }
    public bool? IsApproved { get; init; }
    public bool? IsActive { get; init; } = true; // Default to active assessments only
}

public record GetRiskAssessmentsResponse
{
    public List<RiskAssessmentDto> RiskAssessments { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}