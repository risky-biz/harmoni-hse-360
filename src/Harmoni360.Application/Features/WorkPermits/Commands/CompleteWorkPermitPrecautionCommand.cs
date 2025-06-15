using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class CompleteWorkPermitPrecautionCommand : IRequest<WorkPermitPrecautionDto>
    {
        public int WorkPermitId { get; set; }
        public int PrecautionId { get; set; }
        public string CompletionNotes { get; set; } = string.Empty;
        public bool RequiresVerification { get; set; }
    }
}