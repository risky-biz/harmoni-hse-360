using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class PPEInspection : BaseEntity, IAuditableEntity
{
    public int PPEItemId { get; private set; }
    public PPEItem PPEItem { get; private set; } = null!;
    public int InspectorId { get; private set; }
    public User Inspector { get; private set; } = null!;
    public DateTime InspectionDate { get; private set; }
    public DateTime? NextInspectionDate { get; private set; }
    public InspectionResult Result { get; private set; }
    public string? Findings { get; private set; }
    public string? CorrectiveActions { get; private set; }
    public PPECondition? RecommendedCondition { get; private set; }
    public bool RequiresMaintenance { get; private set; }
    public string? MaintenanceNotes { get; private set; }

    private readonly List<string> _photoPathsInternal = new();
    public IReadOnlyCollection<string> PhotoPaths => _photoPathsInternal.AsReadOnly();
    
    // For EF Core serialization
    public string PhotoPathsJson 
    { 
        get => string.Join(";", _photoPathsInternal);
        set => _photoPathsInternal.AddRange(value.Split(';', StringSplitOptions.RemoveEmptyEntries));
    }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPEInspection() { } // For EF Core

    public static PPEInspection Create(
        int ppeItemId,
        int inspectorId,
        DateTime inspectionDate,
        InspectionResult result,
        string createdBy,
        string? findings = null,
        string? correctiveActions = null,
        PPECondition? recommendedCondition = null,
        bool requiresMaintenance = false,
        string? maintenanceNotes = null,
        int? nextInspectionIntervalDays = null)
    {
        var inspection = new PPEInspection
        {
            PPEItemId = ppeItemId,
            InspectorId = inspectorId,
            InspectionDate = inspectionDate,
            Result = result,
            Findings = findings,
            CorrectiveActions = correctiveActions,
            RecommendedCondition = recommendedCondition,
            RequiresMaintenance = requiresMaintenance,
            MaintenanceNotes = maintenanceNotes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        if (nextInspectionIntervalDays.HasValue)
        {
            inspection.NextInspectionDate = inspectionDate.AddDays(nextInspectionIntervalDays.Value);
        }

        return inspection;
    }

    public void UpdateFindings(
        InspectionResult result,
        string? findings,
        string? correctiveActions,
        PPECondition? recommendedCondition,
        bool requiresMaintenance,
        string? maintenanceNotes,
        string modifiedBy)
    {
        Result = result;
        Findings = findings;
        CorrectiveActions = correctiveActions;
        RecommendedCondition = recommendedCondition;
        RequiresMaintenance = requiresMaintenance;
        MaintenanceNotes = maintenanceNotes;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void AddPhoto(string photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
            throw new ArgumentException("Photo path cannot be empty", nameof(photoPath));

        _photoPathsInternal.Add(photoPath);
    }

    public void RemovePhoto(string photoPath)
    {
        _photoPathsInternal.Remove(photoPath);
    }

    public void SetNextInspectionDate(DateTime nextInspectionDate)
    {
        if (nextInspectionDate <= InspectionDate)
            throw new ArgumentException("Next inspection date must be after current inspection date");

        NextInspectionDate = nextInspectionDate;
    }

    // Computed Properties
    public bool IsPassed => Result == InspectionResult.Passed;
    public bool IsFailed => Result == InspectionResult.Failed;
    public bool HasObservations => Result == InspectionResult.PassedWithObservations;
    public bool IsOverdue => NextInspectionDate.HasValue && DateTime.UtcNow > NextInspectionDate.Value;
    public int? DaysUntilNextInspection => NextInspectionDate.HasValue 
        ? (int)(NextInspectionDate.Value - DateTime.UtcNow).TotalDays 
        : null;
}

public enum InspectionResult
{
    Passed = 1,
    PassedWithObservations = 2,
    Failed = 3,
    RequiresMaintenance = 4,
    RequiresReplacement = 5
}