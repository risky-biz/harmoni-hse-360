namespace Harmoni360.Application.Features.Trainings.DTOs;

public class TrainingParticipantDto
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserDepartment { get; set; } = string.Empty;
    public string UserPosition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Enrollment Information
    public DateTime EnrolledAt { get; set; }
    public string EnrolledBy { get; set; } = string.Empty;
    public string EnrollmentNotes { get; set; } = string.Empty;
    
    // Attendance Information
    public bool AttendanceMarked { get; set; }
    public DateTime? AttendanceDate { get; set; }
    public string? AttendanceMarkedBy { get; set; }
    public string? AttendanceNotes { get; set; }
    
    // Assessment Results
    public decimal? FinalScore { get; set; }
    public bool? Passed { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AssessedBy { get; set; }
    public string? AssessmentNotes { get; set; }
    
    // Certification Information
    public bool CertificateIssued { get; set; }
    public DateTime? CertificateIssuedAt { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    
    // Feedback
    public string? Feedback { get; set; }
    public decimal? Rating { get; set; }
    public DateTime? FeedbackDate { get; set; }
    
    // Prerequisites
    public bool PrerequisitesMet { get; set; }
    public string? PrerequisiteNotes { get; set; }
    public DateTime? PrerequisiteCheckDate { get; set; }
    public string? PrerequisiteCheckedBy { get; set; }
    
    // Computed Properties
    public bool CanMarkAttendance => Status == "Enrolled" && !AttendanceMarked;
    public bool CanRecordResults => AttendanceMarked && !Passed.HasValue;
    public bool CanIssueCertificate => Passed == true && !CertificateIssued;
    public string StatusBadgeColor => Status switch
    {
        "Enrolled" => "info",
        "Attending" => "warning",
        "Completed" => "success",
        "Failed" => "danger",
        "NoShow" => "secondary",
        "Withdrawn" => "dark",
        _ => "secondary"
    };
}