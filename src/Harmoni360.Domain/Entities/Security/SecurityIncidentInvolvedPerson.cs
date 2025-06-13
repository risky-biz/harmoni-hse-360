using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents a person involved in a security incident
/// </summary>
public class SecurityIncidentInvolvedPerson : BaseEntity
{
    public int SecurityIncidentId { get; private set; }
    public SecurityIncident SecurityIncident { get; private set; } = null!;
    
    public int PersonId { get; private set; }
    public User Person { get; private set; } = null!;
    
    public string Involvement { get; private set; } = string.Empty;
    public bool IsWitness { get; private set; }
    public bool IsVictim { get; private set; }
    public bool IsSuspect { get; private set; }
    public bool IsReporter { get; private set; }
    
    // Additional Details
    public string? Statement { get; private set; }
    public DateTime? StatementDate { get; private set; }
    public bool StatementTaken { get; private set; }
    
    // Contact Information
    public string? ContactMethod { get; private set; }
    public string? AdditionalNotes { get; private set; }
    
    public DateTime InvolvedAt { get; private set; }
    public string AddedBy { get; private set; } = string.Empty;
    
    // Constructor for EF Core
    protected SecurityIncidentInvolvedPerson() { }
    
    // Factory method
    public static SecurityIncidentInvolvedPerson Create(
        int securityIncidentId,
        int personId,
        string involvement,
        bool isWitness = false,
        bool isVictim = false,
        bool isSuspect = false,
        bool isReporter = false)
    {
        if (string.IsNullOrWhiteSpace(involvement))
            throw new ArgumentException("Involvement description is required", nameof(involvement));
        
        return new SecurityIncidentInvolvedPerson
        {
            SecurityIncidentId = securityIncidentId,
            PersonId = personId,
            Involvement = involvement,
            IsWitness = isWitness,
            IsVictim = isVictim,
            IsSuspect = isSuspect,
            IsReporter = isReporter,
            StatementTaken = false,
            InvolvedAt = DateTime.UtcNow,
            AddedBy = string.Empty // Will be set by the calling context
        };
    }
    
    // Business Methods
    public void RecordStatement(string statement, string recordedBy)
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException("Statement cannot be empty", nameof(statement));
        
        Statement = statement;
        StatementDate = DateTime.UtcNow;
        StatementTaken = true;
        AddedBy = recordedBy;
    }
    
    public void UpdateContactMethod(string contactMethod)
    {
        ContactMethod = contactMethod;
    }
    
    public void AddNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(AdditionalNotes))
        {
            AdditionalNotes = notes;
        }
        else
        {
            AdditionalNotes += $"\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {notes}";
        }
    }
    
    public void UpdateInvolvementType(
        bool isWitness = false,
        bool isVictim = false,
        bool isSuspect = false,
        bool isReporter = false)
    {
        IsWitness = isWitness;
        IsVictim = isVictim;
        IsSuspect = isSuspect;
        IsReporter = isReporter;
    }
}