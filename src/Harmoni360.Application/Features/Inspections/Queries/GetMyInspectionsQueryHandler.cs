using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Queries;

public class GetMyInspectionsQueryHandler : IRequestHandler<GetMyInspectionsQuery, PagedList<InspectionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyInspectionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<InspectionDto>> Handle(GetMyInspectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inspections
            .Include(i => i.Inspector)
            .Include(i => i.Department)
            .Include(i => i.Items)
            .Include(i => i.Findings)
            .Include(i => i.Attachments)
            .Where(i => i.InspectorId == _currentUserService.UserId || i.CreatedBy == _currentUserService.Email)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(i => 
                i.Title.ToLower().Contains(searchTerm) ||
                i.Description.ToLower().Contains(searchTerm) ||
                i.InspectionNumber.ToLower().Contains(searchTerm));
        }

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        if (request.Type.HasValue)
            query = query.Where(i => i.Type == request.Type.Value);

        if (request.Category.HasValue)
            query = query.Where(i => i.Category == request.Category.Value);

        if (request.Priority.HasValue)
            query = query.Where(i => i.Priority == request.Priority.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(i => i.DepartmentId == request.DepartmentId.Value);

        if (request.RiskLevel.HasValue)
            query = query.Where(i => i.RiskLevel == request.RiskLevel.Value);

        if (request.StartDate.HasValue)
            query = query.Where(i => i.ScheduledDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(i => i.ScheduledDate <= request.EndDate.Value);

        if (request.IsOverdue.HasValue)
        {
            if (request.IsOverdue.Value)
                query = query.Where(i => i.Status == InspectionStatus.Scheduled && i.ScheduledDate < DateTime.UtcNow);
            else
                query = query.Where(i => !(i.Status == InspectionStatus.Scheduled && i.ScheduledDate < DateTime.UtcNow));
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "title" => request.SortDescending 
                ? query.OrderByDescending(i => i.Title)
                : query.OrderBy(i => i.Title),
            "type" => request.SortDescending 
                ? query.OrderByDescending(i => i.Type)
                : query.OrderBy(i => i.Type),
            "status" => request.SortDescending 
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            "priority" => request.SortDescending 
                ? query.OrderByDescending(i => i.Priority)
                : query.OrderBy(i => i.Priority),
            "inspector" => request.SortDescending 
                ? query.OrderByDescending(i => i.Inspector.Name)
                : query.OrderBy(i => i.Inspector.Name),
            "createdat" => request.SortDescending 
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt),
            _ => request.SortDescending 
                ? query.OrderByDescending(i => i.ScheduledDate)
                : query.OrderBy(i => i.ScheduledDate)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var inspections = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var inspectionDtos = inspections.Select(MapToDto).ToList();

        return new PagedList<InspectionDto>(
            inspectionDtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private static InspectionDto MapToDto(Inspection inspection)
    {
        return new InspectionDto
        {
            Id = inspection.Id,
            InspectionNumber = inspection.InspectionNumber,
            Title = inspection.Title,
            Description = inspection.Description,
            Type = inspection.Type,
            TypeName = inspection.Type.ToString(),
            Category = inspection.Category,
            CategoryName = inspection.Category.ToString(),
            Status = inspection.Status,
            StatusName = inspection.Status.ToString(),
            Priority = inspection.Priority,
            PriorityName = inspection.Priority.ToString(),
            ScheduledDate = inspection.ScheduledDate,
            StartedDate = inspection.StartedDate,
            CompletedDate = inspection.CompletedDate,
            InspectorId = inspection.InspectorId,
            InspectorName = inspection.Inspector?.Name ?? "Unknown",
            LocationId = inspection.LocationId,
            DepartmentId = inspection.DepartmentId,
            DepartmentName = inspection.Department?.Name ?? "",
            FacilityId = inspection.FacilityId,
            RiskLevel = inspection.RiskLevel,
            RiskLevelName = inspection.RiskLevel.ToString(),
            Summary = inspection.Summary,
            Recommendations = inspection.Recommendations,
            EstimatedDurationMinutes = inspection.EstimatedDurationMinutes,
            ActualDurationMinutes = inspection.ActualDurationMinutes,
            ItemsCount = inspection.Items.Count,
            CompletedItemsCount = inspection.Items.Count(i => i.IsCompleted),
            FindingsCount = inspection.Findings.Count,
            CriticalFindingsCount = inspection.Findings.Count(f => f.Severity == FindingSeverity.Critical),
            AttachmentsCount = inspection.Attachments.Count,
            CanEdit = inspection.CanEdit,
            CanStart = inspection.CanStart,
            CanComplete = inspection.CanComplete,
            CanCancel = inspection.CanCancel,
            IsOverdue = inspection.IsOverdue,
            CreatedAt = inspection.CreatedAt,
            LastModifiedAt = inspection.LastModifiedAt,
            CreatedBy = inspection.CreatedBy,
            LastModifiedBy = inspection.LastModifiedBy
        };
    }
}