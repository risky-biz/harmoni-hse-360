using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.DTOs;

public class WasteReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WasteClassification Classification { get; set; }
    public string ClassificationDisplay { get; set; } = string.Empty;
    public WasteReportStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? EstimatedQuantity { get; set; }
    public string? QuantityUnit { get; set; }
    public string? DisposalMethod { get; set; }
    public DateTime? DisposalDate { get; set; }
    public string? DisposedBy { get; set; }
    public decimal? DisposalCost { get; set; }
    public string? ContractorName { get; set; }
    public string? ManifestNumber { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public List<WasteCommentDto> Comments { get; set; } = new();
    
    // Computed Properties
    public bool CanEdit => Status == WasteReportStatus.Draft || Status == WasteReportStatus.UnderReview;
    public bool CanDispose => Status == WasteReportStatus.Approved && DisposalDate == null;
    public bool CanApprove => Status == WasteReportStatus.UnderReview;
    public bool CanReject => Status == WasteReportStatus.UnderReview;
    public bool CanArchive => Status == WasteReportStatus.Disposed || Status == WasteReportStatus.Rejected;
    public bool IsOverdue => Status == WasteReportStatus.Approved && DisposalDate == null && ReportDate.AddDays(30) < DateTime.UtcNow;
    public bool IsHighRisk => Classification == WasteClassification.HazardousChemical || Classification == WasteClassification.HazardousBiological || Classification == WasteClassification.HazardousRadioactive || Classification == WasteClassification.Medical;
    public bool HasComments => Comments.Any();
    public int CommentsCount => Comments.Count;
    public int DaysUntilDisposal => DisposalDate?.Subtract(DateTime.UtcNow).Days ?? 0;
    public int DaysOverdue => IsOverdue ? DateTime.UtcNow.Subtract(ReportDate.AddDays(30)).Days : 0;
}

public class WasteReportSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? EstimatedQuantity { get; set; }
    public string? QuantityUnit { get; set; }
    public DateTime? DisposalDate { get; set; }
    public decimal? DisposalCost { get; set; }
    public int CommentsCount { get; set; }
    public bool IsOverdue { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDispose { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WasteReportDashboardDto
{
    public int TotalReports { get; set; }
    public int DraftReports { get; set; }
    public int UnderReviewReports { get; set; }
    public int ApprovedReports { get; set; }
    public int DisposedReports { get; set; }
    public int RejectedReports { get; set; }
    public int OverdueReports { get; set; }
    
    public int HazardousWasteReports { get; set; }
    public int ChemicalWasteReports { get; set; }
    public int ReportsDueToday { get; set; }
    public int ReportsDueThisWeek { get; set; }
    
    public decimal TotalEstimatedQuantity { get; set; }
    public decimal TotalDisposalCost { get; set; }
    public decimal AverageDisposalTime { get; set; }
    
    public List<WasteTypeStatDto> ReportsByType { get; set; } = new();
    public List<WasteMonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public List<WasteReportSummaryDto> RecentReports { get; set; } = new();
    public List<WasteReportSummaryDto> HighPriorityReports { get; set; } = new();
}

public class WasteTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percentage { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
}

public class WasteMonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int TotalReports { get; set; }
    public int DisposedReports { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
}

public class WasteCommentDto
{
    public int Id { get; set; }
    public int WasteReportId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string CommentedBy { get; set; } = string.Empty;
    public DateTime CommentedAt { get; set; }
    public string? Category { get; set; }
    public bool IsInternal { get; set; }
}
