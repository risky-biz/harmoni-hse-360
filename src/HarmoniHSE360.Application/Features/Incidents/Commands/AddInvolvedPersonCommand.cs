using HarmoniHSE360.Domain.Entities;
using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class AddInvolvedPersonCommand : IRequest<Unit>
{
    public int IncidentId { get; set; }
    public int PersonId { get; set; }
    public InvolvementType InvolvementType { get; set; }
    public string? InjuryDescription { get; set; }
}