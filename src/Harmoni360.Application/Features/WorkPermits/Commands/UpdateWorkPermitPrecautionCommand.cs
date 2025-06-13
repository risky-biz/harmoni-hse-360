using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class UpdateWorkPermitPrecautionCommand : IRequest<WorkPermitPrecautionDto>
    {
        public int WorkPermitId { get; set; }
        public int PrecautionId { get; set; }
        public string PrecautionDescription { get; set; } = string.Empty;
        public PrecautionCategory Category { get; set; }
        public bool IsRequired { get; set; }
        public int Priority { get; set; }
        public string ResponsiblePerson { get; set; } = string.Empty;
        public string VerificationMethod { get; set; } = string.Empty;
        public bool RequiresVerification { get; set; }
        public bool IsK3Requirement { get; set; }
        public string K3StandardReference { get; set; } = string.Empty;
        public bool IsMandatoryByLaw { get; set; }
    }
}