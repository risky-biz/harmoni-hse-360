using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Features.WorkPermits.Commands;

public class UploadWorkPermitAttachmentCommandHandler : IRequestHandler<UploadWorkPermitAttachmentCommand, WorkPermitAttachmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UploadWorkPermitAttachmentCommandHandler> _logger;

    public UploadWorkPermitAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UploadWorkPermitAttachmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<WorkPermitAttachmentDto> Handle(UploadWorkPermitAttachmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing upload attachment for work permit {WorkPermitId}", request.WorkPermitId);

        // Verify work permit exists
        var workPermit = await _context.WorkPermits
            .FirstOrDefaultAsync(wp => wp.Id == request.WorkPermitId, cancellationToken);

        if (workPermit == null)
        {
            _logger.LogWarning("Work permit {WorkPermitId} not found", request.WorkPermitId);
            throw new InvalidOperationException($"Work permit with ID {request.WorkPermitId} not found");
        }

        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            _logger.LogWarning("No file provided or file is empty");
            throw new ArgumentException("No file provided or file is empty");
        }

        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "work-permits", request.WorkPermitId.ToString());
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        // Generate unique filename
        var fileExtension = Path.GetExtension(request.File.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        try
        {
            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("File saved to {FilePath}", filePath);

            // Create attachment entity
            var currentUser = _currentUserService.Name ?? "System";
            var attachment = WorkPermitAttachment.Create(
                workPermitId: request.WorkPermitId,
                fileName: uniqueFileName,
                originalFileName: request.File.FileName,
                contentType: request.File.ContentType,
                fileSize: request.File.Length,
                filePath: filePath,
                uploadedBy: currentUser,
                attachmentType: request.AttachmentType,
                description: request.Description ?? string.Empty
            );

            // Add to database
            _context.WorkPermitAttachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Attachment {AttachmentId} uploaded successfully for work permit {WorkPermitId}", 
                attachment.Id, request.WorkPermitId);

            // Return DTO
            return new WorkPermitAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedBy = attachment.UploadedBy,
                UploadedAt = attachment.UploadedAt,
                AttachmentType = attachment.AttachmentType.ToString(),
                Description = attachment.Description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for work permit {WorkPermitId}", request.WorkPermitId);
            
            // Clean up file if it was created but database operation failed
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to clean up file {FilePath} after database error", filePath);
                }
            }
            
            throw;
        }
    }
}