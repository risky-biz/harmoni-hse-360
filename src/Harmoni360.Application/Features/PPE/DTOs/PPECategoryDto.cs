namespace Harmoni360.Application.Features.PPE.DTOs;

public class PPECategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool RequiresCertification { get; set; }
    public bool RequiresInspection { get; set; }
    public int? InspectionIntervalDays { get; set; }
    public bool RequiresExpiry { get; set; }
    public int? DefaultExpiryDays { get; set; }
    public string? ComplianceStandard { get; set; }
    public bool IsActive { get; set; }
}