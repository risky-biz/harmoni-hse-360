using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class UpdateInspectionCommandHandler : IRequestHandler<UpdateInspectionCommand, InspectionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateInspectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InspectionDto> Handle(UpdateInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections
            .Include(i => i.Items)
            .Include(i => i.Inspector)
            .Include(i => i.Department)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inspection == null)
            throw new ArgumentException($"Inspection with ID {request.Id} not found");

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);
            
            if (!departmentExists)
                throw new ArgumentException($"Department with ID {request.DepartmentId} not found");
        }

        // Update basic information
        inspection.UpdateBasicInfo(
            request.Title,
            request.Description,
            request.Priority,
            request.ScheduledDate,
            request.LocationId,
            request.DepartmentId,
            request.FacilityId);

        // Update estimated duration if provided
        if (request.EstimatedDurationMinutes.HasValue)
        {
            var property = typeof(Inspection).GetProperty(nameof(Inspection.EstimatedDurationMinutes));
            property?.SetValue(inspection, request.EstimatedDurationMinutes.Value);
        }

        // Update inspection items
        UpdateInspectionItems(inspection, request.Items, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return await MapToDto(inspection, cancellationToken);
    }

    private void UpdateInspectionItems(
        Inspection inspection, 
        List<UpdateInspectionItemCommand> itemCommands, 
        CancellationToken cancellationToken)
    {
        var currentItems = inspection.Items.ToList();
        var updatedItemIds = itemCommands.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();

        // Remove items that are not in the update list
        foreach (var item in currentItems.Where(i => !updatedItemIds.Contains(i.Id)))
        {
            inspection.RemoveItem(item);
        }

        foreach (var itemCommand in itemCommands)
        {
            if (itemCommand.Id.HasValue)
            {
                // Update existing item
                var existingItem = currentItems.FirstOrDefault(i => i.Id == itemCommand.Id.Value);
                if (existingItem != null)
                {
                    existingItem.UpdateResponse(itemCommand.Response ?? string.Empty, itemCommand.Notes);
                    
                    // Update validation rules
                    existingItem.SetValidationRules(
                        itemCommand.MinValue,
                        itemCommand.MaxValue,
                        itemCommand.ExpectedValue,
                        itemCommand.Unit);

                    // Update multiple choice options
                    if (itemCommand.Type == InspectionItemType.MultipleChoice && itemCommand.Options.Any())
                    {
                        existingItem.SetMultipleChoiceOptions(itemCommand.Options);
                    }
                }
            }
            else
            {
                // Create new item
                var newItem = InspectionItem.Create(
                    inspection.Id,
                    itemCommand.Question,
                    itemCommand.Type,
                    itemCommand.IsRequired,
                    itemCommand.Description,
                    itemCommand.SortOrder,
                    itemCommand.ChecklistItemId);

                // Set response if provided
                if (!string.IsNullOrEmpty(itemCommand.Response))
                {
                    newItem.UpdateResponse(itemCommand.Response, itemCommand.Notes);
                }

                // Set validation rules
                newItem.SetValidationRules(
                    itemCommand.MinValue,
                    itemCommand.MaxValue,
                    itemCommand.ExpectedValue,
                    itemCommand.Unit);

                // Set multiple choice options
                if (itemCommand.Type == InspectionItemType.MultipleChoice && itemCommand.Options.Any())
                {
                    newItem.SetMultipleChoiceOptions(itemCommand.Options);
                }

                inspection.AddItem(newItem);
            }
        }
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