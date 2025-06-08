using Harmoni360.Application.Features.PPE.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.PPE.Queries;

public record GetPPECategoriesQuery : IRequest<List<PPECategoryDto>>
{
    public bool IncludeInactive { get; init; } = false;
}

public record GetPPEDashboardQuery : IRequest<PPEDashboardDto>;

public class PPEDashboardDto
{
    public int TotalItems { get; set; }
    public int AvailableItems { get; set; }
    public int AssignedItems { get; set; }
    public int OutOfServiceItems { get; set; }
    public int ExpiredItems { get; set; }
    public int ExpiringSoonItems { get; set; }
    public int MaintenanceDueItems { get; set; }
    public int InspectionDueItems { get; set; }
    public int LostItems { get; set; }
    public int RetiredItems { get; set; }
    
    public List<PPECategoryStatsDto> CategoryStats { get; set; } = new();
    public List<PPEStatusStatsDto> StatusStats { get; set; } = new();
    public List<PPEConditionStatsDto> ConditionStats { get; set; } = new();
    public List<PPEExpiryWarningDto> ExpiryWarnings { get; set; } = new();
    public List<PPEMaintenanceWarningDto> MaintenanceWarnings { get; set; } = new();
}

public class PPECategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int AvailableItems { get; set; }
    public int AssignedItems { get; set; }
}

public class PPEStatusStatsDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class PPEConditionStatsDto
{
    public string Condition { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class PPEExpiryWarningDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsExpired { get; set; }
}

public class PPEMaintenanceWarningDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public bool IsOverdue { get; set; }
}