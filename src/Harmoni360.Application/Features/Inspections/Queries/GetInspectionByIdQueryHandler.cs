using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Queries;

public class GetInspectionByIdQueryHandler : IRequestHandler<GetInspectionByIdQuery, InspectionDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetInspectionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InspectionDetailDto?> Handle(GetInspectionByIdQuery request, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections
            .Include(i => i.Inspector)
            .Include(i => i.Department)
            .Include(i => i.Items)
            .Include(i => i.Findings)
                .ThenInclude(f => f.ResponsiblePerson)
            .Include(i => i.Findings)
                .ThenInclude(f => f.Attachments)
            .Include(i => i.Attachments)
            .Include(i => i.Comments)
                .ThenInclude(c => c.User)
            .Include(i => i.Comments)
                .ThenInclude(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(i => i.Id == request.InspectionId, cancellationToken);

        if (inspection == null)
            return null;

        return MapToDetailDto(inspection);
    }

    private static InspectionDetailDto MapToDetailDto(Inspection inspection)
    {
        return new InspectionDetailDto
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
            CanEdit = inspection.CanEdit,
            CanStart = inspection.CanStart,
            CanComplete = inspection.CanComplete,
            CanCancel = inspection.CanCancel,
            IsOverdue = inspection.IsOverdue,
            CreatedAt = inspection.CreatedAt,
            LastModifiedAt = inspection.LastModifiedAt,
            CreatedBy = inspection.CreatedBy,
            LastModifiedBy = inspection.LastModifiedBy,

            // Map related entities
            Items = inspection.Items.OrderBy(i => i.SortOrder).Select(MapItemToDto).ToList(),
            Findings = inspection.Findings.OrderByDescending(f => f.CreatedAt).Select(MapFindingToDto).ToList(),
            Attachments = inspection.Attachments.OrderByDescending(a => a.CreatedAt).Select(MapAttachmentToDto).ToList(),
            Comments = inspection.Comments
                .Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(MapCommentToDto)
                .ToList()
        };
    }

    private static InspectionItemDto MapItemToDto(InspectionItem item)
    {
        return new InspectionItemDto
        {
            Id = item.Id,
            InspectionId = item.InspectionId,
            ChecklistItemId = item.ChecklistItemId,
            Question = item.Question,
            Description = item.Description,
            Type = item.Type,
            TypeName = item.Type.ToString(),
            IsRequired = item.IsRequired,
            Response = item.Response,
            Status = item.Status,
            StatusName = item.Status.ToString(),
            Notes = item.Notes,
            SortOrder = item.SortOrder,
            ExpectedValue = item.ExpectedValue,
            Unit = item.Unit,
            MinValue = item.MinValue,
            MaxValue = item.MaxValue,
            Options = item.GetMultipleChoiceOptions(),
            IsCompliant = item.IsCompliant,
            IsCompleted = item.IsCompleted,
            HasResponse = item.HasResponse
        };
    }

    private static InspectionFindingDto MapFindingToDto(InspectionFinding finding)
    {
        return new InspectionFindingDto
        {
            Id = finding.Id,
            InspectionId = finding.InspectionId,
            FindingNumber = finding.FindingNumber,
            Description = finding.Description,
            Type = finding.Type,
            TypeName = finding.Type.ToString(),
            Severity = finding.Severity,
            SeverityName = finding.Severity.ToString(),
            RiskLevel = finding.RiskLevel,
            RiskLevelName = finding.RiskLevel.ToString(),
            RootCause = finding.RootCause,
            ImmediateAction = finding.ImmediateAction,
            CorrectiveAction = finding.CorrectiveAction,
            DueDate = finding.DueDate,
            ResponsiblePersonId = finding.ResponsiblePersonId,
            ResponsiblePersonName = finding.ResponsiblePerson?.Name ?? "",
            Status = finding.Status,
            StatusName = finding.Status.ToString(),
            Location = finding.Location,
            Equipment = finding.Equipment,
            Regulation = finding.Regulation,
            ClosedDate = finding.ClosedDate,
            ClosureNotes = finding.ClosureNotes,
            IsOverdue = finding.IsOverdue,
            CanEdit = finding.CanEdit,
            CanClose = finding.CanClose,
            HasCorrectiveAction = finding.HasCorrectiveAction,
            CreatedAt = finding.CreatedAt,
            LastModifiedAt = finding.LastModifiedAt,
            CreatedBy = finding.CreatedBy,
            LastModifiedBy = finding.LastModifiedBy,
            Attachments = finding.Attachments.Select(MapFindingAttachmentToDto).ToList()
        };
    }

    private static InspectionAttachmentDto MapAttachmentToDto(InspectionAttachment attachment)
    {
        return new InspectionAttachmentDto
        {
            Id = attachment.Id,
            InspectionId = attachment.InspectionId,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            FileSizeFormatted = attachment.GetFileSizeFormatted(),
            FilePath = attachment.FilePath,
            Description = attachment.Description,
            Category = attachment.Category,
            IsPhoto = attachment.IsPhoto,
            ThumbnailPath = attachment.ThumbnailPath,
            IsDocument = attachment.IsDocument,
            FileExtension = attachment.FileExtension,
            CreatedAt = attachment.CreatedAt,
            LastModifiedAt = attachment.LastModifiedAt,
            CreatedBy = attachment.CreatedBy,
            LastModifiedBy = attachment.LastModifiedBy
        };
    }

    private static FindingAttachmentDto MapFindingAttachmentToDto(FindingAttachment attachment)
    {
        return new FindingAttachmentDto
        {
            Id = attachment.Id,
            FindingId = attachment.FindingId,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            FileSizeFormatted = attachment.GetFileSizeFormatted(),
            FilePath = attachment.FilePath,
            Description = attachment.Description,
            IsPhoto = attachment.IsPhoto,
            ThumbnailPath = attachment.ThumbnailPath,
            IsDocument = attachment.IsDocument,
            FileExtension = attachment.FileExtension,
            CreatedAt = attachment.CreatedAt,
            LastModifiedAt = attachment.LastModifiedAt,
            CreatedBy = attachment.CreatedBy,
            LastModifiedBy = attachment.LastModifiedBy
        };
    }

    private static InspectionCommentDto MapCommentToDto(InspectionComment comment)
    {
        return new InspectionCommentDto
        {
            Id = comment.Id,
            InspectionId = comment.InspectionId,
            UserId = comment.UserId,
            UserName = comment.User?.Name ?? "Unknown",
            Comment = comment.Comment,
            IsInternal = comment.IsInternal,
            ParentCommentId = comment.ParentCommentId,
            IsReply = comment.IsReply,
            HasReplies = comment.HasReplies,
            CreatedAt = comment.CreatedAt,
            LastModifiedAt = comment.LastModifiedAt,
            CreatedBy = comment.CreatedBy,
            LastModifiedBy = comment.LastModifiedBy,
            Replies = comment.Replies
                .OrderBy(r => r.CreatedAt)
                .Select(MapCommentToDto)
                .ToList()
        };
    }
}