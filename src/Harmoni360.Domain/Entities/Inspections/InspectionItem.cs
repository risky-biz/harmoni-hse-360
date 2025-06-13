using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Inspections;

public class InspectionItem : BaseEntity
{
    public int InspectionId { get; private set; }
    public int? ChecklistItemId { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public InspectionItemType Type { get; private set; }
    public bool IsRequired { get; private set; }
    public string? Response { get; private set; }
    public InspectionItemStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }
    public string? ExpectedValue { get; private set; }
    public string? Unit { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public string? Options { get; private set; } // JSON for multiple choice options

    // Navigation Properties
    public virtual Inspection Inspection { get; private set; } = null!;

    private InspectionItem() { }

    public static InspectionItem Create(
        int inspectionId,
        string question,
        InspectionItemType type,
        bool isRequired = false,
        string? description = null,
        int sortOrder = 0,
        int? checklistItemId = null)
    {
        return new InspectionItem
        {
            InspectionId = inspectionId,
            Question = question,
            Description = description,
            Type = type,
            IsRequired = isRequired,
            Status = InspectionItemStatus.NotStarted,
            SortOrder = sortOrder,
            ChecklistItemId = checklistItemId
        };
    }

    public void UpdateResponse(string response, string? notes = null)
    {
        Response = response;
        Notes = notes;
        Status = string.IsNullOrEmpty(response) ? InspectionItemStatus.NotStarted : InspectionItemStatus.Completed;

        // Check compliance for Yes/No questions
        if (Type == InspectionItemType.YesNo && !string.IsNullOrEmpty(response))
        {
            var isCompliant = response.Equals("Yes", StringComparison.OrdinalIgnoreCase) || 
                            response.Equals("True", StringComparison.OrdinalIgnoreCase);
            
            if (!isCompliant && IsRequired)
            {
                Status = InspectionItemStatus.NonCompliant;
            }
        }

        // Check numeric ranges
        if (Type == InspectionItemType.Number && !string.IsNullOrEmpty(response))
        {
            if (decimal.TryParse(response, out var numericValue))
            {
                if ((MinValue.HasValue && numericValue < MinValue.Value) ||
                    (MaxValue.HasValue && numericValue > MaxValue.Value))
                {
                    Status = InspectionItemStatus.NonCompliant;
                }
            }
        }
    }

    public void MarkAsNotApplicable(string reason)
    {
        Status = InspectionItemStatus.NotApplicable;
        Notes = reason;
        Response = "N/A";
    }

    public void SetValidationRules(decimal? minValue = null, decimal? maxValue = null, string? expectedValue = null, string? unit = null)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        ExpectedValue = expectedValue;
        Unit = unit;
    }

    public void SetMultipleChoiceOptions(List<string> options)
    {
        if (Type != InspectionItemType.MultipleChoice)
            throw new InvalidOperationException("Options can only be set for multiple choice items");

        Options = System.Text.Json.JsonSerializer.Serialize(options);
    }

    public List<string> GetMultipleChoiceOptions()
    {
        if (string.IsNullOrEmpty(Options))
            return new List<string>();

        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Options) ?? new List<string>();
    }

    public bool IsCompliant => Status != InspectionItemStatus.NonCompliant;
    public bool IsCompleted => Status == InspectionItemStatus.Completed || Status == InspectionItemStatus.NotApplicable;
    public bool HasResponse => !string.IsNullOrEmpty(Response);
}