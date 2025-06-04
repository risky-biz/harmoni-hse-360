using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class AddIncidentAttachmentsCommandHandler : IRequestHandler<AddIncidentAttachmentsCommand, AddIncidentAttachmentsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AddIncidentAttachmentsCommandHandler> _logger;
    private readonly IIncidentAuditService _auditService;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt", ".mp4", ".avi", ".mov" };
    private static readonly string[] AllowedContentTypes = {
        "image/jpeg", "image/png", "image/gif",
        "application/pdf",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain",
        "video/mp4", "video/avi", "video/quicktime"
    };
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

    public AddIncidentAttachmentsCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<AddIncidentAttachmentsCommandHandler> logger,
        IIncidentAuditService auditService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<AddIncidentAttachmentsResult> Handle(AddIncidentAttachmentsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding {FileCount} attachments to incident {IncidentId}",
                request.Files.Count, request.IncidentId);

            // Verify incident exists
            var incident = await _context.Incidents
                .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken);

            if (incident == null)
            {
                throw new InvalidOperationException($"Incident with ID {request.IncidentId} not found");
            }

            var attachments = new List<AttachmentInfo>();

            foreach (var file in request.Files)
            {
                // Validate file
                ValidateFile(file);

                // Generate unique file name to prevent conflicts
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var folder = $"incidents/{request.IncidentId}/attachments";

                // Upload file
                using var fileStream = file.OpenReadStream();
                var uploadResult = await _fileStorageService.UploadAsync(
                    fileStream,
                    uniqueFileName,
                    file.ContentType,
                    folder);

                // Create attachment entity
                var attachment = new IncidentAttachment(
                    request.IncidentId,
                    file.FileName, // Original filename
                    uploadResult.FilePath, // Storage path
                    uploadResult.Size,
                    request.UploadedBy);

                _context.IncidentAttachments.Add(attachment);

                // Save to get the ID
                await _context.SaveChangesAsync(cancellationToken);

                // Log audit trail for attachment
                await _auditService.LogAttachmentAddedAsync(request.IncidentId, file.FileName);

                // Save audit trail entry
                await _context.SaveChangesAsync(cancellationToken);

                attachments.Add(new AttachmentInfo
                {
                    Id = attachment.Id,
                    FileName = attachment.FileName,
                    FilePath = attachment.FilePath,
                    FileSize = attachment.FileSize,
                    UploadedAt = attachment.UploadedAt
                });

                _logger.LogInformation("Successfully uploaded attachment {FileName} ({FileSize} bytes) for incident {IncidentId}",
                    file.FileName, uploadResult.Size, request.IncidentId);
            }

            return new AddIncidentAttachmentsResult
            {
                Attachments = attachments,
                Message = $"Successfully uploaded {attachments.Count} file(s)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add attachments to incident {IncidentId}", request.IncidentId);
            throw;
        }
    }

    private static void ValidateFile(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File '{file.FileName}' exceeds maximum size of {MaxFileSize / (1024 * 1024)}MB");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException($"Content type '{file.ContentType}' is not allowed");
        }

        // Additional security check - validate file signature/magic bytes
        if (!IsValidFileSignature(file))
        {
            throw new ArgumentException($"File '{file.FileName}' has invalid or suspicious content");
        }
    }

    private static bool IsValidFileSignature(Microsoft.AspNetCore.Http.IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            // Check common file signatures (magic bytes)
            var signature = BitConverter.ToString(buffer).Replace("-", "");

            return file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => signature.StartsWith("FFD8FF"),
                "image/png" => signature.StartsWith("89504E47"),
                "image/gif" => signature.StartsWith("474946"),
                "application/pdf" => signature.StartsWith("255044462D"),
                "text/plain" => true, // Text files don't have specific signatures
                _ => true // For other types, we rely on content type validation
            };
        }
        catch
        {
            return false;
        }
    }
}