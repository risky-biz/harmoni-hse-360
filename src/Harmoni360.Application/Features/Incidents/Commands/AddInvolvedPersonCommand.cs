using Harmoni360.Domain.Entities;
using MediatR;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class AddInvolvedPersonCommand : IRequest<Unit>
{
    public int IncidentId { get; set; }
    public int PersonId { get; set; }
    public InvolvementType InvolvementType { get; set; }
    public string? InjuryDescription { get; set; }
    public string? ManualPersonName { get; set; }
    public string? ManualPersonEmail { get; set; }
}