using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WorkPermits.Queries;

public class GetWorkPermitsQueryHandler : IRequestHandler<GetWorkPermitsQuery, GetWorkPermitsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetWorkPermitsQueryHandler> _logger;

    public GetWorkPermitsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetWorkPermitsQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetWorkPermitsResponse> Handle(GetWorkPermitsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.WorkPermits
                .Include(wp => wp.Attachments)
                .Include(wp => wp.Approvals)
                .Include(wp => wp.Hazards)
                .Include(wp => wp.Precautions)
                .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, request);

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Calculate summary statistics
            var summary = await CalculateSummary(query, cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDirection);

            // Apply pagination
            var workPermits = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var workPermitDtos = workPermits.Select(MapToDto).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            _logger.LogInformation("Retrieved {Count} work permits for user {UserId}", 
                workPermitDtos.Count, _currentUserService.UserId);

            return new GetWorkPermitsResponse
            {
                Items = workPermitDtos,
                TotalCount = totalCount,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                PageCount = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages,
                Summary = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work permits for user {UserId}", _currentUserService.UserId);
            throw;
        }
    }

    private IQueryable<WorkPermit> ApplyFilters(IQueryable<WorkPermit> query, GetWorkPermitsQuery request)
    {
        // Search term filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(wp => 
                wp.Title.ToLower().Contains(searchTerm) ||
                wp.Description.ToLower().Contains(searchTerm) ||
                wp.PermitNumber.ToLower().Contains(searchTerm) ||
                wp.WorkLocation.ToLower().Contains(searchTerm) ||
                wp.RequestedByName.ToLower().Contains(searchTerm) ||
                wp.WorkScope.ToLower().Contains(searchTerm));
        }

        // Type filter
        if (request.Type.HasValue)
        {
            query = query.Where(wp => wp.Type == request.Type.Value);
        }

        // Status filter
        if (request.Status.HasValue)
        {
            query = query.Where(wp => wp.Status == request.Status.Value);
        }

        // Priority filter
        if (request.Priority.HasValue)
        {
            query = query.Where(wp => wp.Priority == request.Priority.Value);
        }

        // Risk level filter
        if (request.RiskLevel.HasValue)
        {
            query = query.Where(wp => wp.RiskLevel == request.RiskLevel.Value);
        }

        // Requested by filter
        if (request.RequestedById.HasValue)
        {
            query = query.Where(wp => wp.RequestedById == request.RequestedById.Value);
        }

        // Date range filters
        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(wp => wp.PlannedStartDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(wp => wp.PlannedStartDate <= request.StartDateTo.Value);
        }

        if (request.EndDateFrom.HasValue)
        {
            query = query.Where(wp => wp.PlannedEndDate >= request.EndDateFrom.Value);
        }

        if (request.EndDateTo.HasValue)
        {
            query = query.Where(wp => wp.PlannedEndDate <= request.EndDateTo.Value);
        }

        // Department filter
        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(wp => wp.RequestedByDepartment.ToLower().Contains(request.Department.ToLower()));
        }

        // Work location filter
        if (!string.IsNullOrWhiteSpace(request.WorkLocation))
        {
            query = query.Where(wp => wp.WorkLocation.ToLower().Contains(request.WorkLocation.ToLower()));
        }

        // Overdue filter
        if (request.IsOverdue.HasValue && request.IsOverdue.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(wp => 
                wp.Status != WorkPermitStatus.Completed && 
                wp.Status != WorkPermitStatus.Cancelled && 
                wp.PlannedEndDate < now);
        }

        // Requires approval filter
        if (request.RequiresApproval.HasValue && request.RequiresApproval.Value)
        {
            query = query.Where(wp => wp.Status == WorkPermitStatus.PendingApproval);
        }

        // Indonesian compliance filters
        if (request.IsJamsostekCompliant.HasValue)
        {
            query = query.Where(wp => wp.IsJamsostekCompliant == request.IsJamsostekCompliant.Value);
        }

        if (request.HasSMK3Compliance.HasValue)
        {
            query = query.Where(wp => wp.HasSMK3Compliance == request.HasSMK3Compliance.Value);
        }

        return query;
    }

    private IQueryable<WorkPermit> ApplySorting(IQueryable<WorkPermit> query, string sortBy, string sortDirection)
    {
        var isDescending = sortDirection.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "permitnumber" => isDescending ? query.OrderByDescending(wp => wp.PermitNumber) : query.OrderBy(wp => wp.PermitNumber),
            "title" => isDescending ? query.OrderByDescending(wp => wp.Title) : query.OrderBy(wp => wp.Title),
            "type" => isDescending ? query.OrderByDescending(wp => wp.Type) : query.OrderBy(wp => wp.Type),
            "status" => isDescending ? query.OrderByDescending(wp => wp.Status) : query.OrderBy(wp => wp.Status),
            "priority" => isDescending ? query.OrderByDescending(wp => wp.Priority) : query.OrderBy(wp => wp.Priority),
            "risklevel" => isDescending ? query.OrderByDescending(wp => wp.RiskLevel) : query.OrderBy(wp => wp.RiskLevel),
            "requestedby" => isDescending ? query.OrderByDescending(wp => wp.RequestedByName) : query.OrderBy(wp => wp.RequestedByName),
            "plannedstartdate" => isDescending ? query.OrderByDescending(wp => wp.PlannedStartDate) : query.OrderBy(wp => wp.PlannedStartDate),
            "plannedenddate" => isDescending ? query.OrderByDescending(wp => wp.PlannedEndDate) : query.OrderBy(wp => wp.PlannedEndDate),
            "worklocation" => isDescending ? query.OrderByDescending(wp => wp.WorkLocation) : query.OrderBy(wp => wp.WorkLocation),
            "updatedat" => isDescending ? query.OrderByDescending(wp => wp.LastModifiedAt) : query.OrderBy(wp => wp.LastModifiedAt),
            _ => isDescending ? query.OrderByDescending(wp => wp.CreatedAt) : query.OrderBy(wp => wp.CreatedAt),
        };
    }

    private async Task<WorkPermitSummaryDto> CalculateSummary(IQueryable<WorkPermit> query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        
        var summary = await query
            .GroupBy(wp => 1)
            .Select(g => new WorkPermitSummaryDto
            {
                TotalPermits = g.Count(),
                DraftPermits = g.Count(wp => wp.Status == WorkPermitStatus.Draft),
                PendingApprovalPermits = g.Count(wp => wp.Status == WorkPermitStatus.PendingApproval),
                ApprovedPermits = g.Count(wp => wp.Status == WorkPermitStatus.Approved),
                InProgressPermits = g.Count(wp => wp.Status == WorkPermitStatus.InProgress),
                CompletedPermits = g.Count(wp => wp.Status == WorkPermitStatus.Completed),
                RejectedPermits = g.Count(wp => wp.Status == WorkPermitStatus.Rejected),
                CancelledPermits = g.Count(wp => wp.Status == WorkPermitStatus.Cancelled),
                ExpiredPermits = g.Count(wp => wp.Status == WorkPermitStatus.Expired),
                OverduePermits = g.Count(wp => 
                    wp.Status != WorkPermitStatus.Completed && 
                    wp.Status != WorkPermitStatus.Cancelled && 
                    wp.PlannedEndDate < now),
                HighRiskPermits = g.Count(wp => wp.RiskLevel == RiskLevel.High),
                CriticalRiskPermits = g.Count(wp => wp.RiskLevel == RiskLevel.Critical)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return summary ?? new WorkPermitSummaryDto();
    }

    private WorkPermitDto MapToDto(WorkPermit workPermit)
    {
        var dto = new WorkPermitDto
        {
            Id = workPermit.Id,
            PermitNumber = workPermit.PermitNumber,
            Title = workPermit.Title,
            Description = workPermit.Description,
            Type = workPermit.Type.ToString(),
            TypeDisplay = GetTypeDisplay(workPermit.Type),
            Status = workPermit.Status.ToString(),
            StatusDisplay = GetStatusDisplay(workPermit.Status),
            Priority = workPermit.Priority.ToString(),
            PriorityDisplay = GetPriorityDisplay(workPermit.Priority),
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
            RiskLevelDisplay = GetRiskLevelDisplay(workPermit.RiskLevel),
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

    private static string GetTypeDisplay(WorkPermitType type)
    {
        return type switch
        {
            WorkPermitType.General => "General Work",
            WorkPermitType.HotWork => "Hot Work",
            WorkPermitType.ColdWork => "Cold Work", 
            WorkPermitType.ConfinedSpace => "Confined Space",
            WorkPermitType.ElectricalWork => "Electrical Work",
            WorkPermitType.Special => "Special Work",
            _ => type.ToString()
        };
    }

    private static string GetStatusDisplay(WorkPermitStatus status)
    {
        return status switch
        {
            WorkPermitStatus.Draft => "Draft",
            WorkPermitStatus.PendingApproval => "Pending Approval",
            WorkPermitStatus.Approved => "Approved",
            WorkPermitStatus.Rejected => "Rejected",
            WorkPermitStatus.InProgress => "In Progress",
            WorkPermitStatus.Completed => "Completed",
            WorkPermitStatus.Cancelled => "Cancelled",
            WorkPermitStatus.Expired => "Expired",
            _ => status.ToString()
        };
    }

    private static string GetPriorityDisplay(WorkPermitPriority priority)
    {
        return priority switch
        {
            WorkPermitPriority.Low => "Low",
            WorkPermitPriority.Medium => "Medium",
            WorkPermitPriority.High => "High",
            WorkPermitPriority.Critical => "Critical",
            _ => priority.ToString()
        };
    }

    private static string GetRiskLevelDisplay(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Low => "Low",
            RiskLevel.Medium => "Medium",
            RiskLevel.High => "High",
            RiskLevel.Critical => "Critical",
            _ => riskLevel.ToString()
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