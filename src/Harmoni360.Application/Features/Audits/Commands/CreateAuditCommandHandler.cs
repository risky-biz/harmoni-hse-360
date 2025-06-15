using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Audits.DTOs;
using Harmoni360.Domain.Entities.Audits;

namespace Harmoni360.Application.Features.Audits.Commands;

public class CreateAuditCommandHandler : IRequestHandler<CreateAuditCommand, AuditDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateAuditCommandHandler> _logger;

    public CreateAuditCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateAuditCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AuditDto> Handle(CreateAuditCommand request, CancellationToken cancellationToken)
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

            // Create the audit
            var audit = Audit.Create(
                request.Title,
                request.Description,
                request.Type,
                request.Category,
                request.Priority,
                request.ScheduledDate,
                currentUser.Id,
                request.LocationId,
                request.DepartmentId,
                request.FacilityId,
                request.EstimatedDurationMinutes);

            // Set compliance information
            if (!string.IsNullOrEmpty(request.StandardsApplied) || request.IsRegulatory)
            {
                audit.SetComplianceInfo(
                    request.StandardsApplied,
                    request.IsRegulatory,
                    request.RegulatoryReference);
            }

            // Set audit fields
            audit.CreatedBy = currentUser.Name;
            audit.CreatedAt = DateTime.UtcNow;

            _context.Audits.Add(audit);
            await _context.SaveChangesAsync(cancellationToken);

            // Add audit items if provided
            foreach (var itemRequest in request.Items)
            {
                var item = AuditItem.Create(
                    audit.Id,
                    itemRequest.Description,
                    itemRequest.Type,
                    itemRequest.IsRequired,
                    itemRequest.Category,
                    itemRequest.SortOrder,
                    itemRequest.ExpectedResult,
                    itemRequest.MaxPoints);

                if (!string.IsNullOrEmpty(itemRequest.ValidationCriteria))
                {
                    item.SetValidationCriteria(itemRequest.ValidationCriteria, itemRequest.AcceptanceCriteria);
                }

                audit.AddItem(item);
            }

            // Save audit items
            if (request.Items.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Audit created successfully. ID: {AuditId}, Number: {AuditNumber}", 
                audit.Id, audit.AuditNumber);

            // Return DTO
            return await MapToDto(audit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit for user {UserId}", _currentUserService.UserId);
            throw;
        }
    }

    private async Task<AuditDto> MapToDto(Audit audit)
    {
        // Load related data
        var dbContext = (DbContext)_context;
        await dbContext.Entry(audit)
            .Reference(a => a.Auditor)
            .LoadAsync();
        await dbContext.Entry(audit)
            .Reference(a => a.Department)
            .LoadAsync();
        await dbContext.Entry(audit)
            .Collection(a => a.Items)
            .LoadAsync();
        await dbContext.Entry(audit)
            .Collection(a => a.Attachments)
            .LoadAsync();
        await dbContext.Entry(audit)
            .Collection(a => a.Findings)
            .LoadAsync();
        await dbContext.Entry(audit)
            .Collection(a => a.Comments)
            .LoadAsync();

        return new AuditDto
        {
            Id = audit.Id,
            AuditNumber = audit.AuditNumber,
            Title = audit.Title,
            Description = audit.Description,
            Type = audit.Type.ToString(),
            TypeDisplay = GetTypeDisplay(audit.Type),
            Category = audit.Category.ToString(),
            CategoryDisplay = GetCategoryDisplay(audit.Category),
            Status = audit.Status.ToString(),
            StatusDisplay = GetStatusDisplay(audit.Status),
            Priority = audit.Priority.ToString(),
            PriorityDisplay = GetPriorityDisplay(audit.Priority),
            ScheduledDate = audit.ScheduledDate,
            StartedDate = audit.StartedDate,
            CompletedDate = audit.CompletedDate,
            AuditorId = audit.AuditorId,
            AuditorName = audit.Auditor.Name,
            LocationId = audit.LocationId,
            DepartmentId = audit.DepartmentId,
            DepartmentName = audit.Department?.Name,
            FacilityId = audit.FacilityId,
            RiskLevel = audit.RiskLevel.ToString(),
            RiskLevelDisplay = GetRiskLevelDisplay(audit.RiskLevel),
            Summary = audit.Summary,
            Recommendations = audit.Recommendations,
            OverallScore = audit.OverallScore?.ToString(),
            ScorePercentage = audit.ScorePercentage,
            EstimatedDurationMinutes = audit.EstimatedDurationMinutes,
            ActualDurationMinutes = audit.ActualDurationMinutes,
            StandardsApplied = audit.StandardsApplied,
            IsRegulatory = audit.IsRegulatory,
            RegulatoryReference = audit.RegulatoryReference,
            TotalPossiblePoints = audit.TotalPossiblePoints,
            AchievedPoints = audit.AchievedPoints,
            CreatedAt = audit.CreatedAt,
            CreatedBy = audit.CreatedBy,
            UpdatedAt = audit.LastModifiedAt,
            UpdatedBy = audit.LastModifiedBy,
            Items = audit.Items.Select(MapItemToDto).ToList(),
            Attachments = audit.Attachments.Select(MapAttachmentToDto).ToList(),
            Findings = audit.Findings.Select(MapFindingToDto).ToList(),
            Comments = audit.Comments.Select(MapCommentToDto).ToList()
        };
    }

    private static AuditItemDto MapItemToDto(AuditItem item)
    {
        return new AuditItemDto
        {
            Id = item.Id,
            AuditId = item.AuditId,
            ItemNumber = item.ItemNumber,
            Description = item.Description,
            Type = item.Type.ToString(),
            Status = item.Status.ToString(),
            Category = item.Category,
            IsRequired = item.IsRequired,
            SortOrder = item.SortOrder,
            ExpectedResult = item.ExpectedResult,
            ActualResult = item.ActualResult,
            Comments = item.Comments,
            IsCompliant = item.IsCompliant,
            MaxPoints = item.MaxPoints,
            ActualPoints = item.ActualPoints,
            AssessedBy = item.AssessedBy,
            AssessedAt = item.AssessedAt,
            Evidence = item.Evidence,
            CorrectiveAction = item.CorrectiveAction,
            DueDate = item.DueDate,
            ResponsiblePersonId = item.ResponsiblePersonId,
            ValidationCriteria = item.ValidationCriteria,
            AcceptanceCriteria = item.AcceptanceCriteria,
            RequiresFollowUp = item.RequiresFollowUp,
            IsOverdue = item.IsOverdue,
            ScorePercentage = item.ScorePercentage
        };
    }

    private static AuditAttachmentDto MapAttachmentToDto(AuditAttachment attachment)
    {
        return new AuditAttachmentDto
        {
            Id = attachment.Id,
            AuditId = attachment.AuditId,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedBy = attachment.UploadedBy,
            UploadedAt = attachment.UploadedAt,
            AttachmentType = attachment.AttachmentType.ToString(),
            Description = attachment.Description,
            Category = attachment.Category,
            IsEvidence = attachment.IsEvidence,
            AuditItemId = attachment.AuditItemId
        };
    }

    private static AuditFindingDto MapFindingToDto(AuditFinding finding)
    {
        return new AuditFindingDto
        {
            Id = finding.Id,
            AuditId = finding.AuditId,
            FindingNumber = finding.FindingNumber,
            Description = finding.Description,
            Type = finding.Type.ToString(),
            TypeDisplay = GetFindingTypeDisplay(finding.Type),
            Severity = finding.Severity.ToString(),
            SeverityDisplay = GetFindingSeverityDisplay(finding.Severity),
            RiskLevel = finding.RiskLevel.ToString(),
            Status = finding.Status.ToString(),
            StatusDisplay = finding.StatusDisplay,
            Location = finding.Location,
            Equipment = finding.Equipment,
            Standard = finding.Standard,
            Regulation = finding.Regulation,
            AuditItemId = finding.AuditItemId,
            RootCause = finding.RootCause,
            ImmediateAction = finding.ImmediateAction,
            CorrectiveAction = finding.CorrectiveAction,
            PreventiveAction = finding.PreventiveAction,
            DueDate = finding.DueDate,
            ResponsiblePersonId = finding.ResponsiblePersonId,
            ResponsiblePersonName = finding.ResponsiblePersonName,
            ClosedDate = finding.ClosedDate,
            ClosureNotes = finding.ClosureNotes,
            ClosedBy = finding.ClosedBy,
            VerificationMethod = finding.VerificationMethod,
            RequiresVerification = finding.RequiresVerification,
            VerificationDate = finding.VerificationDate,
            VerifiedBy = finding.VerifiedBy,
            EstimatedCost = finding.EstimatedCost,
            ActualCost = finding.ActualCost,
            BusinessImpact = finding.BusinessImpact,
            IsOverdue = finding.IsOverdue,
            CanEdit = finding.CanEdit,
            CanClose = finding.CanClose,
            IsCritical = finding.IsCritical,
            DaysOverdue = finding.DaysOverdue
        };
    }

    private static AuditCommentDto MapCommentToDto(AuditComment comment)
    {
        return new AuditCommentDto
        {
            Id = comment.Id,
            AuditId = comment.AuditId,
            Comment = comment.Comment,
            CommentedBy = comment.CommentedBy,
            CommentedAt = comment.CommentedAt,
            AuditItemId = comment.AuditItemId,
            AuditFindingId = comment.AuditFindingId,
            Category = comment.Category,
            IsInternal = comment.IsInternal
        };
    }

    // Display name mappings
    private static string GetTypeDisplay(Domain.Enums.AuditType type) => type switch
    {
        Domain.Enums.AuditType.Safety => "Safety Audit",
        Domain.Enums.AuditType.Environmental => "Environmental Audit",
        Domain.Enums.AuditType.Equipment => "Equipment Audit",
        Domain.Enums.AuditType.Compliance => "Compliance Audit",
        Domain.Enums.AuditType.Fire => "Fire Safety Audit",
        Domain.Enums.AuditType.Chemical => "Chemical Safety Audit",
        Domain.Enums.AuditType.Ergonomic => "Ergonomic Audit",
        Domain.Enums.AuditType.Emergency => "Emergency Preparedness Audit",
        Domain.Enums.AuditType.Management => "Management System Audit",
        Domain.Enums.AuditType.Process => "Process Audit",
        _ => type.ToString()
    };

    private static string GetCategoryDisplay(Domain.Enums.AuditCategory category) => category switch
    {
        Domain.Enums.AuditCategory.Routine => "Routine",
        Domain.Enums.AuditCategory.Planned => "Planned",
        Domain.Enums.AuditCategory.Unplanned => "Unplanned",
        Domain.Enums.AuditCategory.Regulatory => "Regulatory",
        Domain.Enums.AuditCategory.Internal => "Internal",
        Domain.Enums.AuditCategory.External => "External",
        Domain.Enums.AuditCategory.Incident => "Incident-related",
        Domain.Enums.AuditCategory.Maintenance => "Maintenance",
        _ => category.ToString()
    };

    private static string GetStatusDisplay(Domain.Enums.AuditStatus status) => status switch
    {
        Domain.Enums.AuditStatus.Draft => "Draft",
        Domain.Enums.AuditStatus.Scheduled => "Scheduled",
        Domain.Enums.AuditStatus.InProgress => "In Progress",
        Domain.Enums.AuditStatus.Completed => "Completed",
        Domain.Enums.AuditStatus.Overdue => "Overdue",
        Domain.Enums.AuditStatus.Cancelled => "Cancelled",
        Domain.Enums.AuditStatus.Archived => "Archived",
        Domain.Enums.AuditStatus.UnderReview => "Under Review",
        _ => status.ToString()
    };

    private static string GetPriorityDisplay(Domain.Enums.AuditPriority priority) => priority switch
    {
        Domain.Enums.AuditPriority.Low => "Low",
        Domain.Enums.AuditPriority.Medium => "Medium",
        Domain.Enums.AuditPriority.High => "High",
        Domain.Enums.AuditPriority.Critical => "Critical",
        _ => priority.ToString()
    };

    private static string GetRiskLevelDisplay(Domain.Enums.RiskLevel riskLevel) => riskLevel switch
    {
        Domain.Enums.RiskLevel.Low => "Low",
        Domain.Enums.RiskLevel.Medium => "Medium",
        Domain.Enums.RiskLevel.High => "High",
        Domain.Enums.RiskLevel.Critical => "Critical",
        _ => riskLevel.ToString()
    };

    private static string GetFindingTypeDisplay(Domain.Enums.FindingType type) => type switch
    {
        Domain.Enums.FindingType.NonConformance => "Non-Conformance",
        Domain.Enums.FindingType.Observation => "Observation",
        Domain.Enums.FindingType.OpportunityForImprovement => "Opportunity for Improvement",
        Domain.Enums.FindingType.PositiveFinding => "Positive Finding",
        Domain.Enums.FindingType.CriticalNonConformance => "Critical Non-Conformance",
        _ => type.ToString()
    };

    private static string GetFindingSeverityDisplay(Domain.Enums.FindingSeverity severity) => severity switch
    {
        Domain.Enums.FindingSeverity.Minor => "Minor",
        Domain.Enums.FindingSeverity.Moderate => "Moderate",
        Domain.Enums.FindingSeverity.Major => "Major",
        Domain.Enums.FindingSeverity.Critical => "Critical",
        _ => severity.ToString()
    };
}