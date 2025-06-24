using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetMyWasteReportsQueryHandler : IRequestHandler<GetMyWasteReportsQuery, PagedList<WasteReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyWasteReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<WasteReportDto>> Handle(GetMyWasteReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WasteReports
            .Include(w => w.Reporter)
            .Include(w => w.Attachments)
            .Where(w => w.ReporterId == request.UserId)
            .OrderByDescending(w => w.GeneratedDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WasteReportDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Classification = (Domain.Enums.WasteClassification)(int)w.Category,
                ClassificationDisplay = w.Category.ToString(),
                Status = Domain.Enums.WasteReportStatus.Draft, 
                StatusDisplay = w.DisposalStatus.ToString(),
                ReportDate = w.GeneratedDate,
                ReportedBy = w.Reporter != null ? w.Reporter.Name : "Unknown",
                Location = w.Location,
                CreatedAt = w.CreatedAt,
                CreatedBy = w.CreatedBy,
                Comments = new List<DTOs.WasteCommentDto>()
            })
            .ToListAsync(cancellationToken);

        return new PagedList<WasteReportDto>(items, totalCount, request.Page, request.PageSize);
    }
}