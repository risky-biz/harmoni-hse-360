using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class DeleteWorkPermitAttachmentCommand : IRequest
    {
        public int WorkPermitId { get; set; }
        public int AttachmentId { get; set; }
    }
}