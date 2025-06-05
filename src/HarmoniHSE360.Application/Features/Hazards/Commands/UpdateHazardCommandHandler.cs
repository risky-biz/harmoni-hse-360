using MediatR;
using Microsoft.EntityFrameworkCore;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Hazards.DTOs;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.ValueObjects;
using HarmoniHSE360.Domain.Events;

namespace HarmoniHSE360.Application.Features.Hazards.Commands;

public class UpdateHazardCommandHandler : IRequestHandler<UpdateHazardCommand, HazardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UpdateHazardCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<HazardDto> Handle(UpdateHazardCommand request, CancellationToken cancellationToken)
    {
        // Get the existing hazard with related data
        var hazard = await _context.Hazards
            .Include(h => h.Reporter)
            .Include(h => h.Attachments)
            .Include(h => h.CurrentRiskAssessment)
            .Include(h => h.RiskAssessments)
            .Include(h => h.MitigationActions)
            .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);

        if (hazard == null)
        {
            throw new ArgumentException($"Hazard with ID {request.Id} not found.");
        }

        // Store original values for audit trail
        var originalStatus = hazard.Status;
        var originalSeverity = hazard.Severity;

        // Use enum values directly from request

        // Update hazard properties
        hazard.UpdateDetails(request.Title, request.Description, _currentUserService.Name);

        // Update GeoLocation if coordinates provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            hazard.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);
        }

        // Update expected resolution date if provided
        if (request.ExpectedResolutionDate.HasValue)
        {
            hazard.SetExpectedResolutionDate(request.ExpectedResolutionDate.Value);
        }

        // Update status if changed
        if (originalStatus != request.Status)
        {
            hazard.UpdateStatus(request.Status, _currentUserService.Name);
        }

        // Update severity if changed
        if (originalSeverity != request.Severity)
        {
            hazard.UpdateSeverity(request.Severity, _currentUserService.Name);
        }

        // Handle attachment removals
        if (request.AttachmentsToRemove?.Any() == true)
        {
            var attachmentsToRemove = hazard.Attachments
                .Where(a => request.AttachmentsToRemove.Contains(a.Id))
                .ToList();

            foreach (var attachment in attachmentsToRemove)
            {
                // Delete file from storage
                await _fileStorageService.DeleteAsync(attachment.FilePath);
                _context.HazardAttachments.Remove(attachment);
            }
        }

        // Handle new attachments
        if (request.NewAttachments?.Any() == true)
        {
            foreach (var file in request.NewAttachments)
            {
                if (file.Length > 0)
                {
                    var uploadResult = await _fileStorageService.UploadAsync(
                        file.OpenReadStream(),
                        file.FileName,
                        file.ContentType,
                        "hazards");

                    hazard.AddAttachment(file.FileName, uploadResult.FilePath, file.Length, _currentUserService.Name);
                }
            }
        }

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // Domain events are added by the entity methods automatically
        // Additional save to persist domain events
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var hazardDto = new HazardDto
        {
            Id = hazard.Id,
            Title = hazard.Title,
            Description = hazard.Description,
            Category = hazard.Category.ToString(),
            Type = hazard.Type.ToString(),
            Location = hazard.Location,
            Latitude = hazard.GeoLocation?.Latitude,
            Longitude = hazard.GeoLocation?.Longitude,
            Status = hazard.Status.ToString(),
            Severity = hazard.Severity.ToString(),
            IdentifiedDate = hazard.IdentifiedDate,
            ExpectedResolutionDate = hazard.ExpectedResolutionDate,
            ReporterName = hazard.Reporter.Name,
            ReporterEmail = hazard.Reporter.Email,
            ReporterDepartment = hazard.ReporterDepartment,
            CurrentRiskLevel = hazard.CurrentRiskAssessment?.RiskLevel.ToString(),
            CurrentRiskScore = hazard.CurrentRiskAssessment?.RiskScore,
            LastAssessmentDate = hazard.CurrentRiskAssessment?.AssessmentDate,
            AttachmentsCount = hazard.Attachments.Count,
            RiskAssessmentsCount = hazard.RiskAssessments.Count,
            MitigationActionsCount = hazard.MitigationActions.Count,
            PendingActionsCount = hazard.MitigationActions.Count(ma => ma.Status != MitigationActionStatus.Completed),
            CreatedAt = hazard.CreatedAt,
            CreatedBy = hazard.CreatedBy,
            LastModifiedAt = hazard.LastModifiedAt,
            LastModifiedBy = hazard.LastModifiedBy,
            Reporter = new UserDto
            {
                Id = hazard.Reporter.Id,
                Name = hazard.Reporter.Name,
                Email = hazard.Reporter.Email,
                Department = hazard.Reporter.Department,
                Position = hazard.Reporter.Position,
                EmployeeId = hazard.Reporter.EmployeeId
            }
        };

        if (hazard.CurrentRiskAssessment != null)
        {
            hazardDto.CurrentRiskAssessment = new RiskAssessmentDto
            {
                Id = hazard.CurrentRiskAssessment.Id,
                HazardId = hazard.CurrentRiskAssessment.HazardId,
                Type = hazard.CurrentRiskAssessment.Type.ToString(),
                AssessmentDate = hazard.CurrentRiskAssessment.AssessmentDate,
                ProbabilityScore = hazard.CurrentRiskAssessment.ProbabilityScore,
                SeverityScore = hazard.CurrentRiskAssessment.SeverityScore,
                RiskScore = hazard.CurrentRiskAssessment.RiskScore,
                RiskLevel = hazard.CurrentRiskAssessment.RiskLevel.ToString(),
                PotentialConsequences = hazard.CurrentRiskAssessment.PotentialConsequences,
                ExistingControls = hazard.CurrentRiskAssessment.ExistingControls,
                RecommendedActions = hazard.CurrentRiskAssessment.RecommendedActions,
                NextReviewDate = hazard.CurrentRiskAssessment.NextReviewDate,
                IsActive = hazard.CurrentRiskAssessment.IsActive
            };
        }

        return hazardDto;
    }
}