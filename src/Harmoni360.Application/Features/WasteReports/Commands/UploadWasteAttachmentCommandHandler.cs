using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class UploadWasteAttachmentCommandHandler : IRequestHandler<UploadWasteAttachmentCommand, WasteAttachmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UploadWasteAttachmentCommandHandler> _logger;

    public UploadWasteAttachmentCommandHandler(
        IApplicationDbContext context,
        ILogger<UploadWasteAttachmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WasteAttachmentDto> Handle(UploadWasteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var wasteReport = await _context.WasteReports
            .FirstOrDefaultAsync(w => w.Id == request.WasteReportId, cancellationToken);

        if (wasteReport == null)
        {
            throw new InvalidOperationException($"Waste report with ID {request.WasteReportId} not found");
        }

        var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "waste-attachments");
        if (!Directory.Exists(uploadsDirectory))
        {
            Directory.CreateDirectory(uploadsDirectory);
        }

        var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
        var filePath = Path.Combine(uploadsDirectory, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine("uploads", "waste-attachments", fileName);
        
        wasteReport.AddAttachment(request.File.FileName, relativePath, request.File.Length, "current-user");

        await _context.SaveChangesAsync(cancellationToken);

        var attachment = wasteReport.Attachments.Last();
        
        return new WasteAttachmentDto
        {
            Id = attachment.Id,
            WasteReportId = attachment.WasteReportId,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            FileSize = attachment.FileSize,
            UploadedBy = attachment.UploadedBy,
            UploadedAt = attachment.UploadedAt
        };
    }
}