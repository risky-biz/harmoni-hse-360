using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.Incidents.Queries;

public class GetIncidentAttachmentsQueryHandler : IRequestHandler<GetIncidentAttachmentsQuery, List<IncidentAttachmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetIncidentAttachmentsQueryHandler> _logger;

    public GetIncidentAttachmentsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetIncidentAttachmentsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<IncidentAttachmentDto>> Handle(GetIncidentAttachmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving attachments for incident {IncidentId}", request.IncidentId);

            var attachments = await _context.IncidentAttachments
                .Where(a => a.IncidentId == request.IncidentId)
                .OrderBy(a => a.UploadedAt)
                .Select(a => new IncidentAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileSize = a.FileSize,
                    UploadedBy = a.UploadedBy,
                    UploadedAt = a.UploadedAt,
                    FileUrl = $"/api/incident/{request.IncidentId}/attachments/{a.Id}/download",
                    FileSizeFormatted = FormatFileSize(a.FileSize)
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {AttachmentCount} attachments for incident {IncidentId}",
                attachments.Count, request.IncidentId);

            return attachments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve attachments for incident {IncidentId}", request.IncidentId);
            throw;
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}