using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents a response action taken for a security incident
/// </summary>
public class SecurityIncidentResponse : BaseEntity
{
    public int SecurityIncidentId { get; private set; }
    public SecurityIncident SecurityIncident { get; private set; } = null!;
    
    public SecurityResponseType ResponseType { get; private set; }
    public string ActionTaken { get; private set; } = string.Empty;
    public DateTime ActionDateTime { get; private set; }
    
    // Response Details
    public bool WasSuccessful { get; private set; }
    public bool FollowUpRequired { get; private set; }
    public string? FollowUpDetails { get; private set; }
    public DateTime? FollowUpDueDate { get; private set; }
    
    // Effort and Cost Tracking
    public int? EffortHours { get; private set; }
    public decimal? Cost { get; private set; }
    
    // Tools and Resources Used
    public string? ToolsUsed { get; private set; }
    public string? ResourcesUsed { get; private set; }
    
    // Navigation Properties
    public int ResponderId { get; private set; }
    public User Responder { get; private set; } = null!;
    
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    
    // Constructor for EF Core
    protected SecurityIncidentResponse() { }
    
    // Factory method
    public static SecurityIncidentResponse Create(
        int securityIncidentId,
        SecurityResponseType responseType,
        string actionTaken,
        DateTime actionDateTime,
        int responderId,
        string createdBy = "")
    {
        if (string.IsNullOrWhiteSpace(actionTaken))
            throw new ArgumentException("Action taken is required", nameof(actionTaken));
        
        return new SecurityIncidentResponse
        {
            SecurityIncidentId = securityIncidentId,
            ResponseType = responseType,
            ActionTaken = actionTaken,
            ActionDateTime = actionDateTime,
            ResponderId = responderId,
            WasSuccessful = true, // Default to successful
            FollowUpRequired = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
    
    // Business Methods
    public void MarkAsUnsuccessful(string reason)
    {
        WasSuccessful = false;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            ActionTaken += $" [UNSUCCESSFUL: {reason}]";
        }
    }
    
    public void RequireFollowUp(string details, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(details))
            throw new ArgumentException("Follow-up details are required", nameof(details));
        
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Follow-up due date must be in the future", nameof(dueDate));
        
        FollowUpRequired = true;
        FollowUpDetails = details;
        FollowUpDueDate = dueDate;
    }
    
    public void CompleteFollowUp()
    {
        FollowUpRequired = false;
        if (!string.IsNullOrWhiteSpace(FollowUpDetails))
        {
            FollowUpDetails += $" [COMPLETED: {DateTime.UtcNow:yyyy-MM-dd HH:mm}]";
        }
    }
    
    public void UpdateEffortTracking(int hours, decimal? cost = null)
    {
        if (hours < 0)
            throw new ArgumentException("Effort hours cannot be negative", nameof(hours));
        
        if (cost.HasValue && cost.Value < 0)
            throw new ArgumentException("Cost cannot be negative", nameof(cost));
        
        EffortHours = hours;
        Cost = cost;
    }
    
    public void UpdateResourceUsage(string? tools, string? resources)
    {
        ToolsUsed = tools;
        ResourcesUsed = resources;
    }
    
    // Calculated Properties
    public bool IsOverdue => FollowUpRequired && 
                            FollowUpDueDate.HasValue && 
                            FollowUpDueDate.Value < DateTime.UtcNow;
    
    public int DaysUntilFollowUp => FollowUpDueDate.HasValue 
        ? Math.Max(0, (FollowUpDueDate.Value.Date - DateTime.UtcNow.Date).Days)
        : 0;
    
    public string ResponseTypeDescription => ResponseType switch
    {
        SecurityResponseType.Initial => "Initial Response",
        SecurityResponseType.Containment => "Containment Action",
        SecurityResponseType.Eradication => "Eradication Action",
        SecurityResponseType.Recovery => "Recovery Action",
        SecurityResponseType.LessonsLearned => "Lessons Learned",
        _ => "Unknown Response Type"
    };
}