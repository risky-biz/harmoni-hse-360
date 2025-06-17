using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class DeleteLicenseAttachmentCommandHandler : IRequestHandler<DeleteLicenseAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteLicenseAttachmentCommandHandler> _logger;

    public DeleteLicenseAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DeleteLicenseAttachmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DeleteLicenseAttachmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the license for audit logging
            var license = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Id == request.LicenseId, cancellationToken);

            if (license == null)
            {
                throw new KeyNotFoundException($"License with ID {request.LicenseId} not found.");
            }

            // Get the attachment
            var attachment = await _context.LicenseAttachments
                .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.LicenseId == request.LicenseId, cancellationToken);

            if (attachment == null)
            {
                throw new KeyNotFoundException($"License attachment with ID {request.AttachmentId} not found for license {request.LicenseId}.");
            }

            // Store attachment details for audit before deletion
            var attachmentDetails = $"File: {attachment.OriginalFileName}, Type: {attachment.AttachmentType}, Size: {attachment.FileSize} bytes";

            // Delete physical file
            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), attachment.FilePath);
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                _logger.LogInformation("Physical file deleted: {FilePath}", attachment.FilePath);
            }

            // Remove attachment from database
            _context.LicenseAttachments.Remove(attachment);

            // Add audit log for attachment deletion
            var currentUser = _currentUserService.Email ?? "System";
            license.LogAuditAction(
                LicenseAuditAction.AttachmentRemoved,
                $"Deleted attachment: {attachmentDetails}",
                currentUser);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License attachment deleted successfully. LicenseId: {LicenseId}, AttachmentId: {AttachmentId}", 
                request.LicenseId, request.AttachmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId} for license {LicenseId}", 
                request.AttachmentId, request.LicenseId);
            throw;
        }
    }
}