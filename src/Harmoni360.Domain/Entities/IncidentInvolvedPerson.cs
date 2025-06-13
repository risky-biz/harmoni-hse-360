using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class IncidentInvolvedPerson : BaseEntity
{
    public int IncidentId { get; private set; }
    public int? PersonId { get; private set; }
    public User? Person { get; private set; }
    public InvolvementType InvolvementType { get; private set; }
    public string? InjuryDescription { get; private set; }
    
    // Fields for manual entry (when PersonId is null)
    public string? ManualPersonName { get; private set; }
    public string? ManualPersonEmail { get; private set; }

    protected IncidentInvolvedPerson() { } // For EF Core

    public IncidentInvolvedPerson(int incidentId, int? personId, InvolvementType involvementType, string? injuryDescription = null, string? manualPersonName = null, string? manualPersonEmail = null)
    {
        IncidentId = incidentId;
        PersonId = personId;
        InvolvementType = involvementType;
        InjuryDescription = injuryDescription;
        ManualPersonName = manualPersonName;
        ManualPersonEmail = manualPersonEmail;
    }

    public void UpdateDetails(InvolvementType involvementType, string? injuryDescription)
    {
        InvolvementType = involvementType;
        InjuryDescription = injuryDescription;
    }
}