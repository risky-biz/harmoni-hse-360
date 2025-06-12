using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class UploadWorkPermitAttachmentCommand : IRequest<WorkPermitAttachmentDto>
    {
        public int WorkPermitId { get; set; }
        public IFormFile File { get; set; } = null!;
        public WorkPermitAttachmentType AttachmentType { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}