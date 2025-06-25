using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Inspections.DTOs;
using Harmoni360.Domain.Entities.Inspections;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class UploadInspectionAttachmentCommandHandler : IRequestHandler<UploadInspectionAttachmentCommand, InspectionAttachmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UploadInspectionAttachmentCommandHandler> _logger;

    public UploadInspectionAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UploadInspectionAttachmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<InspectionAttachmentDto> Handle(UploadInspectionAttachmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing upload attachment for inspection {InspectionId}", request.InspectionId);

        // Verify inspection exists
        var inspection = await _context.Inspections
            .FirstOrDefaultAsync(i => i.Id == request.InspectionId, cancellationToken);

        if (inspection == null)
        {
            _logger.LogWarning("Inspection {InspectionId} not found", request.InspectionId);
            throw new InvalidOperationException($"Inspection with ID {request.InspectionId} not found");
        }

        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            _logger.LogWarning("No file provided or file is empty");
            throw new ArgumentException("No file provided or file is empty");
        }

        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "inspections", request.InspectionId.ToString());
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
            var attachment = InspectionAttachment.Create(
                inspectionId: request.InspectionId,
                fileName: uniqueFileName,
                originalFileName: request.File.FileName,
                contentType: request.File.ContentType,
                fileSize: request.File.Length,
                filePath: filePath,
                description: request.Description,
                category: request.Category
            );

            // Set audit fields
            attachment.GetType()
                .GetProperty("CreatedBy")
                ?.SetValue(attachment, currentUser);

            // Add to database
            _context.InspectionAttachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Attachment {AttachmentId} uploaded successfully for inspection {InspectionId}", 
                attachment.Id, request.InspectionId);

            // Return DTO
            return new InspectionAttachmentDto
            {
                Id = attachment.Id,
                InspectionId = attachment.InspectionId,
                FileName = attachment.FileName,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                FileSizeFormatted = attachment.GetFileSizeFormatted(),
                FilePath = attachment.FilePath,
                Description = attachment.Description,
                Category = attachment.Category,
                IsPhoto = attachment.IsPhoto,
                ThumbnailPath = attachment.ThumbnailPath,
                IsDocument = attachment.IsDocument,
                FileExtension = attachment.FileExtension,
                CreatedAt = attachment.CreatedAt,
                LastModifiedAt = attachment.LastModifiedAt,
                CreatedBy = attachment.CreatedBy,
                LastModifiedBy = attachment.LastModifiedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for inspection {InspectionId}", request.InspectionId);
            
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