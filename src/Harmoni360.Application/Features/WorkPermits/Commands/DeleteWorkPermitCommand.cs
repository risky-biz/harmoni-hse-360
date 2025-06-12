using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class DeleteWorkPermitCommand : IRequest
    {
        public int Id { get; set; }
    }
}