using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class RemoveWorkPermitHazardCommand : IRequest
    {
        public int WorkPermitId { get; set; }
        public int HazardId { get; set; }
    }
}