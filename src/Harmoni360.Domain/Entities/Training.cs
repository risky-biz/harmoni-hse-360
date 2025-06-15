using Harmoni360.Domain.Common;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class Training : BaseEntity, IAuditableEntity
{
    private readonly List<TrainingParticipant> _participants = new();
    private readonly List<TrainingRequirement> _requirements = new();
    private readonly List<TrainingAttachment> _attachments = new();
    private readonly List<TrainingComment> _comments = new();
    private readonly List<TrainingCertification> _certifications = new();

    public string TrainingCode { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TrainingType Type { get; private set; }
    public TrainingCategory Category { get; private set; }
    public TrainingStatus Status { get; private set; }
    public TrainingPriority Priority { get; private set; }
    public TrainingDeliveryMethod DeliveryMethod { get; private set; }
    
    // Scheduling Information
    public DateTime ScheduledStartDate { get; private set; }
    public DateTime ScheduledEndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public int DurationHours { get; private set; }
    public int MaxParticipants { get; private set; }
    public int MinParticipants { get; private set; }
    
    // Location Information
    public string Venue { get; private set; } = string.Empty;
    public string VenueAddress { get; private set; } = string.Empty;
    public GeoLocation? GeoLocation { get; private set; }
    public string OnlineLink { get; private set; } = string.Empty;
    public string OnlinePlatform { get; private set; } = string.Empty;
    
    // Training Content
    public string LearningObjectives { get; private set; } = string.Empty;
    public string CourseOutline { get; private set; } = string.Empty;
    public string Prerequisites { get; private set; } = string.Empty;
    public string Materials { get; private set; } = string.Empty;
    public AssessmentMethod AssessmentMethod { get; private set; }
    public decimal PassingScore { get; private set; }
    
    // Instructor Information
    public string InstructorName { get; private set; } = string.Empty;
    public string InstructorQualifications { get; private set; } = string.Empty;
    public string InstructorContact { get; private set; } = string.Empty;
    public bool IsExternalInstructor { get; private set; }
    
    // Certification Details
    public bool IssuesCertificate { get; private set; }
    public CertificationType CertificationType { get; private set; }
    public ValidityPeriod CertificateValidityPeriod { get; private set; }
    public string CertifyingBody { get; private set; } = string.Empty;
    
    // Cost Information
    public decimal CostPerParticipant { get; private set; }
    public decimal TotalBudget { get; private set; }
    public string Currency { get; private set; } = "IDR";
    
    // Indonesian Compliance Fields
    public bool IsK3MandatoryTraining { get; private set; }
    public string K3RegulationReference { get; private set; } = string.Empty;
    public bool IsBPJSCompliant { get; private set; }
    public string MinistryApprovalNumber { get; private set; } = string.Empty;
    public bool RequiresGovernmentCertification { get; private set; }
    public string IndonesianTrainingStandard { get; private set; } = string.Empty; // SKKNI, etc.
    
    // Evaluation and Feedback
    public decimal AverageRating { get; private set; }
    public int TotalRatings { get; private set; }
    public string EvaluationSummary { get; private set; } = string.Empty;
    public string ImprovementActions { get; private set; } = string.Empty;
    
    // Navigation Properties
    public IReadOnlyCollection<TrainingParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<TrainingRequirement> Requirements => _requirements.AsReadOnly();
    public IReadOnlyCollection<TrainingAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<TrainingComment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<TrainingCertification> Certifications => _certifications.AsReadOnly();
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private Training() { } // For EF Core

    public static Training Create(
        string title,
        string description,
        TrainingType type,
        TrainingCategory category,
        TrainingDeliveryMethod deliveryMethod,
        DateTime scheduledStartDate,
        DateTime scheduledEndDate,
        string venue,
        int maxParticipants,
        int minParticipants,
        string instructorName,
        decimal costPerParticipant = 0,
        GeoLocation? geoLocation = null)
    {
        var training = new Training
        {
            TrainingCode = GenerateTrainingCode(type, category),
            Title = title,
            Description = description,
            Type = type,
            Category = category,
            Status = TrainingStatus.Draft,
            Priority = DeterminePriority(type, category),
            DeliveryMethod = deliveryMethod,
            ScheduledStartDate = scheduledStartDate,
            ScheduledEndDate = scheduledEndDate,
            DurationHours = (int)(scheduledEndDate - scheduledStartDate).TotalHours,
            Venue = venue,
            GeoLocation = geoLocation,
            MaxParticipants = maxParticipants,
            MinParticipants = minParticipants,
            InstructorName = instructorName,
            CostPerParticipant = costPerParticipant,
            TotalBudget = costPerParticipant * maxParticipants,
            AssessmentMethod = AssessmentMethod.Written,
            PassingScore = 70.0m,
            IssuesCertificate = false,
            CertificationType = CertificationType.Completion,
            CertificateValidityPeriod = ValidityPeriod.OneYear,
            CreatedAt = DateTime.UtcNow
        };

        // Set Indonesian compliance defaults for safety-related training
        if (IsSafetyRelatedTraining(type))
        {
            training.IsK3MandatoryTraining = true;
            training.IsBPJSCompliant = true;
            training.RequiresGovernmentCertification = true;
        }

        // Raise domain event
        training.AddDomainEvent(new TrainingCreatedEvent(training.Id, training.Title, training.Type));
        
        return training;
    }

    public void UpdateDetails(
        string title,
        string description,
        DateTime scheduledStartDate,
        DateTime scheduledEndDate,
        string venue,
        int maxParticipants,
        int minParticipants,
        GeoLocation? geoLocation = null)
    {
        if (Status == TrainingStatus.InProgress || Status == TrainingStatus.Completed)
            throw new InvalidOperationException("Cannot update training details while in progress or completed.");

        Title = title;
        Description = description;
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        DurationHours = (int)(scheduledEndDate - scheduledStartDate).TotalHours;
        Venue = venue;
        MaxParticipants = maxParticipants;
        MinParticipants = minParticipants;
        GeoLocation = geoLocation;
        TotalBudget = CostPerParticipant * maxParticipants;
        
        AddDomainEvent(new TrainingUpdatedEvent(Id, Title));
    }

    public void ScheduleTraining(string scheduledBy)
    {
        if (Status != TrainingStatus.Draft)
            throw new InvalidOperationException("Only draft trainings can be scheduled.");

        if (_participants.Count < MinParticipants)
            throw new InvalidOperationException($"Cannot schedule training with less than {MinParticipants} participants.");

        Status = TrainingStatus.Scheduled;
        AddDomainEvent(new TrainingScheduledEvent(Id, Title, ScheduledStartDate));
    }

    public void StartTraining(string startedBy)
    {
        if (Status != TrainingStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled trainings can be started.");

        Status = TrainingStatus.InProgress;
        ActualStartDate = DateTime.UtcNow;
        
        // Update participant statuses
        foreach (var participant in _participants.Where(p => p.Status == ParticipantStatus.Enrolled))
        {
            participant.MarkAsAttending();
        }
        
        AddDomainEvent(new TrainingStartedEvent(Id, Title, InstructorName));
    }

    public void CompleteTraining(string completedBy, string evaluationSummary = "")
    {
        if (Status != TrainingStatus.InProgress)
            throw new InvalidOperationException("Only in-progress trainings can be completed.");

        Status = TrainingStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        EvaluationSummary = evaluationSummary;
        
        // Generate certificates for successful participants
        if (IssuesCertificate)
        {
            var successfulParticipants = _participants.Where(p => p.Status == ParticipantStatus.Completed).ToList();
            foreach (var participant in successfulParticipants)
            {
                GenerateCertificate(participant);
            }
        }
        
        AddDomainEvent(new TrainingCompletedEvent(Id, Title, _participants.Count, InstructorName));
    }

    public void CancelTraining(string cancelledBy, string reason)
    {
        if (Status == TrainingStatus.Completed)
            throw new InvalidOperationException("Completed trainings cannot be cancelled.");

        Status = TrainingStatus.Cancelled;
        
        // Update all participant statuses
        foreach (var participant in _participants.Where(p => p.Status == ParticipantStatus.Enrolled || p.Status == ParticipantStatus.Attending))
        {
            participant.MarkAsWithdrawn();
        }
        
        AddDomainEvent(new TrainingCancelledEvent(Id, Title, reason));
    }

    public void EnrollParticipant(int userId, string userName, string userDepartment, string userPosition, string enrolledBy)
    {
        if (Status == TrainingStatus.Completed || Status == TrainingStatus.Cancelled)
            throw new InvalidOperationException("Cannot enroll participants in completed or cancelled training.");

        if (_participants.Count >= MaxParticipants)
            throw new InvalidOperationException("Training has reached maximum participant capacity.");

        if (_participants.Any(p => p.UserId == userId))
            throw new InvalidOperationException("Participant is already enrolled in this training.");

        var participant = TrainingParticipant.Create(
            Id, userId, userName, userDepartment, userPosition, enrolledBy);

        _participants.Add(participant);
        AddDomainEvent(new TrainingParticipantEnrolledEvent(Id, userId, userName));
    }

    public void RemoveParticipant(int userId, string removedBy, string reason = "")
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new InvalidOperationException("Participant not found in this training.");

        if (participant.Status == ParticipantStatus.Completed)
            throw new InvalidOperationException("Cannot remove participants who have completed the training.");

        participant.MarkAsWithdrawn();
        AddDomainEvent(new TrainingParticipantRemovedEvent(Id, userId, participant.UserName, reason));
    }

    public void SetInstructorDetails(string instructorName, string qualifications, string contact, bool isExternal = false)
    {
        InstructorName = instructorName;
        InstructorQualifications = qualifications;
        InstructorContact = contact;
        IsExternalInstructor = isExternal;
    }

    public void SetCertificationDetails(bool issuesCertificate, CertificationType certificationType, ValidityPeriod validityPeriod, string certifyingBody = "")
    {
        IssuesCertificate = issuesCertificate;
        CertificationType = certificationType;
        CertificateValidityPeriod = validityPeriod;
        CertifyingBody = certifyingBody;
    }

    public void SetIndonesianCompliance(bool isK3Mandatory, string k3RegulationReference, bool isBPJSCompliant, string ministryApprovalNumber = "", string trainingStandard = "")
    {
        IsK3MandatoryTraining = isK3Mandatory;
        K3RegulationReference = k3RegulationReference;
        IsBPJSCompliant = isBPJSCompliant;
        MinistryApprovalNumber = ministryApprovalNumber;
        IndonesianTrainingStandard = trainingStandard;

        if (isK3Mandatory)
        {
            RequiresGovernmentCertification = true;
            Priority = TrainingPriority.Mandatory;
        }
    }

    public void SetAssessmentDetails(AssessmentMethod method, decimal passingScore)
    {
        if (passingScore < 0 || passingScore > 100)
            throw new ArgumentException("Passing score must be between 0 and 100.");

        AssessmentMethod = method;
        PassingScore = passingScore;
    }

    public void AddRequirement(string requirementDescription, bool isMandatory, DateTime? dueDate = null)
    {
        var requirement = TrainingRequirement.Create(Id, requirementDescription, isMandatory, dueDate);
        _requirements.Add(requirement);
    }

    public void AddComment(string content, TrainingCommentType commentType, string authorName, int? authorId = null)
    {
        var comment = TrainingComment.Create(Id, content, commentType, authorName, authorId);
        _comments.Add(comment);
    }

    public void UpdateRating(decimal rating, int participantCount)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.");

        // Calculate new average rating
        var totalScore = (AverageRating * TotalRatings) + (rating * participantCount);
        TotalRatings += participantCount;
        AverageRating = TotalRatings > 0 ? totalScore / TotalRatings : 0;

        AddDomainEvent(new TrainingRatedEvent(Id, "System", rating, null));
    }

    private void GenerateCertificate(TrainingParticipant participant)
    {
        DateTime? validUntil = CertificateValidityPeriod switch
        {
            ValidityPeriod.OneMonth => DateTime.UtcNow.AddMonths(1),
            ValidityPeriod.ThreeMonths => DateTime.UtcNow.AddMonths(3),
            ValidityPeriod.SixMonths => DateTime.UtcNow.AddMonths(6),
            ValidityPeriod.OneYear => DateTime.UtcNow.AddYears(1),
            ValidityPeriod.TwoYears => DateTime.UtcNow.AddYears(2),
            ValidityPeriod.ThreeYears => DateTime.UtcNow.AddYears(3),
            ValidityPeriod.FiveYears => DateTime.UtcNow.AddYears(5),
            ValidityPeriod.Indefinite => null,
            _ => DateTime.UtcNow.AddYears(1)
        };

        var certification = TrainingCertification.Create(
            Id, participant.UserId, participant.UserName, CertificationType, 
            CertifyingBody, validUntil, participant.FinalScore);

        _certifications.Add(certification);
        AddDomainEvent(new TrainingCertificationIssuedEvent(Id, certification.Id, participant.UserName, certification.CertificateNumber));
    }

    private static string GenerateTrainingCode(TrainingType type, TrainingCategory category)
    {
        var typePrefix = type switch
        {
            TrainingType.SafetyOrientation => "SO",
            TrainingType.HSETraining => "HSE",
            TrainingType.K3Training => "K3",
            TrainingType.BPJSCompliance => "BPJS",
            TrainingType.PermitToWork => "PTW",
            TrainingType.ConfinedSpaceEntry => "CSE",
            TrainingType.HotWorkSafety => "HWS",
            TrainingType.ElectricalSafety => "ES",
            TrainingType.FireSafety => "FS",
            TrainingType.TechnicalSkills => "TS",
            TrainingType.LeadershipDevelopment => "LD",
            _ => "TRN"
        };

        var categoryPrefix = category switch
        {
            TrainingCategory.MandatoryCompliance => "MC",
            TrainingCategory.SafetyTraining => "ST",
            TrainingCategory.InductionTraining => "IN",
            TrainingCategory.RefresherTraining => "RF",
            _ => "GN"
        };

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        
        return $"{typePrefix}-{categoryPrefix}-{year:D4}{month:D2}-{timestamp % 10000:D4}{random:D4}";
    }

    private static TrainingPriority DeterminePriority(TrainingType type, TrainingCategory category)
    {
        if (category == TrainingCategory.MandatoryCompliance)
            return TrainingPriority.Mandatory;

        return type switch
        {
            TrainingType.SafetyOrientation => TrainingPriority.Mandatory,
            TrainingType.K3Training => TrainingPriority.Mandatory,
            TrainingType.BPJSCompliance => TrainingPriority.Mandatory,
            TrainingType.ConfinedSpaceEntry => TrainingPriority.Critical,
            TrainingType.HotWorkSafety => TrainingPriority.Critical,
            TrainingType.ElectricalSafety => TrainingPriority.High,
            TrainingType.FireSafety => TrainingPriority.High,
            TrainingType.EmergencyResponse => TrainingPriority.High,
            TrainingType.HSETraining => TrainingPriority.High,
            _ => TrainingPriority.Medium
        };
    }

    private static bool IsSafetyRelatedTraining(TrainingType type)
    {
        return type switch
        {
            TrainingType.SafetyOrientation => true,
            TrainingType.HSETraining => true,
            TrainingType.K3Training => true,
            TrainingType.EmergencyResponse => true,
            TrainingType.ConfinedSpaceEntry => true,
            TrainingType.HotWorkSafety => true,
            TrainingType.ElectricalSafety => true,
            TrainingType.FireSafety => true,
            TrainingType.ChemicalHandling => true,
            TrainingType.PersonalProtectiveEquipment => true,
            _ => false
        };
    }
}