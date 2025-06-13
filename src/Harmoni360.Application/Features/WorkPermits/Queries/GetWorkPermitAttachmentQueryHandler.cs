using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.WorkPermits.Queries;

public class GetWorkPermitAttachmentQueryHandler : IRequestHandler<GetWorkPermitAttachmentQuery, WorkPermitAttachmentFileResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetWorkPermitAttachmentQueryHandler> _logger;

    public GetWorkPermitAttachmentQueryHandler(
        IApplicationDbContext context,
        ILogger<GetWorkPermitAttachmentQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkPermitAttachmentFileResult> Handle(GetWorkPermitAttachmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving attachment {AttachmentId} for work permit {WorkPermitId}", 
            request.AttachmentId, request.WorkPermitId);

        var attachment = await _context.WorkPermitAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.WorkPermitId == request.WorkPermitId, cancellationToken);

        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found for work permit {WorkPermitId}", 
                request.AttachmentId, request.WorkPermitId);
            return new WorkPermitAttachmentFileResult();
        }

        try
        {
            // Read file from storage
            if (!File.Exists(attachment.FilePath))
            {
                _logger.LogError("File not found at path: {FilePath}", attachment.FilePath);
                return new WorkPermitAttachmentFileResult();
            }

            var fileContent = await File.ReadAllBytesAsync(attachment.FilePath, cancellationToken);

            return new WorkPermitAttachmentFileResult
            {
                FileContent = fileContent,
                ContentType = attachment.ContentType,
                FileName = attachment.OriginalFileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading attachment file: {FilePath}", attachment.FilePath);
            return new WorkPermitAttachmentFileResult();
        }
    }
}