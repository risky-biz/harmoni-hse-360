namespace Harmoni360.Application.Features.PPE.DTOs;

public class PPEItemDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string Condition { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Cost { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Certification Info
    public string? CertificationNumber { get; set; }
    public string? CertifyingBody { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? CertificationExpiryDate { get; set; }
    public string? CertificationStandard { get; set; }
    
    // Maintenance Info
    public int? MaintenanceIntervalDays { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceInstructions { get; set; }
    
    // Computed Properties
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public bool IsMaintenanceDue { get; set; }
    public bool IsMaintenanceDueSoon { get; set; }
    public bool IsCertificationExpired { get; set; }
    public bool IsCertificationExpiringSoon { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public bool IsInspectionDue { get; set; }
    
    // Audit Info
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class PPEItemSummaryDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public bool IsMaintenanceDue { get; set; }
    public bool IsInspectionDue { get; set; }
}