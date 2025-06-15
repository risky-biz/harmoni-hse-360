using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class UpdateWasteReportCommandHandler : IRequestHandler<UpdateWasteReportCommand, WasteReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateWasteReportCommandHandler> _logger;

    public UpdateWasteReportCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateWasteReportCommandHandler> logger)
    {
        _context = context;
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

        // Update using reflection or create an Update method in the entity
        var type = wasteReport.GetType();
        type.GetProperty("Title")?.SetValue(wasteReport, request.Title);
        type.GetProperty("Description")?.SetValue(wasteReport, request.Description);
        type.GetProperty("Category")?.SetValue(wasteReport, request.Category);
        type.GetProperty("GeneratedDate")?.SetValue(wasteReport, request.GeneratedDate);
        type.GetProperty("Location")?.SetValue(wasteReport, request.Location);
        type.GetProperty("LastModifiedAt")?.SetValue(wasteReport, DateTime.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Waste report {Id} updated successfully", wasteReport.Id);

        return new WasteReportDto
        {
            Id = wasteReport.Id,
            Title = wasteReport.Title,
            Description = wasteReport.Description,
            Category = wasteReport.Category.ToString(),
            Status = wasteReport.DisposalStatus.ToString(),
            GeneratedDate = wasteReport.GeneratedDate,
            Location = wasteReport.Location,
            ReporterId = wasteReport.ReporterId,
            ReporterName = wasteReport.Reporter?.Name,
            AttachmentsCount = wasteReport.Attachments?.Count ?? 0,
            CreatedAt = wasteReport.CreatedAt,
            CreatedBy = wasteReport.CreatedBy
        };
    }
}