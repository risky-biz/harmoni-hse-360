using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class UploadSafetyVideoCommandHandler : IRequestHandler<UploadSafetyVideoCommand, WorkPermitSafetyVideoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UploadSafetyVideoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WorkPermitSafetyVideoDto> Handle(UploadSafetyVideoCommand request, CancellationToken cancellationToken)
    {
        // Verify the Work Permit Settings exists
        var workPermitSettings = await _context.WorkPermitSettings
            .Include(wps => wps.SafetyVideos)
            .FirstOrDefaultAsync(wps => wps.Id == request.WorkPermitSettingsId, cancellationToken);

        if (workPermitSettings == null)
        {
            throw new KeyNotFoundException($"Work Permit Settings with ID {request.WorkPermitSettingsId} was not found.");
        }

        // Use the domain method to set the safety induction video
        workPermitSettings.SetSafetyInductionVideo(
            fileName: request.FileName,
            filePath: request.FilePath,
            fileSize: request.FileSize,
            contentType: request.ContentType,
            duration: request.Duration,
            uploadedBy: _currentUserService.Email
        );

        await _context.SaveChangesAsync(cancellationToken);

        // Get the newly created video
        var safetyVideo = workPermitSettings.ActiveSafetyVideo!;

        // Update metadata if provided
        if (!string.IsNullOrEmpty(request.Description) || !string.IsNullOrEmpty(request.ThumbnailPath) || 
            !string.IsNullOrEmpty(request.Resolution) || request.Bitrate.HasValue)
        {
            safetyVideo.UpdateMetadata(
                description: request.Description,
                thumbnailPath: request.ThumbnailPath,
                resolution: request.Resolution,
                bitrate: request.Bitrate,
                modifiedBy: _currentUserService.Email
            );
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new WorkPermitSafetyVideoDto
        {
            Id = safetyVideo.Id,
            FileName = safetyVideo.FileName,
            OriginalFileName = safetyVideo.OriginalFileName,
            FilePath = safetyVideo.FilePath,
            FileSize = safetyVideo.FileSize,
            ContentType = safetyVideo.ContentType,
            Duration = safetyVideo.Duration,
            IsActive = safetyVideo.IsActive,
            Description = safetyVideo.Description,
            ThumbnailPath = safetyVideo.ThumbnailPath,
            Resolution = safetyVideo.Resolution,
            Bitrate = safetyVideo.Bitrate,
            CreatedAt = safetyVideo.CreatedAt,
            CreatedBy = safetyVideo.CreatedBy,
            LastModifiedAt = safetyVideo.LastModifiedAt,
            LastModifiedBy = safetyVideo.LastModifiedBy
        };
    }
}