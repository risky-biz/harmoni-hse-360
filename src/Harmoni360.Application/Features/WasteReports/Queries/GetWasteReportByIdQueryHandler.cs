using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteReportByIdQueryHandler : IRequestHandler<GetWasteReportByIdQuery, WasteReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetWasteReportByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WasteReportDto> Handle(GetWasteReportByIdQuery request, CancellationToken cancellationToken)
    {
        var wasteReport = await _context.WasteReports
            .Include(w => w.Attachments)
            .Include(w => w.Reporter)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (wasteReport == null)
        {
            throw new InvalidOperationException($"Waste report with ID {request.Id} not found");
        }

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