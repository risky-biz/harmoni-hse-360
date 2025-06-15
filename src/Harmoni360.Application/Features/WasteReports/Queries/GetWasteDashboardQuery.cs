using MediatR;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetWasteDashboardQuery() : IRequest<WasteDashboardDto>;

public class WasteDashboardDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int CompletedReports { get; set; }
    public int TotalDisposalProviders { get; set; }
    public int ActiveDisposalProviders { get; set; }
    public List<WasteCategoryStatsDto> CategoryStats { get; set; } = new();
    public List<MonthlyWasteStatsDto> MonthlyStats { get; set; } = new();
    public List<WasteReportDto> RecentReports { get; set; } = new();
    public List<DisposalProviderDto> ExpiringProviders { get; set; } = new();
}

public class WasteCategoryStatsDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class MonthlyWasteStatsDto
{
    public string Month { get; set; } = string.Empty;
    public int ReportCount { get; set; }
}

public class DisposalProviderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}