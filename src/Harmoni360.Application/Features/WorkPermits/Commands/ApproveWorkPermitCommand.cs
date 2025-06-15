using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class ApproveWorkPermitCommand : IRequest<WorkPermitDto>
    {
        public int Id { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string K3CertificateNumber { get; set; } = string.Empty;
        public string AuthorityLevel { get; set; } = string.Empty;
    }
}