using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;

namespace Harmoni360.Domain.Entities;

public class TrainingParticipant : BaseEntity, IAuditableEntity
{
    public int TrainingId { get; private set; }
    public int UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string UserDepartment { get; private set; } = string.Empty;
    public string UserPosition { get; private set; } = string.Empty;
    public string UserEmail { get; private set; } = string.Empty;
    public string UserPhone { get; private set; } = string.Empty;
    
    public ParticipantStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public string EnrolledBy { get; private set; } = string.Empty;
    
    // Attendance Information
    public DateTime? AttendanceStartTime { get; private set; }
    public DateTime? AttendanceEndTime { get; private set; }
    public decimal AttendancePercentage { get; private set; }
    public string AttendanceNotes { get; private set; } = string.Empty;
    
    // Assessment Results
    public decimal? FinalScore { get; private set; }
    public bool HasPassed { get; private set; }
    public DateTime? AssessmentDate { get; private set; }
    public string AssessmentNotes { get; private set; } = string.Empty;
    public AssessmentMethod? AssessmentMethodUsed { get; private set; }
    
    // Feedback and Evaluation
    public decimal? TrainingRating { get; private set; } // Rating given by participant about the training
    public string TrainingFeedback { get; private set; } = string.Empty;
    public string InstructorFeedback { get; private set; } = string.Empty; // Feedback from instructor about participant
    
    // Prerequisites and Requirements
    public bool HasMetPrerequisites { get; private set; } = true;
    public string PrerequisiteNotes { get; private set; } = string.Empty;
    public DateTime? PrerequisiteVerificationDate { get; private set; }
    
    // Indonesian Compliance Fields
    public string EmployeeId { get; private set; } = string.Empty;
    public string K3LicenseNumber { get; private set; } = string.Empty; // If participant has K3 license
    public bool IsBPJSRegistered { get; private set; }
    public string BPJSNumber { get; private set; } = string.Empty;
    public bool IsIndonesianCitizen { get; private set; } = true;
    public string WorkPermitNumber { get; private set; } = string.Empty; // For foreign workers
    
    // Completion and Certification
    public DateTime? CompletedAt { get; private set; }
    public bool IsEligibleForCertificate { get; private set; }
    public string CompletionNotes { get; private set; } = string.Empty;
    public DateTime? CertificateIssuedAt { get; private set; }
    public string CertificateNumber { get; private set; } = string.Empty;
    
