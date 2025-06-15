using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Queries;

public class GetInspectionDashboardQueryHandler : IRequestHandler<GetInspectionDashboardQuery, InspectionDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetInspectionDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InspectionDashboardDto> Handle(GetInspectionDashboardQuery request, CancellationToken cancellationToken)
    {
        var inspections = await _context.Inspections
            .Include(i => i.Inspector)
            .Include(i => i.Department)
            .Include(i => i.Items)
            .Include(i => i.Findings)
            .Include(i => i.Attachments)
            .ToListAsync(cancellationToken);

        var totalInspections = inspections.Count;
        var scheduledInspections = inspections.Count(i => i.Status == InspectionStatus.Scheduled);
        var inProgressInspections = inspections.Count(i => i.Status == InspectionStatus.InProgress);
        var completedInspections = inspections.Count(i => i.Status == InspectionStatus.Completed);
        var overdueInspections = inspections.Count(i => i.IsOverdue);
        var criticalFindings = inspections.SelectMany(i => i.Findings).Count(f => f.Severity == FindingSeverity.Critical);

        // Calculate average completion time in hours
        var completedWithDuration = inspections
            .Where(i => i.Status == InspectionStatus.Completed && i.ActualDurationMinutes.HasValue)
            .ToList();
        
        var averageCompletionTime = completedWithDuration.Any() 
            ? Math.Round(completedWithDuration.Average(i => i.ActualDurationMinutes!.Value) / 60.0, 1)
            : 0;

        // Calculate compliance rate (completed vs total required items)
        var totalItems = inspections.SelectMany(i => i.Items).Count();
        var completedItems = inspections.SelectMany(i => i.Items).Count(item => item.IsCompleted);
        var complianceRate = totalItems > 0 ? Math.Round((double)completedItems / totalItems * 100, 1) : 0;

        // Get recent inspections (last 10)
        var recentInspections = inspections
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .Select(MapToDto)
            .ToList();

        // Get critical findings
        var criticalFindingsList = inspections
            .SelectMany(i => i.Findings)
            .Where(f => f.Severity == FindingSeverity.Critical)
            .OrderByDescending(f => f.CreatedAt)
            .Take(10)
            .Select(MapFindingToDto)
            .ToList();

        // Get upcoming inspections (next 2 weeks)
        var upcomingDate = DateTime.UtcNow.AddDays(14);
        var upcomingInspections = inspections
            .Where(i => i.Status == InspectionStatus.Scheduled && i.ScheduledDate <= upcomingDate)
            .OrderBy(i => i.ScheduledDate)
            .Take(10)
            .Select(MapToDto)
            .ToList();

        // Get overdue inspections
        var overdueList = inspections
            .Where(i => i.IsOverdue)
            .OrderBy(i => i.ScheduledDate)
            .Take(10)
            .Select(MapToDto)
            .ToList();

        // Calculate inspection statistics by status
        var inspectionsByStatus = Enum.GetValues(typeof(InspectionStatus)).Cast<InspectionStatus>()
            .Select(status => new InspectionStatusStatistic
            {
                Status = status,
                StatusName = status.ToString(),
                Count = inspections.Count(i => i.Status == status),
                Percentage = totalInspections > 0 ? Math.Round((double)inspections.Count(i => i.Status == status) / totalInspections * 100, 1) : 0
            })
            .Where(s => s.Count > 0)
            .ToList();

        // Calculate inspection statistics by type
        var inspectionsByType = Enum.GetValues(typeof(InspectionType)).Cast<InspectionType>()
            .Select(type => new InspectionTypeStatistic
            {
                Type = type,
                TypeName = type.ToString(),
                Count = inspections.Count(i => i.Type == type),
                Percentage = totalInspections > 0 ? Math.Round((double)inspections.Count(i => i.Type == type) / totalInspections * 100, 1) : 0
            })
            .Where(s => s.Count > 0)
            .ToList();

        // Calculate monthly trends (last 6 months)
        var monthlyTrends = new List<MonthlyInspectionTrend>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            var monthInspections = inspections.Where(insp => 
                insp.CreatedAt >= monthStart && insp.CreatedAt <= monthEnd).ToList();

            monthlyTrends.Add(new MonthlyInspectionTrend
            {
                Month = monthStart.ToString("MMM"),
                Year = monthStart.Year,
                Scheduled = monthInspections.Count(i => i.Status == InspectionStatus.Scheduled),
                Completed = monthInspections.Count(i => i.Status == InspectionStatus.Completed),
                Overdue = monthInspections.Count(i => i.IsOverdue),
                CriticalFindings = monthInspections.SelectMany(i => i.Findings).Count(f => f.Severity == FindingSeverity.Critical)
            });
        }

        return new InspectionDashboardDto
        {
            TotalInspections = totalInspections,
            ScheduledInspections = scheduledInspections,
            InProgressInspections = inProgressInspections,
            CompletedInspections = completedInspections,
            OverdueInspections = overdueInspections,
            CriticalFindings = criticalFindings,
            AverageCompletionTime = averageCompletionTime,
            ComplianceRate = complianceRate,
            RecentInspections = recentInspections,
            CriticalFindingsList = criticalFindingsList,
            OverdueList = overdueList,
            UpcomingInspections = upcomingInspections,
            InspectionsByType = inspectionsByType,
            InspectionsByStatus = inspectionsByStatus,
            MonthlyTrends = monthlyTrends
        };
    }

    private static InspectionDto MapToDto(Domain.Entities.Inspections.Inspection inspection)
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

    private static InspectionFindingDto MapFindingToDto(Domain.Entities.Inspections.InspectionFinding finding)
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
            Attachments = finding.Attachments.Select(a => new FindingAttachmentDto
            {
                Id = a.Id,
                FindingId = a.FindingId,
                FileName = a.FileName,
                OriginalFileName = a.OriginalFileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                FileSizeFormatted = a.GetFileSizeFormatted(),
                FilePath = a.FilePath,
                Description = a.Description,
                IsPhoto = a.IsPhoto,
                ThumbnailPath = a.ThumbnailPath,
                IsDocument = a.IsDocument,
                FileExtension = a.FileExtension,
                CreatedAt = a.CreatedAt,
                LastModifiedAt = a.LastModifiedAt,
                CreatedBy = a.CreatedBy,
                LastModifiedBy = a.LastModifiedBy
            }).ToList()
        };
    }
}