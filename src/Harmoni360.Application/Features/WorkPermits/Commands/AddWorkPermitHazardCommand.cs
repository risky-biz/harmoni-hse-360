using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class AddWorkPermitHazardCommand : IRequest<WorkPermitHazardDto>
    {
        public int WorkPermitId { get; set; }
        public string HazardDescription { get; set; } = string.Empty;
        public Harmoni360.Domain.Enums.HazardCategory Category { get; set; }
        public int Likelihood { get; set; } // 1-5 scale
        public int Severity { get; set; } // 1-5 scale
        public string ControlMeasures { get; set; } = string.Empty;
        public string ResponsiblePerson { get; set; } = string.Empty;
    }
}