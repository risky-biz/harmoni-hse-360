using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class SubmitWorkPermitCommand : IRequest<WorkPermitDto>
    {
        public int Id { get; set; }
    }
}