using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteReportsQueryHandler : IRequestHandler<GetWasteReportsQuery, List<WasteReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWasteReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WasteReportDto>> Handle(GetWasteReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WasteReports
            .Include(w => w.Reporter)
            .AsQueryable();

        if (request.Category.HasValue)
        {
            query = query.Where(w => w.Category == request.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(w => w.Title.Contains(request.Search!) || w.Description.Contains(request.Search!));
        }

        return await query
            .OrderByDescending(w => w.GeneratedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WasteReportDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Category = w.Category.ToString(),
                GeneratedDate = w.GeneratedDate,
                Location = w.Location,
                ReporterId = w.ReporterId,
                ReporterName = w.Reporter != null ? w.Reporter.Name : null,
                AttachmentsCount = w.Attachments.Count
            })
            .ToListAsync(cancellationToken);
    }
}
