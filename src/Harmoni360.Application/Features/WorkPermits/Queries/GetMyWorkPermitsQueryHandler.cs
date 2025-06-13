using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WorkPermits.Queries;

public class GetMyWorkPermitsQueryHandler : IRequestHandler<GetMyWorkPermitsQuery, PagedList<WorkPermitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyWorkPermitsQueryHandler> _logger;

    public GetMyWorkPermitsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetMyWorkPermitsQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedList<WorkPermitDto>> Handle(GetMyWorkPermitsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated for GetMyWorkPermitsQuery");
                return new PagedList<WorkPermitDto>(new List<WorkPermitDto>(), 0, request.Page, request.PageSize);
            }

            var query = _context.WorkPermits
                .Include(wp => wp.Attachments)
                .Include(wp => wp.Approvals)
                .Include(wp => wp.Hazards)
                .Include(wp => wp.Precautions)
                .AsQueryable();

            // Filter by user role
            switch (request.Role.ToLower())
            {
                case "requester":
                    query = query.Where(wp => wp.RequestedById == currentUserId);
                    break;
                case "approver":
                    // Find work permits that need approval from this user (based on their role/permissions)
                    query = query.Where(wp => wp.Status == WorkPermitStatus.PendingApproval);
                    break;
                case "supervisor":
                    // Find work permits where this user is assigned as supervisor
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
                    if (currentUser != null)
                    {
                        query = query.Where(wp => wp.WorkSupervisor == currentUser.Name || wp.SafetyOfficer == currentUser.Name);
                    }
                    break;
                default:
                    // Default to requester role
                    query = query.Where(wp => wp.RequestedById == currentUserId);
                    break;
            }

            // Apply status filter
            if (request.Status.HasValue)
            {
                query = query.Where(wp => wp.Status == request.Status.Value);
            }

            // Apply type filter
            if (request.Type.HasValue)
            {
                query = query.Where(wp => wp.Type == request.Type.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var workPermits = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var workPermitDtos = workPermits.Select(MapToDto).ToList();

            _logger.LogInformation("Retrieved {Count} work permits for user {UserId} in role {Role}", 
                workPermitDtos.Count, currentUserId, request.Role);

            return new PagedList<WorkPermitDto>(workPermitDtos, totalCount, request.Page, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user's work permits");
            throw;
        }
    }

    private static IQueryable<WorkPermit> ApplySorting(IQueryable<WorkPermit> query, string sortBy, bool sortDescending)
    {
        return sortBy.ToLower() switch
        {
            "permitnumber" => sortDescending ? query.OrderByDescending(wp => wp.PermitNumber) : query.OrderBy(wp => wp.PermitNumber),
            "title" => sortDescending ? query.OrderByDescending(wp => wp.Title) : query.OrderBy(wp => wp.Title),
            "status" => sortDescending ? query.OrderByDescending(wp => wp.Status) : query.OrderBy(wp => wp.Status),
            "type" => sortDescending ? query.OrderByDescending(wp => wp.Type) : query.OrderBy(wp => wp.Type),
            "priority" => sortDescending ? query.OrderByDescending(wp => wp.Priority) : query.OrderBy(wp => wp.Priority),
            "risklevel" => sortDescending ? query.OrderByDescending(wp => wp.RiskLevel) : query.OrderBy(wp => wp.RiskLevel),
            "plannedstartdate" => sortDescending ? query.OrderByDescending(wp => wp.PlannedStartDate) : query.OrderBy(wp => wp.PlannedStartDate),
            "plannedenddate" => sortDescending ? query.OrderByDescending(wp => wp.PlannedEndDate) : query.OrderBy(wp => wp.PlannedEndDate),
            _ => sortDescending ? query.OrderByDescending(wp => wp.CreatedAt) : query.OrderBy(wp => wp.CreatedAt)
        };
    }

    private static WorkPermitDto MapToDto(WorkPermit workPermit)
    {
        return new WorkPermitDto
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
            RiskLevel = workPermit.RiskLevel.ToString(),
            RiskLevelDisplay = GetRiskLevelDisplay(workPermit.RiskLevel),
            WorkLocation = workPermit.WorkLocation,
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
            RiskAssessmentSummary = workPermit.RiskAssessmentSummary,
            EmergencyProcedures = workPermit.EmergencyProcedures,
            CompletionNotes = workPermit.CompletionNotes,
            IsCompletedSafely = workPermit.IsCompletedSafely,
            LessonsLearned = workPermit.LessonsLearned,
            CreatedAt = workPermit.CreatedAt,
            CreatedBy = workPermit.CreatedBy,
            UpdatedAt = workPermit.LastModifiedAt,
            UpdatedBy = workPermit.LastModifiedBy,
            // Indonesian compliance fields
            K3LicenseNumber = workPermit.K3LicenseNumber,
            CompanyWorkPermitNumber = workPermit.CompanyWorkPermitNumber,
            IsJamsostekCompliant = workPermit.IsJamsostekCompliant,
            HasSMK3Compliance = workPermit.HasSMK3Compliance,
            EnvironmentalPermitNumber = workPermit.EnvironmentalPermitNumber,
            // Safety requirements
            RequiresHotWorkPermit = workPermit.RequiresHotWorkPermit,
            RequiresConfinedSpaceEntry = workPermit.RequiresConfinedSpaceEntry,
            RequiresElectricalIsolation = workPermit.RequiresElectricalIsolation,
            RequiresHeightWork = workPermit.RequiresHeightWork,
            RequiresRadiationWork = workPermit.RequiresRadiationWork,
            RequiresExcavation = workPermit.RequiresExcavation,
            RequiresFireWatch = workPermit.RequiresFireWatch,
            RequiresGasMonitoring = workPermit.RequiresGasMonitoring,
            // Collections - simplified mapping
            Attachments = new List<WorkPermitAttachmentDto>(),
            Approvals = new List<WorkPermitApprovalDto>(),
            Hazards = new List<WorkPermitHazardDto>(),
            Precautions = new List<WorkPermitPrecautionDto>()
        };
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

}