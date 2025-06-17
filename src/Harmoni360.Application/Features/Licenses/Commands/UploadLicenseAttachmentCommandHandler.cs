using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class UploadLicenseAttachmentCommandHandler : IRequestHandler<UploadLicenseAttachmentCommand, LicenseAttachmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UploadLicenseAttachmentCommandHandler> _logger;

    public UploadLicenseAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UploadLicenseAttachmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LicenseAttachmentDto> Handle(UploadLicenseAttachmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Verify license exists
            var license = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Id == request.LicenseId, cancellationToken);

            if (license == null)
            {
                throw new KeyNotFoundException($"License with ID {request.LicenseId} not found.");
            }

            // Generate file path and unique filename
            var fileExtension = Path.GetExtension(request.File.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine("uploads", "licenses", request.LicenseId.ToString(), fileName);

            // Ensure directory exists
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(filePath)!);
            Directory.CreateDirectory(directoryPath);

            // Save file to disk
            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            // Create attachment entity
            var attachment = LicenseAttachment.Create(
                licenseId: request.LicenseId,
                fileName: fileName,
                originalFileName: request.File.FileName,
                contentType: request.File.ContentType,
                fileSize: request.File.Length,
                filePath: filePath,
                uploadedBy: currentUser.Name,
                attachmentType: request.AttachmentType,
                description: request.Description,
                isRequired: request.IsRequired,
                validUntil: request.ValidUntil
            );

            _context.LicenseAttachments.Add(attachment);

            // Add audit log for attachment upload
            var attachmentDetails = $"File: {request.File.FileName}, Type: {request.AttachmentType}, Size: {request.File.Length} bytes";
            license.LogAuditAction(
                LicenseAuditAction.AttachmentAdded,
                $"Uploaded attachment: {attachmentDetails}",
                currentUser.Name);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License attachment uploaded successfully. LicenseId: {LicenseId}, AttachmentId: {AttachmentId}, FileName: {FileName}", 
                request.LicenseId, attachment.Id, request.File.FileName);

            // Return DTO
            return new LicenseAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedBy = attachment.UploadedBy,
                UploadedAt = attachment.UploadedAt,
                AttachmentType = attachment.AttachmentType.ToString(),
                AttachmentTypeDisplay = GetAttachmentTypeDisplay(attachment.AttachmentType),
                Description = attachment.Description,
                IsRequired = attachment.IsRequired,
                ValidUntil = attachment.ValidUntil
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for license {LicenseId} by user {UserId}", request.LicenseId, _currentUserService.UserId);
            throw;
        }
    }

    private static string GetAttachmentTypeDisplay(Domain.Enums.LicenseAttachmentType type) => type switch
    {
        Domain.Enums.LicenseAttachmentType.Application => "Application Document",
        Domain.Enums.LicenseAttachmentType.SupportingDocument => "Supporting Document", 
        Domain.Enums.LicenseAttachmentType.Certificate => "Certificate",
        Domain.Enums.LicenseAttachmentType.Compliance => "Compliance Document",
        Domain.Enums.LicenseAttachmentType.Insurance => "Insurance Document",
        Domain.Enums.LicenseAttachmentType.TechnicalSpec => "Technical Specification",
        Domain.Enums.LicenseAttachmentType.LegalDocument => "Legal Document",
        Domain.Enums.LicenseAttachmentType.RenewalDocument => "Renewal Document",
        Domain.Enums.LicenseAttachmentType.InspectionReport => "Inspection Report",
        Domain.Enums.LicenseAttachmentType.Other => "Other",
        _ => type.ToString()
    };
}