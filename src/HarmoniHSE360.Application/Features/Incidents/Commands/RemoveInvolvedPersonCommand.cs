using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class RemoveInvolvedPersonCommand : IRequest<Unit>
{
    public int IncidentId { get; set; }
    public int PersonId { get; set; }
}