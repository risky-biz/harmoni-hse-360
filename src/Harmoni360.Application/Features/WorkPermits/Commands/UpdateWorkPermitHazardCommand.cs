using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class UpdateWorkPermitHazardCommand : IRequest<WorkPermitHazardDto>
    {
        public int WorkPermitId { get; set; }
        public int HazardId { get; set; }
        public string HazardDescription { get; set; } = string.Empty;
        public HazardCategory Category { get; set; }
        public int Likelihood { get; set; }
        public int Severity { get; set; }
        public string ControlMeasures { get; set; } = string.Empty;
        public string ResponsiblePerson { get; set; } = string.Empty;
        public bool IsControlImplemented { get; set; }
        public string ImplementationNotes { get; set; } = string.Empty;
    }
}