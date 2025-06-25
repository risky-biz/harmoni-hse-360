using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Inspections.Queries;

public class GetInspectionAttachmentQueryHandler : IRequestHandler<GetInspectionAttachmentQuery, InspectionAttachmentFileResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetInspectionAttachmentQueryHandler> _logger;

    public GetInspectionAttachmentQueryHandler(
        IApplicationDbContext context,
        ILogger<GetInspectionAttachmentQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InspectionAttachmentFileResult> Handle(GetInspectionAttachmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting attachment {AttachmentId} from inspection {InspectionId}", 
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

        if (!File.Exists(attachment.FilePath))
        {
            _logger.LogWarning("File not found on disk: {FilePath}", attachment.FilePath);
            throw new InvalidOperationException("File not found on disk");
        }

        try
        {
            var fileContent = await File.ReadAllBytesAsync(attachment.FilePath, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved attachment {AttachmentId} from inspection {InspectionId}", 
                request.AttachmentId, request.InspectionId);

            return new InspectionAttachmentFileResult
            {
                FileContent = fileContent,
                ContentType = attachment.ContentType,
                FileName = attachment.OriginalFileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachment {AttachmentId} from inspection {InspectionId}", 
                request.AttachmentId, request.InspectionId);
            throw;
        }
    }
}