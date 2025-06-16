using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Application.Features.WorkPermits.Commands;

public class CreateWorkPermitCommandHandler : IRequestHandler<CreateWorkPermitCommand, WorkPermitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateWorkPermitCommandHandler> _logger;

    public CreateWorkPermitCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateWorkPermitCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<WorkPermitDto> Handle(CreateWorkPermitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Create GeoLocation if coordinates provided
            GeoLocation? geoLocation = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                geoLocation = GeoLocation.Create(
                    request.Latitude.Value,
                    request.Longitude.Value);
            }

            // Create the work permit
            var workPermit = WorkPermit.Create(
                request.Title,
                request.Description,
                request.Type,
                request.WorkLocation,
                request.PlannedStartDate,
                request.PlannedEndDate,
                currentUser.Id,
                currentUser.Name,
                currentUser.Department,
                currentUser.Position,
                request.ContactPhone,
                request.WorkScope,
                request.NumberOfWorkers,
                geoLocation);

            // Set additional work details
            workPermit.GetType().GetProperty(nameof(WorkPermit.WorkSupervisor))?.SetValue(workPermit, request.WorkSupervisor);
            workPermit.GetType().GetProperty(nameof(WorkPermit.SafetyOfficer))?.SetValue(workPermit, request.SafetyOfficer);
            workPermit.GetType().GetProperty(nameof(WorkPermit.ContractorCompany))?.SetValue(workPermit, request.ContractorCompany);
            workPermit.GetType().GetProperty(nameof(WorkPermit.EquipmentToBeUsed))?.SetValue(workPermit, request.EquipmentToBeUsed);
            workPermit.GetType().GetProperty(nameof(WorkPermit.MaterialsInvolved))?.SetValue(workPermit, request.MaterialsInvolved);

            // Set safety requirements
            workPermit.SetSafetyRequirements(
                request.RequiresHotWorkPermit,
                request.RequiresConfinedSpaceEntry,
                request.RequiresElectricalIsolation,
                request.RequiresHeightWork,
                request.RequiresRadiationWork,
                request.RequiresExcavation,
                request.RequiresFireWatch,
                request.RequiresGasMonitoring);

            // Set Indonesian compliance
            if (!string.IsNullOrEmpty(request.K3LicenseNumber) || 
                !string.IsNullOrEmpty(request.CompanyWorkPermitNumber))
            {
                workPermit.SetIndonesianCompliance(
                    request.K3LicenseNumber,
                    request.CompanyWorkPermitNumber,
                    request.IsJamsostekCompliant,
                    request.HasSMK3Compliance,
                    request.EnvironmentalPermitNumber);
            }

            // Set risk assessment details using reflection (since properties have private setters)
            if (!string.IsNullOrEmpty(request.RiskAssessmentSummary))
            {
                workPermit.GetType().GetProperty(nameof(WorkPermit.RiskAssessmentSummary))?.SetValue(workPermit, request.RiskAssessmentSummary);
            }
            if (!string.IsNullOrEmpty(request.EmergencyProcedures))
            {
                workPermit.GetType().GetProperty(nameof(WorkPermit.EmergencyProcedures))?.SetValue(workPermit, request.EmergencyProcedures);
            }

            // Set audit fields
            workPermit.CreatedBy = currentUser.Name;
            workPermit.CreatedAt = DateTime.UtcNow;

            _context.WorkPermits.Add(workPermit);
            await _context.SaveChangesAsync(cancellationToken);

            // Add hazards if provided
            foreach (var hazardRequest in request.Hazards)
            {
                var riskLevel = CalculateRiskLevel(hazardRequest.Likelihood, hazardRequest.Severity);
                // Convert enum to categoryId (enum values correspond to entity IDs)
                var categoryId = (int)hazardRequest.Category + 1; // Assuming IDs start from 1
                workPermit.AddHazard(
                    hazardRequest.HazardDescription,
                    categoryId,
                    riskLevel,
                    hazardRequest.ControlMeasures,
                    hazardRequest.ResponsiblePerson);
            }

            // Add precautions if provided
            foreach (var precautionRequest in request.Precautions)
            {
                workPermit.AddPrecaution(
                    precautionRequest.PrecautionDescription,
                    precautionRequest.Category,
                    precautionRequest.IsRequired,
                    precautionRequest.Priority,
                    precautionRequest.ResponsiblePerson,
                    precautionRequest.VerificationMethod,
                    precautionRequest.IsK3Requirement,
                    precautionRequest.K3StandardReference,
                    precautionRequest.IsMandatoryByLaw);
            }

            // Save hazards and precautions
            if (request.Hazards.Any() || request.Precautions.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Work permit created successfully. ID: {WorkPermitId}, Number: {PermitNumber}", 
                workPermit.Id, workPermit.PermitNumber);

            // Return DTO
            return await MapToDto(workPermit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work permit for user {UserId}", _currentUserService.UserId);
            throw;
        }
    }

    private async Task<WorkPermitDto> MapToDto(WorkPermit workPermit)
    {
        // Load related data
        var dbContext = (DbContext)_context;
        await dbContext.Entry(workPermit)
            .Collection(wp => wp.Attachments)
            .LoadAsync();
        await dbContext.Entry(workPermit)
            .Collection(wp => wp.Approvals)
            .LoadAsync();
        await dbContext.Entry(workPermit)
            .Collection(wp => wp.Hazards)
            .LoadAsync();
        await dbContext.Entry(workPermit)
            .Collection(wp => wp.Precautions)
            .LoadAsync();

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
            CreatedBy = workPermit.CreatedBy,
            UpdatedAt = workPermit.LastModifiedAt,
            UpdatedBy = workPermit.LastModifiedBy,
            Attachments = workPermit.Attachments.Select(a => new WorkPermitAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                OriginalFileName = a.OriginalFileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt,
                AttachmentType = a.AttachmentType.ToString(),
                Description = a.Description
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
            }).ToList(),
            RequiredApprovalLevels = workPermit.GetRequiredApprovalLevels(),
            ReceivedApprovalLevels = workPermit.GetReceivedApprovalLevels(),
            MissingApprovalLevels = workPermit.GetMissingApprovalLevels()
        };
        
        // Calculate approval progress percentage
        dto.ApprovalProgress = dto.RequiredApprovalLevels.Length > 0 
            ? (int)Math.Round((double)dto.ReceivedApprovalLevels.Length / dto.RequiredApprovalLevels.Length * 100)
            : 0;
            
        return dto;
    }

    private static RiskLevel CalculateRiskLevel(int likelihood, int severity)
    {
        var riskScore = likelihood * severity;
        
        return riskScore switch
        {
            >= 20 => RiskLevel.Critical,
            >= 15 => RiskLevel.High,
            >= 10 => RiskLevel.Medium,
            >= 5 => RiskLevel.Low,
            _ => RiskLevel.Low
        };
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