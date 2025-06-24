using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class AddWasteCommentCommandHandler : IRequestHandler<AddWasteCommentCommand, WasteCommentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IWasteAuditService _auditService;

    public AddWasteCommentCommandHandler(IApplicationDbContext context, IWasteAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<WasteCommentDto> Handle(AddWasteCommentCommand request, CancellationToken cancellationToken)
    {
        var wasteReport = await _context.WasteReports
            .FirstOrDefaultAsync(w => w.Id == request.WasteReportId, cancellationToken);

        if (wasteReport == null)
        {
            throw new InvalidOperationException($"Waste report with ID {request.WasteReportId} not found");
        }

        var comment = WasteComment.Create(
            request.WasteReportId,
            request.CommentedById,
            request.Comment,
            request.Type,
            "current-user");

        _context.WasteComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        // Log comment addition
        await _auditService.LogCommentAsync(request.WasteReportId, "Added", request.Comment);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.CommentedById, cancellationToken);

        return new WasteCommentDto
        {
            Id = comment.Id,
            WasteReportId = comment.WasteReportId,
            Comment = comment.Comment,
            Type = comment.Type.ToString(),
            CommentedById = comment.CommentedById,
            CommentedByName = user?.Name ?? "Unknown",
            CreatedAt = comment.CreatedAt,
            CreatedBy = comment.CreatedBy
        };
    }
}