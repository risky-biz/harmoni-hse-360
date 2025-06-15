using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteAttachmentQueryHandler : IRequestHandler<GetWasteAttachmentQuery, WasteAttachmentDownloadDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetWasteAttachmentQueryHandler> _logger;

    public GetWasteAttachmentQueryHandler(
        IApplicationDbContext context,
        ILogger<GetWasteAttachmentQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WasteAttachmentDownloadDto> Handle(GetWasteAttachmentQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _context.WasteAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken);

        if (attachment == null)
        {
            throw new InvalidOperationException($"Attachment with ID {request.AttachmentId} not found");
        }

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {attachment.FileName}");
        }

        var fileData = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        var contentType = GetContentType(attachment.FileName);

        return new WasteAttachmentDownloadDto
        {
            FileName = attachment.FileName,
            ContentType = contentType,
            FileData = fileData
        };
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}