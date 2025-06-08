using MediatR;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class DeleteIncidentCommand : IRequest<bool>
{
    public int Id { get; set; }
}