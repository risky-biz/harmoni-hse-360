using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Hazards.DTOs;

namespace Harmoni360.Application.Features.Hazards.Queries;

public record GetHazardsQuery : IRequest<GetHazardsResponse>
{
    // Pagination
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    
    // Filtering
    public string? SearchTerm { get; init; }
    public HazardCategory? Category { get; init; }
    public HazardType? Type { get; init; }
    public HazardStatus? Status { get; init; }
    public HazardSeverity? Severity { get; init; }
    public RiskLevel? RiskLevel { get; init; }
    public string? Location { get; init; }
    public string? Department { get; init; }
    public int? ReporterId { get; init; }
    public int? AssignedToId { get; init; }
    
    // Date filtering
    public DateTime? IdentifiedDateFrom { get; init; }
    public DateTime? IdentifiedDateTo { get; init; }
    public DateTime? ExpectedResolutionDateFrom { get; init; }
    public DateTime? ExpectedResolutionDateTo { get; init; }
    
    // Geographic filtering
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusKm { get; init; }
    
    // Sorting
    public string? SortBy { get; init; } = "IdentifiedDate";
    public string? SortDirection { get; init; } = "DESC";
    
    // Include options
    public bool IncludeAttachments { get; init; } = false;
    public bool IncludeRiskAssessments { get; init; } = false;
    public bool IncludeMitigationActions { get; init; } = false;
    public bool IncludeReporter { get; init; } = true;
    
    // Special filters
    public bool OnlyUnassessed { get; init; } = false;
    public bool OnlyOverdue { get; init; } = false;
    public bool OnlyHighRisk { get; init; } = false;
    public bool OnlyMyHazards { get; init; } = false;
}

public class GetHazardsResponse
{
    public List<HazardDto> Hazards { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    
    // Summary statistics
    public HazardsSummary Summary { get; set; } = new();
}

public class HazardsSummary
{
    public int TotalHazards { get; set; }
    public int OpenHazards { get; set; }
    public int HighRiskHazards { get; set; }
    public int OverdueActions { get; set; }
    public int UnassessedHazards { get; set; }
    public Dictionary<string, int> HazardsByCategory { get; set; } = new();
    public Dictionary<string, int> HazardsBySeverity { get; set; } = new();
    public Dictionary<string, int> HazardsByStatus { get; set; } = new();
}