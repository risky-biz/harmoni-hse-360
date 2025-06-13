using Harmoni360.Domain.Common;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class WorkPermit : BaseEntity, IAuditableEntity
{
    private readonly List<WorkPermitAttachment> _attachments = new();
    private readonly List<WorkPermitApproval> _approvals = new();
    private readonly List<WorkPermitHazard> _hazards = new();
    private readonly List<WorkPermitPrecaution> _precautions = new();

    public string PermitNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WorkPermitType Type { get; private set; }
    public WorkPermitStatus Status { get; private set; }
    public WorkPermitPriority Priority { get; private set; }
    
    // Work Details
    public string WorkLocation { get; private set; } = string.Empty;
    public GeoLocation? GeoLocation { get; private set; }
    public DateTime PlannedStartDate { get; private set; }
    public DateTime PlannedEndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public int EstimatedDuration { get; private set; } // in hours
    
    // Personnel
    public int RequestedById { get; private set; }
    public string RequestedByName { get; private set; } = string.Empty;
    public string RequestedByDepartment { get; private set; } = string.Empty;
    public string RequestedByPosition { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public string WorkSupervisor { get; private set; } = string.Empty;
    public string SafetyOfficer { get; private set; } = string.Empty;
    
    // Work Scope
    public string WorkScope { get; private set; } = string.Empty;
    public string EquipmentToBeUsed { get; private set; } = string.Empty;
    public string MaterialsInvolved { get; private set; } = string.Empty;
    public int NumberOfWorkers { get; private set; }
    public string ContractorCompany { get; private set; } = string.Empty;
    
    // Safety Requirements
    public bool RequiresHotWorkPermit { get; private set; }
    public bool RequiresConfinedSpaceEntry { get; private set; }
    public bool RequiresElectricalIsolation { get; private set; }
    public bool RequiresHeightWork { get; private set; }
    public bool RequiresRadiationWork { get; private set; }
    public bool RequiresExcavation { get; private set; }
    public bool RequiresFireWatch { get; private set; }
    public bool RequiresGasMonitoring { get; private set; }
    
    // Indonesian Work Permit Compliance
    public string K3LicenseNumber { get; private set; } = string.Empty; // Keselamatan dan Kesehatan Kerja
    public string CompanyWorkPermitNumber { get; private set; } = string.Empty;
    public bool IsJamsostekCompliant { get; private set; } // BPJS Ketenagakerjaan compliance
    public bool HasSMK3Compliance { get; private set; } // Sistem Manajemen K3
    public string EnvironmentalPermitNumber { get; private set; } = string.Empty; // AMDAL/UKL-UPL
    
    // Risk Assessment
    public RiskLevel RiskLevel { get; private set; }
    public string RiskAssessmentSummary { get; private set; } = string.Empty;
    public string EmergencyProcedures { get; private set; } = string.Empty;
    
    // Completion
    public string CompletionNotes { get; private set; } = string.Empty;
    public bool IsCompletedSafely { get; private set; }
    public string LessonsLearned { get; private set; } = string.Empty;
    
    // Navigation Properties
    public IReadOnlyCollection<WorkPermitAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<WorkPermitApproval> Approvals => _approvals.AsReadOnly();
    public IReadOnlyCollection<WorkPermitHazard> Hazards => _hazards.AsReadOnly();
    public IReadOnlyCollection<WorkPermitPrecaution> Precautions => _precautions.AsReadOnly();
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private WorkPermit() { } // For EF Core

    public static WorkPermit Create(
        string title,
        string description,
        WorkPermitType type,
        string workLocation,
        DateTime plannedStartDate,
        DateTime plannedEndDate,
        int requestedById,
        string requestedByName,
        string requestedByDepartment,
        string requestedByPosition,
        string contactPhone,
        string workScope,
        int numberOfWorkers,
        GeoLocation? geoLocation = null)
    {
        var workPermit = new WorkPermit
        {
            PermitNumber = GeneratePermitNumber(type),
            Title = title,
            Description = description,
            Type = type,
            Status = WorkPermitStatus.Draft,
            Priority = DeterminePriority(type),
            WorkLocation = workLocation,
            GeoLocation = geoLocation,
            PlannedStartDate = plannedStartDate,
            PlannedEndDate = plannedEndDate,
            EstimatedDuration = (int)(plannedEndDate - plannedStartDate).TotalHours,
            RequestedById = requestedById,
            RequestedByName = requestedByName,
            RequestedByDepartment = requestedByDepartment,
            RequestedByPosition = requestedByPosition,
            ContactPhone = contactPhone,
            WorkScope = workScope,
            NumberOfWorkers = numberOfWorkers,
            RiskLevel = RiskLevel.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        workPermit.AddDomainEvent(new WorkPermitCreatedEvent(workPermit.Id, workPermit.PermitNumber, workPermit.Type));
        
        return workPermit;
    }

    public void UpdateDetails(
        string title,
        string description,
        string workLocation,
        DateTime plannedStartDate,
        DateTime plannedEndDate,
        string workScope,
        int numberOfWorkers,
        GeoLocation? geoLocation = null)
    {
        Title = title;
        Description = description;
        WorkLocation = workLocation;
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        EstimatedDuration = (int)(plannedEndDate - plannedStartDate).TotalHours;
        WorkScope = workScope;
        NumberOfWorkers = numberOfWorkers;
        GeoLocation = geoLocation;
        
        AddDomainEvent(new WorkPermitUpdatedEvent(Id, PermitNumber));
    }

    public void SubmitForApproval(string submittedBy)
    {
        if (Status != WorkPermitStatus.Draft)
            throw new InvalidOperationException("Only draft permits can be submitted for approval.");
            
        Status = WorkPermitStatus.PendingApproval;
        AddDomainEvent(new WorkPermitSubmittedEvent(Id, PermitNumber, submittedBy));
    }

    public void Approve(int approvedById, string approvedByName, string approvalLevel, string comments = "")
    {
        if (Status != WorkPermitStatus.PendingApproval)
            throw new InvalidOperationException("Only permits pending approval can be approved.");

        var approval = new WorkPermitApproval
        {
            WorkPermitId = Id,
            ApprovedById = approvedById,
            ApprovedByName = approvedByName,
            ApprovalLevel = approvalLevel,
            ApprovedAt = DateTime.UtcNow,
            Comments = comments,
            IsApproved = true
        };

        _approvals.Add(approval);
        
        // Check if all required approvals are obtained
        if (HasAllRequiredApprovals())
        {
            Status = WorkPermitStatus.Approved;
            AddDomainEvent(new WorkPermitApprovedEvent(Id, PermitNumber, approvedByName));
        }
    }

    public void Reject(int rejectedById, string rejectedByName, string reason)
    {
        Status = WorkPermitStatus.Rejected;
        
        var rejection = new WorkPermitApproval
        {
            WorkPermitId = Id,
            ApprovedById = rejectedById,
            ApprovedByName = rejectedByName,
            ApprovalLevel = "Rejection",
            ApprovedAt = DateTime.UtcNow,
            Comments = reason,
            IsApproved = false
        };

        _approvals.Add(rejection);
        AddDomainEvent(new WorkPermitRejectedEvent(Id, PermitNumber, rejectedByName, reason));
    }

    public void StartWork(string startedBy)
    {
        if (Status != WorkPermitStatus.Approved)
            throw new InvalidOperationException("Only approved permits can be started.");
            
        Status = WorkPermitStatus.InProgress;
        ActualStartDate = DateTime.UtcNow;
        AddDomainEvent(new WorkPermitStartedEvent(Id, PermitNumber, startedBy));
    }

    public void CompleteWork(string completedBy, string completionNotes, bool isCompletedSafely, string? lessonsLearned = null)
    {
        if (Status != WorkPermitStatus.InProgress)
            throw new InvalidOperationException("Only in-progress permits can be completed.");
            
        Status = WorkPermitStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        CompletionNotes = completionNotes;
        IsCompletedSafely = isCompletedSafely;
        LessonsLearned = lessonsLearned ?? string.Empty;
        
        AddDomainEvent(new WorkPermitCompletedEvent(Id, PermitNumber, completedBy, isCompletedSafely));
    }

    public void Cancel(string cancelledBy, string reason)
    {
        if (Status == WorkPermitStatus.Completed || Status == WorkPermitStatus.Cancelled)
            throw new InvalidOperationException("Completed or already cancelled permits cannot be cancelled.");
            
        Status = WorkPermitStatus.Cancelled;
        CompletionNotes = $"Cancelled: {reason}";
        
        AddDomainEvent(new WorkPermitCancelledEvent(Id, PermitNumber, cancelledBy, reason));
    }

    public void AddHazard(string hazardDescription, int? categoryId, RiskLevel riskLevel, string controlMeasures, string responsiblePerson = "")
    {
        var hazard = new WorkPermitHazard
        {
            WorkPermitId = Id,
            HazardDescription = hazardDescription,
            CategoryId = categoryId,
            RiskLevel = riskLevel,
            ControlMeasures = controlMeasures,
            ResponsiblePerson = responsiblePerson,
            ResidualRiskLevel = riskLevel,
            IsControlImplemented = false
        };

        _hazards.Add(hazard);
        UpdateRiskLevel();
    }

    public void AddPrecaution(string precautionDescription, PrecautionCategory category, bool isRequired = true, int priority = 1, string responsiblePerson = "", string verificationMethod = "", bool isK3Requirement = false, string k3StandardReference = "", bool isMandatoryByLaw = false)
    {
        var precaution = new WorkPermitPrecaution
        {
            WorkPermitId = Id,
            PrecautionDescription = precautionDescription,
            Category = category,
            IsRequired = isRequired,
            Priority = priority,
            ResponsiblePerson = responsiblePerson,
            VerificationMethod = verificationMethod,
            IsK3Requirement = isK3Requirement,
            K3StandardReference = k3StandardReference,
            IsMandatoryByLaw = isMandatoryByLaw,
            IsCompleted = false,
            RequiresVerification = true,
            IsVerified = false
        };

        _precautions.Add(precaution);
    }

    public void SetIndonesianCompliance(
        string k3LicenseNumber,
        string companyWorkPermitNumber,
        bool isJamsostekCompliant,
        bool hasSMK3Compliance,
        string environmentalPermitNumber = "")
    {
        K3LicenseNumber = k3LicenseNumber;
        CompanyWorkPermitNumber = companyWorkPermitNumber;
        IsJamsostekCompliant = isJamsostekCompliant;
        HasSMK3Compliance = hasSMK3Compliance;
        EnvironmentalPermitNumber = environmentalPermitNumber;
    }

    public void SetSafetyRequirements(
        bool requiresHotWork = false,
        bool requiresConfinedSpace = false,
        bool requiresElectricalIsolation = false,
        bool requiresHeightWork = false,
        bool requiresRadiationWork = false,
        bool requiresExcavation = false,
        bool requiresFireWatch = false,
        bool requiresGasMonitoring = false)
    {
        RequiresHotWorkPermit = requiresHotWork;
        RequiresConfinedSpaceEntry = requiresConfinedSpace;
        RequiresElectricalIsolation = requiresElectricalIsolation;
        RequiresHeightWork = requiresHeightWork;
        RequiresRadiationWork = requiresRadiationWork;
        RequiresExcavation = requiresExcavation;
        RequiresFireWatch = requiresFireWatch;
        RequiresGasMonitoring = requiresGasMonitoring;
        
        UpdateRiskLevel();
    }

    private void UpdateRiskLevel()
    {
        var highRiskFactors = 0;
        
        if (RequiresHotWorkPermit) highRiskFactors++;
        if (RequiresConfinedSpaceEntry) highRiskFactors++;
        if (RequiresElectricalIsolation) highRiskFactors++;
        if (RequiresHeightWork) highRiskFactors++;
        if (RequiresRadiationWork) highRiskFactors++;
        if (RequiresExcavation) highRiskFactors++;
        
        var highRiskHazards = _hazards.Count(h => h.RiskLevel == RiskLevel.High || h.RiskLevel == RiskLevel.Critical);
        
        RiskLevel = (highRiskFactors + highRiskHazards) switch
        {
            >= 3 => RiskLevel.Critical,
            >= 2 => RiskLevel.High,
            >= 1 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private bool HasAllRequiredApprovals()
    {
        var requiredApprovals = Type switch
        {
            WorkPermitType.HotWork => new[] { "SafetyOfficer", "DepartmentHead", "HotWorkSpecialist" },
            WorkPermitType.ConfinedSpace => new[] { "SafetyOfficer", "DepartmentHead", "ConfinedSpaceSpecialist" },
            WorkPermitType.ElectricalWork => new[] { "SafetyOfficer", "ElectricalSupervisor", "DepartmentHead" },
            WorkPermitType.Special => new[] { "SafetyOfficer", "DepartmentHead", "SpecialWorkSpecialist", "HSEManager" },
            _ => new[] { "SafetyOfficer", "DepartmentHead" }
        };

        return requiredApprovals.All(level => 
            _approvals.Any(a => a.ApprovalLevel == level && a.IsApproved));
    }

    private static string GeneratePermitNumber(WorkPermitType type)
    {
        var prefix = type switch
        {
            WorkPermitType.HotWork => "HW",
            WorkPermitType.ColdWork => "CW",
            WorkPermitType.ConfinedSpace => "CS",
            WorkPermitType.ElectricalWork => "EW",
            WorkPermitType.Special => "SP",
            WorkPermitType.General => "GP",
            _ => "WP"
        };

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return $"{prefix}-{year:D4}{month:D2}-{timestamp % 10000:D4}";
    }

    private static WorkPermitPriority DeterminePriority(WorkPermitType type)
    {
        return type switch
        {
            WorkPermitType.HotWork => WorkPermitPriority.High,
            WorkPermitType.ConfinedSpace => WorkPermitPriority.Critical,
            WorkPermitType.ElectricalWork => WorkPermitPriority.High,
            WorkPermitType.Special => WorkPermitPriority.Critical,
            _ => WorkPermitPriority.Medium
        };
    }
}

