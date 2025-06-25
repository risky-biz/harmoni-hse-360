using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class DeleteInspectionAttachmentCommandHandler : IRequestHandler<DeleteInspectionAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteInspectionAttachmentCommandHandler> _logger;

    public DeleteInspectionAttachmentCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteInspectionAttachmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeleteInspectionAttachmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting attachment {AttachmentId} from inspection {InspectionId}", 
            request.AttachmentId, request.InspectionId);

        // Find attachment
        var attachment = await _context.InspectionAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.InspectionId == request.InspectionId, cancellationToken);

        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found for inspection {InspectionId}", 
                request.AttachmentId, request.InspectionId);
            throw new InvalidOperationException($"Attachment with ID {request.AttachmentId} not found");
        }

        try
        {
            // Delete file from disk
            if (File.Exists(attachment.FilePath))
            {
                File.Delete(attachment.FilePath);
                _logger.LogInformation("File deleted from disk: {FilePath}", attachment.FilePath);
            }

            // Delete thumbnail if exists
            if (!string.IsNullOrEmpty(attachment.ThumbnailPath) && File.Exists(attachment.ThumbnailPath))
            {
                File.Delete(attachment.ThumbnailPath);
                _logger.LogInformation("Thumbnail deleted from disk: {ThumbnailPath}", attachment.ThumbnailPath);
            }

            // Remove from database
            _context.InspectionAttachments.Remove(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Attachment {AttachmentId} deleted successfully from inspection {InspectionId}", 
                request.AttachmentId, request.InspectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId} from inspection {InspectionId}", 
                request.AttachmentId, request.InspectionId);
            throw;
        }
    }
}