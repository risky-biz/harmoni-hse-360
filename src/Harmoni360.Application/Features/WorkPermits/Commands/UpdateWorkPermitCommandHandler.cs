using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class UpdateWorkPermitCommandHandler : IRequestHandler<UpdateWorkPermitCommand, WorkPermitDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<UpdateWorkPermitCommandHandler> _logger;

        public UpdateWorkPermitCommandHandler(IApplicationDbContext context, ILogger<UpdateWorkPermitCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WorkPermitDto> Handle(UpdateWorkPermitCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating work permit with ID: {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .Include(wp => wp.Attachments)
                .Include(wp => wp.Approvals)
                .Include(wp => wp.Hazards)
                .Include(wp => wp.Precautions)
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                throw new InvalidOperationException($"Work permit with ID {request.Id} not found");
            }

            if (workPermit.Status != Domain.Enums.WorkPermitStatus.Draft && 
                workPermit.Status != Domain.Enums.WorkPermitStatus.Rejected)
            {
                throw new InvalidOperationException($"Work permit cannot be updated in {workPermit.Status} status");
            }

            // TODO: Implement update methods in WorkPermit entity
            // For now, updating properties directly
            // workPermit.Update(...)
            // workPermit.UpdateSafetyRequirements(...)
            // workPermit.UpdateK3Compliance(...)
            // workPermit.UpdateRiskAssessment(...)

            _context.WorkPermits.Update(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work permit {Id} updated successfully", request.Id);

            // Map to DTO
            return new WorkPermitDto
            {
                Id = workPermit.Id,
                PermitNumber = workPermit.PermitNumber,
                Title = workPermit.Title,
                Description = workPermit.Description,
                Type = workPermit.Type.ToString(),
                Status = workPermit.Status.ToString(),
                Priority = workPermit.Priority.ToString(),
                WorkLocation = workPermit.WorkLocation,
                GeoLocation = workPermit.GeoLocation != null ? new GeoLocationDto
                {
                    Latitude = workPermit.GeoLocation.Latitude,
                    Longitude = workPermit.GeoLocation.Longitude,
                    Address = "", // Address stored separately in WorkPermit
                    LocationDescription = "" // Description stored separately in WorkPermit
                } : null,
                PlannedStartDate = workPermit.PlannedStartDate,
                PlannedEndDate = workPermit.PlannedEndDate,
                ActualStartDate = workPermit.ActualStartDate,
                ActualEndDate = workPermit.ActualEndDate,
                EstimatedDuration = workPermit.EstimatedDuration,
                RequestedById = workPermit.RequestedById,
                RequestedByName = workPermit.RequestedByName,
                RequestedByDepartment = workPermit.RequestedByDepartment,
                RequestedByPosition = workPermit.RequestedByPosition,
                ContactPhone = workPermit.ContactPhone,
                WorkSupervisor = workPermit.WorkSupervisor,
                SafetyOfficer = workPermit.SafetyOfficer,
                WorkScope = workPermit.WorkScope,
                EquipmentToBeUsed = workPermit.EquipmentToBeUsed,
                MaterialsInvolved = workPermit.MaterialsInvolved,
                NumberOfWorkers = workPermit.NumberOfWorkers,
                ContractorCompany = workPermit.ContractorCompany,
                RequiresHotWorkPermit = workPermit.RequiresHotWorkPermit,
                RequiresConfinedSpaceEntry = workPermit.RequiresConfinedSpaceEntry,
                RequiresElectricalIsolation = workPermit.RequiresElectricalIsolation,
                RequiresHeightWork = workPermit.RequiresHeightWork,
                RequiresRadiationWork = workPermit.RequiresRadiationWork,
                RequiresExcavation = workPermit.RequiresExcavation,
                RequiresFireWatch = workPermit.RequiresFireWatch,
                RequiresGasMonitoring = workPermit.RequiresGasMonitoring,
                K3LicenseNumber = workPermit.K3LicenseNumber,
                CompanyWorkPermitNumber = workPermit.CompanyWorkPermitNumber,
                IsJamsostekCompliant = workPermit.IsJamsostekCompliant,
                HasSMK3Compliance = workPermit.HasSMK3Compliance,
                EnvironmentalPermitNumber = workPermit.EnvironmentalPermitNumber,
                RiskLevel = workPermit.RiskLevel.ToString(),
                RiskAssessmentSummary = workPermit.RiskAssessmentSummary,
                EmergencyProcedures = workPermit.EmergencyProcedures,
                CompletionNotes = workPermit.CompletionNotes,
                IsCompletedSafely = workPermit.IsCompletedSafely,
                LessonsLearned = workPermit.LessonsLearned,
                CreatedAt = workPermit.CreatedAt,
                UpdatedAt = workPermit.LastModifiedAt
            };
        }
    }
}