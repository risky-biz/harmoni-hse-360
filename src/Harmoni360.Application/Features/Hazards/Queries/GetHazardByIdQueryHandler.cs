using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Hazards.Queries;

public class GetHazardByIdQueryHandler : IRequestHandler<GetHazardByIdQuery, HazardDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetHazardByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<HazardDetailDto?> Handle(GetHazardByIdQuery request, CancellationToken cancellationToken)
    {
        // Check cache first for frequently accessed hazards
        var cacheKey = $"hazard_detail_{request.Id}";
        var cachedResult = await _cacheService.GetAsync<HazardDetailDto>(cacheKey);
        
        if (cachedResult != null && !ShouldBypassCache(request))
        {
            return cachedResult;
        }

        // Build query with conditional includes for performance
        var query = _context.Hazards
            .Include(h => h.Reporter)
            .Include(h => h.CurrentRiskAssessment)
                .ThenInclude(ra => ra != null ? ra.Assessor : null)
            .AsQueryable();

        // Conditional includes based on request
        if (request.IncludeAttachments)
        {
            query = query.Include(h => h.Attachments);
        }

        if (request.IncludeRiskAssessments)
        {
            query = query.Include(h => h.RiskAssessments)
                        .ThenInclude(ra => ra.Assessor);
        }

        if (request.IncludeMitigationActions)
        {
            query = query.Include(h => h.MitigationActions)
                        .ThenInclude(ma => ma.AssignedTo)
                        .Include(h => h.MitigationActions)
                        .ThenInclude(ma => ma.VerifiedBy);
        }

        if (request.IncludeReassessments)
        {
            query = query.Include(h => h.Reassessments);
        }

        // Execute query
        var hazard = await query.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);
        
        if (hazard == null)
        {
            return null;
        }

        // Check authorization - basic check for user access
        if (!CanAccessHazard(hazard, cancellationToken))
        {
            return null; // Return null instead of throwing for security
        }

        // Map to detailed DTO
        var hazardDetailDto = await MapToDetailDto(hazard, request, cancellationToken);

        // Cache the result for 5 minutes if it's a basic query
        if (!ShouldBypassCache(request))
        {
            await _cacheService.SetAsync(cacheKey, hazardDetailDto, TimeSpan.FromMinutes(5));
        }

        return hazardDetailDto;
    }

    private Task<HazardDetailDto> MapToDetailDto(Hazard hazard, GetHazardByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = new HazardDetailDto
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

        // Map current risk assessment if exists
        if (hazard.CurrentRiskAssessment != null)
        {
            dto.CurrentRiskAssessment = MapRiskAssessmentToDto(hazard.CurrentRiskAssessment);
        }

        // Map attachments
        if (request.IncludeAttachments)
        {
            dto.Attachments = hazard.Attachments.Select(a => new HazardAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt,
                Description = a.Description,
                DownloadUrl = $"/api/hazards/{hazard.Id}/attachments/{a.Id}/download"
            }).OrderByDescending(a => a.UploadedAt).ToList();
        }

        // Map all risk assessments
        if (request.IncludeRiskAssessments)
        {
            dto.RiskAssessments = hazard.RiskAssessments
                .OrderByDescending(ra => ra.AssessmentDate)
                .Select(MapRiskAssessmentToDto)
                .ToList();
        }

        // Map mitigation actions
        if (request.IncludeMitigationActions)
        {
            dto.MitigationActions = hazard.MitigationActions
                .OrderBy(ma => ma.Status == MitigationActionStatus.Completed ? 1 : 0) // Active first
                .ThenByDescending(ma => ma.Priority)
                .ThenBy(ma => ma.TargetDate)
                .Select(ma => new HazardMitigationActionDto
                {
                    Id = ma.Id,
                    HazardId = ma.HazardId,
                    ActionDescription = ma.ActionDescription,
                    Type = ma.Type.ToString(),
                    Status = ma.Status.ToString(),
                    Priority = ma.Priority.ToString(),
                    TargetDate = ma.TargetDate,
                    CompletedDate = ma.CompletedDate,
                    AssignedToName = ma.AssignedTo.Name,
                    CompletionNotes = ma.CompletionNotes,
                    EstimatedCost = ma.EstimatedCost,
                    ActualCost = ma.ActualCost,
                    EffectivenessRating = ma.EffectivenessRating,
                    EffectivenessNotes = ma.EffectivenessNotes,
                    RequiresVerification = ma.RequiresVerification,
                    VerifiedByName = ma.VerifiedBy?.Name,
                    VerifiedAt = ma.VerifiedAt,
                    VerificationNotes = ma.VerificationNotes,
                    AssignedTo = new UserDto
                    {
                        Id = ma.AssignedTo.Id,
                        Name = ma.AssignedTo.Name,
                        Email = ma.AssignedTo.Email,
                        Department = ma.AssignedTo.Department,
                        Position = ma.AssignedTo.Position,
                        EmployeeId = ma.AssignedTo.EmployeeId
                    },
                    VerifiedBy = ma.VerifiedBy != null ? new UserDto
                    {
                        Id = ma.VerifiedBy.Id,
                        Name = ma.VerifiedBy.Name,
                        Email = ma.VerifiedBy.Email,
                        Department = ma.VerifiedBy.Department,
                        Position = ma.VerifiedBy.Position,
                        EmployeeId = ma.VerifiedBy.EmployeeId
                    } : null
                }).ToList();
        }

        // Map reassessments
        if (request.IncludeReassessments)
        {
            dto.Reassessments = hazard.Reassessments
                .OrderByDescending(r => r.ScheduledDate)
                .Select(r => new HazardReassessmentDto
                {
                    Id = r.Id,
                    HazardId = r.HazardId,
                    ScheduledDate = r.ScheduledDate,
                    Reason = r.Reason,
                    IsCompleted = r.IsCompleted,
                    CompletedAt = r.CompletedAt,
                    CompletedByName = r.CompletedBy?.Name,
                    CompletionNotes = r.CompletionNotes,
                    CreatedAt = r.CreatedAt,
                    CompletedBy = r.CompletedBy != null ? new UserDto
                    {
                        Id = r.CompletedBy.Id,
                        Name = r.CompletedBy.Name,
                        Email = r.CompletedBy.Email,
                        Department = r.CompletedBy.Department,
                        Position = r.CompletedBy.Position,
                        EmployeeId = r.CompletedBy.EmployeeId
                    } : null
                }).ToList();
        }

        return Task.FromResult(dto);
    }

    private static RiskAssessmentDto MapRiskAssessmentToDto(RiskAssessment riskAssessment)
    {
        return new RiskAssessmentDto
        {
            Id = riskAssessment.Id,
            HazardId = riskAssessment.HazardId,
            Type = riskAssessment.Type.ToString(),
            AssessorName = riskAssessment.Assessor.Name,
            AssessmentDate = riskAssessment.AssessmentDate,
            ProbabilityScore = riskAssessment.ProbabilityScore,
            SeverityScore = riskAssessment.SeverityScore,
            RiskScore = riskAssessment.RiskScore,
            RiskLevel = riskAssessment.RiskLevel.ToString(),
            PotentialConsequences = riskAssessment.PotentialConsequences,
            ExistingControls = riskAssessment.ExistingControls,
            RecommendedActions = riskAssessment.RecommendedActions,
            AdditionalNotes = riskAssessment.AdditionalNotes,
            NextReviewDate = riskAssessment.NextReviewDate,
            IsActive = riskAssessment.IsActive,
            Assessor = new UserDto
            {
                Id = riskAssessment.Assessor.Id,
                Name = riskAssessment.Assessor.Name,
                Email = riskAssessment.Assessor.Email,
                Department = riskAssessment.Assessor.Department,
                Position = riskAssessment.Assessor.Position,
                EmployeeId = riskAssessment.Assessor.EmployeeId
            }
        };
    }

    private bool CanAccessHazard(Hazard hazard, CancellationToken cancellationToken)
    {
        // Basic authorization logic - can be expanded based on business requirements
        var currentUserId = _currentUserService.UserId;
        
        // System administrators can see all hazards
        if (_currentUserService.IsInRole("Administrator"))
        {
            return true;
        }

        // Safety managers can see all hazards
        if (_currentUserService.IsInRole("SafetyManager"))
        {
            return true;
        }

        // Users can see hazards they reported or are assigned to mitigation actions
        if (currentUserId > 0)
        {
            if (hazard.ReporterId == currentUserId)
            {
                return true;
            }

            var isAssignedToActions = hazard.MitigationActions.Any(ma => ma.AssignedToId == currentUserId);
            if (isAssignedToActions)
            {
                return true;
            }
        }

        // For now, return true - department-based access would require additional user data
        return true;
    }

    private static bool ShouldBypassCache(GetHazardByIdQuery request)
    {
        // Bypass cache for admin queries that include audit trails or related incidents
        return request.IncludeAuditTrail || request.IncludeRelatedIncidents;
    }
}