using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Licenses.Queries;

public class GetLicenseAttachmentQueryHandler : IRequestHandler<GetLicenseAttachmentQuery, LicenseAttachmentFileResult?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetLicenseAttachmentQueryHandler> _logger;

    public GetLicenseAttachmentQueryHandler(
        IApplicationDbContext context,
        ILogger<GetLicenseAttachmentQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LicenseAttachmentFileResult?> Handle(GetLicenseAttachmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the attachment
            var attachment = await _context.LicenseAttachments
                .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.LicenseId == request.LicenseId, cancellationToken);

            if (attachment == null)
            {
                _logger.LogWarning("License attachment not found. LicenseId: {LicenseId}, AttachmentId: {AttachmentId}", 
                    request.LicenseId, request.AttachmentId);
                return null;
            }

            // Get file path
            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), attachment.FilePath);
            
            if (!File.Exists(fullFilePath))
            {
                _logger.LogWarning("Physical file not found for attachment. AttachmentId: {AttachmentId}, FilePath: {FilePath}", 
                    request.AttachmentId, attachment.FilePath);
                return null;
            }

            // Read file content
            var fileContent = await File.ReadAllBytesAsync(fullFilePath, cancellationToken);

            _logger.LogInformation("License attachment downloaded successfully. LicenseId: {LicenseId}, AttachmentId: {AttachmentId}", 
                request.LicenseId, request.AttachmentId);

            return new LicenseAttachmentFileResult
            {
                Content = fileContent,
                ContentType = attachment.ContentType,
                FileName = attachment.OriginalFileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId} for license {LicenseId}", 
                request.AttachmentId, request.LicenseId);
            throw;
        }
    }
}