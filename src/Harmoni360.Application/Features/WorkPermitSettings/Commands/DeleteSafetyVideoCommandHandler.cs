using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class DeleteSafetyVideoCommandHandler : IRequestHandler<DeleteSafetyVideoCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteSafetyVideoCommandHandler> _logger;

    public DeleteSafetyVideoCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<DeleteSafetyVideoCommandHandler> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteSafetyVideoCommand request, CancellationToken cancellationToken)
    {
        var safetyVideo = await _context.WorkPermitSafetyVideos
            .FirstOrDefaultAsync(sv => sv.Id == request.VideoId, cancellationToken);

        if (safetyVideo == null)
        {
            throw new KeyNotFoundException($"Safety Video with ID {request.VideoId} was not found.");
        }

        // Remove the database record first
        _context.WorkPermitSafetyVideos.Remove(safetyVideo);
        await _context.SaveChangesAsync(cancellationToken);

        // Then try to delete the physical file (best effort)
        try
        {
            await _fileStorageService.DeleteAsync(safetyVideo.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file for video {VideoId} at path {FilePath}",
                request.VideoId, safetyVideo.FilePath);
            // Don't throw here - the database record is already deleted
        }

        return Unit.Value;
    }
}