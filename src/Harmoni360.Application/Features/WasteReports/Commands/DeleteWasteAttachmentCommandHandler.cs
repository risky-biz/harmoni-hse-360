using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class DeleteWasteAttachmentCommandHandler : IRequestHandler<DeleteWasteAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteWasteAttachmentCommandHandler> _logger;

    public DeleteWasteAttachmentCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteWasteAttachmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeleteWasteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _context.WasteAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken);

        if (attachment == null)
        {
            throw new InvalidOperationException($"Attachment with ID {request.AttachmentId} not found");
        }

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted file: {FilePath}", fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {FilePath}", fullPath);
            }
        }

        _context.WasteAttachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}