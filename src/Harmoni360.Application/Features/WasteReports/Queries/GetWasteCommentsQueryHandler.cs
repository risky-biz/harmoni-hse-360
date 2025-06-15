using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.Commands;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteCommentsQueryHandler : IRequestHandler<GetWasteCommentsQuery, List<WasteCommentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWasteCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WasteCommentDto>> Handle(GetWasteCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.WasteComments
            .Include(c => c.CommentedBy)
            .Where(c => c.WasteReportId == request.WasteReportId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new WasteCommentDto
            {
                Id = c.Id,
                WasteReportId = c.WasteReportId,
                Comment = c.Comment,
                Type = c.Type.ToString(),
                CommentedById = c.CommentedById,
                CommentedByName = c.CommentedBy.Name,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }
}