using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class CreateInspectionCommandHandler : IRequestHandler<CreateInspectionCommand, InspectionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateInspectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InspectionDto> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        // Validate inspector exists
        var inspector = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.InspectorId, cancellationToken);
        
        if (inspector == null)
            throw new ArgumentException($"Inspector with ID {request.InspectorId} not found");

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);
            
            if (!departmentExists)
                throw new ArgumentException($"Department with ID {request.DepartmentId} not found");
        }

        // Create inspection
        var inspection = Inspection.Create(
            request.Title,
            request.Description,
            request.Type,
            request.Category,
            request.Priority,
            request.ScheduledDate,
            request.InspectorId,
            request.LocationId,
            request.DepartmentId,
            request.FacilityId);

        // Set estimated duration if provided
        if (request.EstimatedDurationMinutes.HasValue)
        {
            // Use reflection to set the private property or add a method to the domain entity
            var property = typeof(Inspection).GetProperty(nameof(Inspection.EstimatedDurationMinutes));
            property?.SetValue(inspection, request.EstimatedDurationMinutes.Value);
        }

        // Add inspection items
        foreach (var itemRequest in request.Items)
        {
            var item = InspectionItem.Create(
                inspection.Id,
                itemRequest.Question,
                itemRequest.Type,
                itemRequest.IsRequired,
                itemRequest.Description,
                itemRequest.SortOrder,
                itemRequest.ChecklistItemId);

            // Set validation rules
            item.SetValidationRules(
                itemRequest.MinValue,
                itemRequest.MaxValue,
                itemRequest.ExpectedValue,
                itemRequest.Unit);

            // Set multiple choice options
            if (itemRequest.Type == InspectionItemType.MultipleChoice && itemRequest.Options.Any())
            {
                item.SetMultipleChoiceOptions(itemRequest.Options);
            }

            inspection.AddItem(item);
        }

        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapToDto(inspection, cancellationToken);
    }

    private async Task<InspectionDto> MapToDto(Inspection inspection, CancellationToken cancellationToken)
    {
        var inspector = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == inspection.InspectorId, cancellationToken);

        var department = inspection.DepartmentId.HasValue
            ? await _context.Departments.FirstOrDefaultAsync(d => d.Id == inspection.DepartmentId.Value, cancellationToken)
            : null;

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
            InspectorName = inspector?.Name ?? "Unknown",
            LocationId = inspection.LocationId,
            DepartmentId = inspection.DepartmentId,
            DepartmentName = department?.Name ?? "",
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