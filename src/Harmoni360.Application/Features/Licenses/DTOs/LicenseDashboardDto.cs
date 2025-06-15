namespace Harmoni360.Application.Features.Licenses.DTOs;

public class LicenseDashboardDto
{
    // License Status Summary
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int DraftLicenses { get; set; }
    public int PendingSubmissionLicenses { get; set; }
    public int SubmittedLicenses { get; set; }
    public int UnderReviewLicenses { get; set; }
    public int ApprovedLicenses { get; set; }
    public int RejectedLicenses { get; set; }
    public int ExpiredLicenses { get; set; }
    public int SuspendedLicenses { get; set; }
    public int RevokedLicenses { get; set; }
    public int PendingRenewalLicenses { get; set; }
    
    // Risk Analytics
    public int HighRiskLicenses { get; set; }
    public int CriticalLicenses { get; set; }
    public double ComplianceRate { get; set; }
    
    // Alerts
    public int ExpiringThisWeek { get; set; }
    public int ExpiringThisMonth { get; set; }
    public int OverdueLicenses { get; set; }
    public int LicensesDueForRenewal { get; set; }
    
    // Recent Activity
    public List<LicenseDto> RecentLicenses { get; set; } = new();
    public List<LicenseDto> ExpiringLicenses { get; set; } = new();
    public List<LicenseDto> HighPriorityLicenses { get; set; } = new();
    
    // Trends
    public List<LicenseMonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public List<LicenseTypeStatDto> LicensesByType { get; set; } = new();
    public List<LicenseAuthorityStatDto> LicensesByAuthority { get; set; } = new();
}

public class LicenseMonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Created { get; set; }
    public int Expired { get; set; }
    public int Renewed { get; set; }
    public int Active { get; set; }
}

public class LicenseTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class LicenseAuthorityStatDto
{
    public string Authority { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Active { get; set; }
    public int Expired { get; set; }
    public double ComplianceRate { get; set; }
}