using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class UpdateWasteReportCommandHandler : IRequestHandler<UpdateWasteReportCommand, WasteReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IWasteAuditService _auditService;
    private readonly ILogger<UpdateWasteReportCommandHandler> _logger;

    public UpdateWasteReportCommandHandler(
        IApplicationDbContext context,
        IWasteAuditService auditService,
        ILogger<UpdateWasteReportCommandHandler> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<WasteReportDto> Handle(UpdateWasteReportCommand request, CancellationToken cancellationToken)
    {
        var wasteReport = await _context.WasteReports
            .Include(w => w.Reporter)
            .Include(w => w.Attachments)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (wasteReport == null)
        {
            throw new InvalidOperationException($"Waste report with ID {request.Id} not found");
        }

        // Capture original values for audit logging
        var originalTitle = wasteReport.Title;
        var originalDescription = wasteReport.Description;
        var originalLocation = wasteReport.Location;

        // Update using reflection (since the entity uses private setters)
        var type = wasteReport.GetType();
        type.GetProperty("Title")?.SetValue(wasteReport, request.Title);
        type.GetProperty("Description")?.SetValue(wasteReport, request.Description);
        type.GetProperty("Location")?.SetValue(wasteReport, request.Location);
        type.GetProperty("LastModifiedAt")?.SetValue(wasteReport, DateTime.UtcNow);

        // Log field changes
        if (originalTitle != request.Title)
        {
            await _auditService.LogWasteReportUpdatedAsync(request.Id, "Title", originalTitle, request.Title);
        }
        if (originalDescription != request.Description)
        {
            await _auditService.LogWasteReportUpdatedAsync(request.Id, "Description", originalDescription, request.Description);
        }
        if (originalLocation != request.Location)
        {
            await _auditService.LogWasteReportUpdatedAsync(request.Id, "Location", originalLocation, request.Location);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Waste report {Id} updated successfully", wasteReport.Id);

        return new WasteReportDto
        {
            Id = wasteReport.Id,
            Title = wasteReport.Title,
            Description = wasteReport.Description,
            Classification = (Domain.Enums.WasteClassification)(int)wasteReport.Category,
            ClassificationDisplay = wasteReport.Category.ToString(),
            Status = Domain.Enums.WasteReportStatus.Draft,
            StatusDisplay = wasteReport.DisposalStatus.ToString(),
            ReportDate = wasteReport.GeneratedDate,
            ReportedBy = wasteReport.Reporter?.Name ?? "Unknown",
            Location = wasteReport.Location,
            CreatedAt = wasteReport.CreatedAt,
            CreatedBy = wasteReport.CreatedBy,
            Comments = new List<DTOs.WasteCommentDto>()
        };
    }
}