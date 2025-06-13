using Harmoni360.Domain.Entities;
using MediatR;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class UpdateInvolvedPersonCommand : IRequest<Unit>
{
    public int IncidentId { get; set; }
    public int PersonId { get; set; }
    public InvolvementType InvolvementType { get; set; }
    public string? InjuryDescription { get; set; }
}