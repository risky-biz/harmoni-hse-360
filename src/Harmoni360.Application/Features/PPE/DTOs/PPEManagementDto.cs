namespace Harmoni360.Application.Features.PPE.DTOs;

public class PPECategoryManagementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool RequiresCertification { get; set; }
    public bool RequiresInspection { get; set; }
    public int? InspectionIntervalDays { get; set; }
    public bool RequiresExpiry { get; set; }
    public int? DefaultExpiryDays { get; set; }
    public string? ComplianceStandard { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public int ItemCount { get; set; } // Number of items in this category
}

public class PPEManagementStatsDto
{
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int TotalSizes { get; set; }
    public int ActiveSizes { get; set; }
    public int TotalStorageLocations { get; set; }
    public int ActiveStorageLocations { get; set; }
    public int TotalPPEItems { get; set; }
    public DateTime LastUpdated { get; set; }
}