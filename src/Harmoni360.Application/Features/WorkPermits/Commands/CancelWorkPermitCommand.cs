using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class CancelWorkPermitCommand : IRequest<WorkPermitDto>
    {
        public int Id { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }
}