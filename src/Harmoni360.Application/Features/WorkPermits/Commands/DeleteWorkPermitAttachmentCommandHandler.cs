using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.WorkPermits.Commands;

public class DeleteWorkPermitAttachmentCommandHandler : IRequestHandler<DeleteWorkPermitAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteWorkPermitAttachmentCommandHandler> _logger;

    public DeleteWorkPermitAttachmentCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteWorkPermitAttachmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeleteWorkPermitAttachmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting attachment {AttachmentId} from work permit {WorkPermitId}", 
            request.AttachmentId, request.WorkPermitId);

        var attachment = await _context.WorkPermitAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.WorkPermitId == request.WorkPermitId, 
                cancellationToken);

        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found for work permit {WorkPermitId}", 
                request.AttachmentId, request.WorkPermitId);
            throw new InvalidOperationException($"Attachment with ID {request.AttachmentId} not found for work permit {request.WorkPermitId}");
        }

        try
        {
            // Remove from database first
            _context.WorkPermitAttachments.Remove(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            // Then delete the physical file
            if (File.Exists(attachment.FilePath))
            {
                File.Delete(attachment.FilePath);
                _logger.LogInformation("Physical file deleted: {FilePath}", attachment.FilePath);
            }
            else
            {
                _logger.LogWarning("Physical file not found: {FilePath}", attachment.FilePath);
            }

            _logger.LogInformation("Attachment {AttachmentId} deleted successfully from work permit {WorkPermitId}", 
                request.AttachmentId, request.WorkPermitId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId} from work permit {WorkPermitId}", 
                request.AttachmentId, request.WorkPermitId);
            throw;
        }
    }
}