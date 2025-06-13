using MediatR;
using Harmoni360.Application.Features.Hazards.DTOs;

namespace Harmoni360.Application.Features.Hazards.Queries;

public record GetHazardDashboardQuery : IRequest<HazardDashboardDto>
{
    // Date range for dashboard metrics
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    
    // Department filter for department-specific dashboards
    public string? Department { get; init; }
    
    // Include trends and analytics
    public bool IncludeTrends { get; init; } = true;
    public bool IncludeLocationAnalytics { get; init; } = true;
    public bool IncludeComplianceMetrics { get; init; } = true;
    public bool IncludePerformanceMetrics { get; init; } = true;
    
    // User context for personalized metrics
    public bool PersonalizedView { get; init; } = false;
}