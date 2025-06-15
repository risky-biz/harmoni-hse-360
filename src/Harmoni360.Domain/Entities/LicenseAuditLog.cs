using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class LicenseAuditLog : BaseEntity, IAuditableEntity
{
    public int LicenseId { get; set; }
    public LicenseAuditAction Action { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public string OldValues { get; set; } = string.Empty; // JSON
    public string NewValues { get; set; } = string.Empty; // JSON
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    
    // Navigation Properties
    public License? License { get; set; }
    
    // Audit Fields  
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static LicenseAuditLog Create(
        int licenseId,
        LicenseAuditAction action,
        string actionDescription,
        string performedBy,
        string oldValues = "",
        string newValues = "",
        string comments = "")
    {
        return new LicenseAuditLog
        {
            LicenseId = licenseId,
            Action = action,
            ActionDescription = actionDescription,
            OldValues = oldValues,
            NewValues = newValues,
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            Comments = comments,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = performedBy
        };
    }

    public void SetRequestInfo(string ipAddress, string userAgent)
    {
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}