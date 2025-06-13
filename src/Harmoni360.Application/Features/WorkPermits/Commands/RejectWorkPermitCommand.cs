using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class RejectWorkPermitCommand : IRequest<WorkPermitDto>
    {
        public int Id { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}