using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class AddWorkPermitHazardCommandHandler : IRequestHandler<AddWorkPermitHazardCommand, WorkPermitHazardDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<AddWorkPermitHazardCommandHandler> _logger;

        public AddWorkPermitHazardCommandHandler(IApplicationDbContext context, ILogger<AddWorkPermitHazardCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WorkPermitHazardDto> Handle(AddWorkPermitHazardCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding hazard to work permit {WorkPermitId}", request.WorkPermitId);

            var workPermit = await _context.WorkPermits
                .FirstOrDefaultAsync(wp => wp.Id == request.WorkPermitId, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.WorkPermitId} not found");
            }

            // Calculate risk level based on likelihood and severity
            var riskScore = request.Likelihood * request.Severity;
            var riskLevel = CalculateRiskLevel(riskScore);

            var hazard = WorkPermitHazard.Create(
                workPermitId: request.WorkPermitId,
                hazardDescription: request.HazardDescription,
                categoryId: null, // TODO: Map category enum to category ID
                riskLevel: riskLevel,
                likelihood: request.Likelihood,
                severity: request.Severity,
                controlMeasures: request.ControlMeasures,
                residualRiskLevel: riskLevel, // Will be updated after controls are implemented
                responsiblePerson: request.ResponsiblePerson
            );

            _context.WorkPermitHazards.Add(hazard);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Hazard added successfully to work permit {WorkPermitId}", request.WorkPermitId);

            return new WorkPermitHazardDto
            {
                Id = hazard.Id,
                HazardDescription = hazard.HazardDescription,
                Category = hazard.Category?.Name ?? "Unknown",
                RiskLevel = hazard.RiskLevel.ToString(),
                Likelihood = hazard.Likelihood,
                Severity = hazard.Severity,
                ControlMeasures = hazard.ControlMeasures,
                ResidualRiskLevel = hazard.ResidualRiskLevel.ToString(),
                ResponsiblePerson = hazard.ResponsiblePerson,
                IsControlImplemented = hazard.IsControlImplemented,
                ControlImplementedDate = hazard.ControlImplementedDate,
                ImplementationNotes = hazard.ImplementationNotes
            };
        }

        private Domain.Enums.RiskLevel CalculateRiskLevel(int riskScore)
        {
            return riskScore switch
            {
                <= 6 => Domain.Enums.RiskLevel.Low,
                <= 12 => Domain.Enums.RiskLevel.Medium,
                <= 20 => Domain.Enums.RiskLevel.High,
                _ => Domain.Enums.RiskLevel.Critical
            };
        }
    }
}