    // Navigation Properties
    public Training? Training { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private TrainingParticipant() { } // For EF Core

    public static TrainingParticipant Create(
        int trainingId,
        int userId,
        string userName,
        string userDepartment,
        string userPosition,
        string enrolledBy,
        string userEmail = "",
        string userPhone = "",
        string employeeId = "")
    {
        var participant = new TrainingParticipant
        {
            TrainingId = trainingId,
            UserId = userId,
            UserName = userName,
            UserDepartment = userDepartment,
            UserPosition = userPosition,
            UserEmail = userEmail,
            UserPhone = userPhone,
            EmployeeId = employeeId,
            Status = ParticipantStatus.Enrolled,
            EnrolledAt = DateTime.UtcNow,
            EnrolledBy = enrolledBy,
            HasMetPrerequisites = true,
            IsIndonesianCitizen = true,
            IsBPJSRegistered = false,
            AttendancePercentage = 0,
            IsEligibleForCertificate = false,
            CreatedAt = DateTime.UtcNow
        };

        return participant;
    }

    public void UpdateContactInformation(string email, string phone)
    {
        UserEmail = email;
        UserPhone = phone;
    }

    public void SetIndonesianComplianceInfo(
        bool isIndonesianCitizen,
        bool isBPJSRegistered,
        string bpjsNumber = "",
        string workPermitNumber = "",
        string k3LicenseNumber = "")
    {
        IsIndonesianCitizen = isIndonesianCitizen;
        IsBPJSRegistered = isBPJSRegistered;
        BPJSNumber = bpjsNumber;
        WorkPermitNumber = workPermitNumber;
        K3LicenseNumber = k3LicenseNumber;
    }

    public void VerifyPrerequisites(bool hasMet, string notes = "", string verifiedBy = "")
    {
        HasMetPrerequisites = hasMet;
        PrerequisiteNotes = notes;
        PrerequisiteVerificationDate = DateTime.UtcNow;
        
        if (!hasMet)
        {
            Status = ParticipantStatus.Pending;
        }
        else if (Status == ParticipantStatus.Pending)
        {
            Status = ParticipantStatus.Enrolled;
        }
    }

    public void MarkAsAttending()
    {
        if (Status != ParticipantStatus.Enrolled)
            throw new InvalidOperationException("Only enrolled participants can be marked as attending.");

        if (!HasMetPrerequisites)
            throw new InvalidOperationException("Participant must meet prerequisites before attending.");

        Status = ParticipantStatus.Attending;
        AttendanceStartTime = DateTime.UtcNow;
    }

    public void RecordAttendance(DateTime startTime, DateTime endTime, decimal attendancePercentage, string notes = "")
    {
        if (attendancePercentage < 0 || attendancePercentage > 100)
            throw new ArgumentException("Attendance percentage must be between 0 and 100.");

        AttendanceStartTime = startTime;
        AttendanceEndTime = endTime;
        AttendancePercentage = attendancePercentage;
        AttendanceNotes = notes;
        
        // Update eligibility for certificate based on attendance
        UpdateCertificateEligibility();
    }

    public void RecordAssessmentResult(
        decimal score,
        decimal passingScore,
        AssessmentMethod method,
        string notes = "",
        string assessedBy = "")
    {
        if (Status != ParticipantStatus.Attending)
            throw new InvalidOperationException("Only attending participants can have assessment results recorded.");

        if (score < 0 || score > 100)
            throw new ArgumentException("Score must be between 0 and 100.");

        FinalScore = score;
        HasPassed = score >= passingScore;
        AssessmentDate = DateTime.UtcNow;
        AssessmentNotes = notes;
        AssessmentMethodUsed = method;
        
        // Update status based on assessment result
        if (HasPassed)
        {
            Status = ParticipantStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            CompletionNotes = $"Completed with score: {score}%";
        }
        else
        {
            Status = ParticipantStatus.Failed;
            CompletionNotes = $"Failed with score: {score}% (Required: {passingScore}%)";
        }
        
        UpdateCertificateEligibility();
    }

    public void ProvideTrainingFeedback(decimal rating, string feedback)
    {
        if (Status != ParticipantStatus.Completed)
            throw new InvalidOperationException("Only completed participants can provide feedback.");

        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        TrainingRating = rating;
        TrainingFeedback = feedback;
    }

    public void AddInstructorFeedback(string feedback)
    {
        InstructorFeedback = feedback;
    }

    public void MarkAsNoShow()
    {
        if (Status != ParticipantStatus.Enrolled)
            throw new InvalidOperationException("Only enrolled participants can be marked as no-show.");

        Status = ParticipantStatus.NoShow;
        AttendancePercentage = 0;
        CompletionNotes = "Marked as no-show";
    }

    public void MarkAsWithdrawn()
    {
        if (Status == ParticipantStatus.Completed)
            throw new InvalidOperationException("Completed participants cannot be withdrawn.");

        Status = ParticipantStatus.Withdrawn;
        CompletionNotes = "Withdrawn from training";
    }

    public void IssueCertificate(string certificateNumber)
    {
        if (!IsEligibleForCertificate)
            throw new InvalidOperationException("Participant is not eligible for certificate.");

        if (Status != ParticipantStatus.Completed)
            throw new InvalidOperationException("Only completed participants can receive certificates.");

        CertificateNumber = certificateNumber;
        CertificateIssuedAt = DateTime.UtcNow;
    }

    public void RetakeAssessment()
    {
        if (Status != ParticipantStatus.Failed)
            throw new InvalidOperationException("Only failed participants can retake assessment.");

        Status = ParticipantStatus.Attending;
        FinalScore = null;
        HasPassed = false;
        AssessmentDate = null;
        AssessmentNotes = string.Empty;
        CompletionNotes = "Retaking assessment";
        IsEligibleForCertificate = false;
    }

    private void UpdateCertificateEligibility()
    {
        // Check if participant meets minimum requirements for certificate
        var minimumAttendance = 80.0m; // 80% attendance required
        var hasPassedAssessment = HasPassed;
        var hasMetAttendanceRequirement = AttendancePercentage >= minimumAttendance;
        
        IsEligibleForCertificate = hasPassedAssessment && hasMetAttendanceRequirement && HasMetPrerequisites;
    }

    public bool CanRetakeAssessment()
    {
        return Status == ParticipantStatus.Failed && 
               AttendancePercentage >= 80.0m && 
               HasMetPrerequisites;
    }

    public bool RequiresSpecialAccommodation()
    {
        // For Indonesian compliance, check if foreign worker needs special accommodation
        return !IsIndonesianCitizen && !string.IsNullOrEmpty(WorkPermitNumber);
    }

    public decimal CalculateOverallPerformance()
    {
        if (!FinalScore.HasValue)
            return 0;

        // Weight: 70% assessment score, 30% attendance
        var assessmentWeight = 0.7m;
        var attendanceWeight = 0.3m;
        
        return (FinalScore.Value * assessmentWeight) + (AttendancePercentage * attendanceWeight);
    }
}