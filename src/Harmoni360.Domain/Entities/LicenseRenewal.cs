using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class LicenseRenewal : BaseEntity, IAuditableEntity
{
    public int LicenseId { get; set; }
    public string RenewalNumber { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    public DateTime NewExpiryDate { get; set; }
    public LicenseRenewalStatus Status { get; set; }
    public string RenewalNotes { get; set; } = string.Empty;
    public decimal? RenewalFee { get; set; }
    public bool DocumentsRequired { get; set; }
    public bool InspectionRequired { get; set; }
    public DateTime? InspectionDate { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
    
    // Navigation Properties
    public License? License { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static LicenseRenewal Create(
        int licenseId,
        DateTime newExpiryDate,
        string initiatedBy,
        decimal? renewalFee = null,
        bool documentsRequired = false,
        bool inspectionRequired = false)
    {
        var renewalNumber = GenerateRenewalNumber(licenseId);
        
        return new LicenseRenewal
        {
            LicenseId = licenseId,
            RenewalNumber = renewalNumber,
            ApplicationDate = DateTime.UtcNow,
            NewExpiryDate = newExpiryDate,
            Status = LicenseRenewalStatus.Draft,
            RenewalFee = renewalFee,
            DocumentsRequired = documentsRequired,
            InspectionRequired = inspectionRequired,
            CreatedBy = initiatedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Submit(string submittedBy)
    {
        if (Status != LicenseRenewalStatus.Draft)
            throw new InvalidOperationException("Only draft renewals can be submitted.");
            
        Status = LicenseRenewalStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
        ProcessedBy = submittedBy;
    }

    public void Approve(string approvedBy, string approvalNotes = "")
    {
        if (Status != LicenseRenewalStatus.Submitted && Status != LicenseRenewalStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review renewals can be approved.");

        Status = LicenseRenewalStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        ProcessedBy = approvedBy;
        RenewalNotes = approvalNotes;
    }

    public void Reject(string rejectedBy, string rejectionReason)
    {
        if (Status != LicenseRenewalStatus.Submitted && Status != LicenseRenewalStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review renewals can be rejected.");

        Status = LicenseRenewalStatus.Rejected;
        RejectedDate = DateTime.UtcNow;
        ProcessedBy = rejectedBy;
        RenewalNotes = rejectionReason;
    }

    public void ScheduleInspection(DateTime inspectionDate)
    {
        if (!InspectionRequired)
            throw new InvalidOperationException("This renewal does not require inspection.");
            
        InspectionDate = inspectionDate;
    }

    private static string GenerateRenewalNumber(int licenseId)
    {
        var year = DateTime.UtcNow.Year;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"REN-{licenseId:D6}-{year:D4}-{timestamp % 1000:D3}";
    }
}