using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermits.Queries
{
    public class GetWorkPermitByIdQueryHandler : IRequestHandler<GetWorkPermitByIdQuery, WorkPermitDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetWorkPermitByIdQueryHandler> _logger;

        public GetWorkPermitByIdQueryHandler(IApplicationDbContext context, ILogger<GetWorkPermitByIdQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WorkPermitDto?> Handle(GetWorkPermitByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving work permit with ID: {Id}", request.Id);

            var workPermit = await _context.WorkPermits
                .Include(wp => wp.Attachments)
                .Include(wp => wp.Approvals)
                .Include(wp => wp.Hazards)
                .Include(wp => wp.Precautions)
                .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

            if (workPermit == null)
            {
                _logger.LogWarning("Work permit with ID {Id} not found", request.Id);
                return null;
            }

            var dto = new WorkPermitDto
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
                UpdatedAt = workPermit.LastModifiedAt,
                Attachments = workPermit.Attachments.Select(a => new WorkPermitAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    AttachmentType = a.AttachmentType.ToString(),
                    Description = a.Description,
                    UploadedBy = a.UploadedBy,
                    UploadedAt = a.UploadedAt
                }).ToList(),
                Approvals = workPermit.Approvals.Select(a => new WorkPermitApprovalDto
                {
                    Id = a.Id,
                    ApprovedById = a.ApprovedById,
                    ApprovedByName = a.ApprovedByName,
                    ApprovalLevel = a.ApprovalLevel,
                    ApprovedAt = a.ApprovedAt,
                    IsApproved = a.IsApproved,
                    Comments = a.Comments,
                    ApprovalOrder = a.ApprovalOrder,
                    K3CertificateNumber = a.K3CertificateNumber,
                    AuthorityLevel = a.AuthorityLevel
                }).ToList(),
                Hazards = workPermit.Hazards.Select(h => new WorkPermitHazardDto
                {
                    Id = h.Id,
                    HazardDescription = h.HazardDescription,
                    Category = h.Category?.Name ?? GetHazardCategoryName(h.CategoryId),
                    RiskLevel = h.RiskLevel.ToString(),
                    Likelihood = h.Likelihood,
                    Severity = h.Severity,
                    ControlMeasures = h.ControlMeasures,
                    ResidualRiskLevel = h.ResidualRiskLevel.ToString(),
                    ResponsiblePerson = h.ResponsiblePerson,
                    IsControlImplemented = h.IsControlImplemented,
                    ControlImplementedDate = h.ControlImplementedDate,
                    ImplementationNotes = h.ImplementationNotes
                }).ToList(),
                Precautions = workPermit.Precautions.Select(p => new WorkPermitPrecautionDto
                {
                    Id = p.Id,
                    PrecautionDescription = p.PrecautionDescription,
                    Category = p.Category.ToString(),
                    IsRequired = p.IsRequired,
                    IsCompleted = p.IsCompleted,
                    CompletedAt = p.CompletedAt,
                    CompletedBy = p.CompletedBy,
                    CompletionNotes = p.CompletionNotes,
                    Priority = p.Priority,
                    ResponsiblePerson = p.ResponsiblePerson,
                    VerificationMethod = p.VerificationMethod,
                    RequiresVerification = p.RequiresVerification,
                    IsVerified = p.IsVerified,
                    VerifiedAt = p.VerifiedAt,
                    VerifiedBy = p.VerifiedBy,
                    IsK3Requirement = p.IsK3Requirement,
                    K3StandardReference = p.K3StandardReference,
                    IsMandatoryByLaw = p.IsMandatoryByLaw
                }).ToList()
            };

            // Computed properties are calculated in the DTO itself

            return dto;
        }

        private static string GetHazardCategoryName(int? categoryId)
        {
            // Map categoryId back to enum name for display
            // This assumes the enum values correspond to IDs (enum value + 1)
            if (!categoryId.HasValue) return "Unknown";
            
            var enumValue = categoryId.Value - 1; // Convert back from ID to enum value
            return enumValue switch
            {
                0 => "Physical",
                1 => "Chemical", 
                2 => "Biological",
                3 => "Ergonomic",
                4 => "Fire",
                5 => "Electrical",
                6 => "Mechanical",
                7 => "Environmental",
                8 => "Radiological",
                9 => "Behavioral",
                _ => "Unknown"
            };
        }
    }
}