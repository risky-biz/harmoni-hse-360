using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class IncidentInvolvedPerson : BaseEntity
{
    public int IncidentId { get; private set; }
    public int PersonId { get; private set; }
    public User Person { get; private set; } = null!;
    public InvolvementType InvolvementType { get; private set; }
    public string? InjuryDescription { get; private set; }

    protected IncidentInvolvedPerson() { } // For EF Core

    public IncidentInvolvedPerson(int incidentId, int personId, InvolvementType involvementType, string? injuryDescription = null)
    {
        IncidentId = incidentId;
        PersonId = personId;
        InvolvementType = involvementType;
        InjuryDescription = injuryDescription;
    }

    public void UpdateDetails(InvolvementType involvementType, string? injuryDescription)
    {
        InvolvementType = involvementType;
        InjuryDescription = injuryDescription;
    }
}