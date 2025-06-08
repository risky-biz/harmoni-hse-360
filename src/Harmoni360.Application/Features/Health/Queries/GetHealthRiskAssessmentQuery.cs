using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetHealthRiskAssessmentQuery : IRequest<HealthRiskAssessmentDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Department { get; init; }
    public PersonType? PersonType { get; init; }
    public RiskAssessmentScope Scope { get; init; } = RiskAssessmentScope.Standard;
    public bool IncludeInactive { get; init; } = false;
    public bool IncludePredictiveAnalysis { get; init; } = true;
}

public enum RiskAssessmentScope
{
    Basic,          // Just critical conditions and compliance
    Standard,       // Include incident patterns and trends
    Comprehensive   // Full population health analysis with predictive insights
}