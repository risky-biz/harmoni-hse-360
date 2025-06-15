using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Entities.Waste;

public class CreateWasteReportCommandHandler : IRequestHandler<CreateWasteReportCommand, WasteReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAntivirusScanner _antivirusScanner;

    public CreateWasteReportCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IAntivirusScanner antivirusScanner)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _antivirusScanner = antivirusScanner;
    }

    public async Task<WasteReportDto> Handle(CreateWasteReportCommand request, CancellationToken cancellationToken)
    {
        var reporter = request.ReporterId.HasValue ? await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReporterId.Value, cancellationToken) : null;

        var waste = WasteReport.Create(request.Title, request.Description, request.Category, request.GeneratedDate, request.Location, request.ReporterId, _currentUserService.Email);
        _context.Add(waste);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.Attachments.Any())
        {
            foreach (var file in request.Attachments)
            {
                await _antivirusScanner.ScanAsync(file.OpenReadStream(), cancellationToken);
                var upload = await _fileStorageService.UploadAsync(
                    file.OpenReadStream(),
                    file.FileName,
                    file.ContentType,
                    "waste");
                waste.AddAttachment(file.FileName, upload.FilePath, file.Length, _currentUserService.Email);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new WasteReportDto
        {
            Id = waste.Id,
            Title = waste.Title,
            Description = waste.Description,
            Category = waste.Category.ToString(),
            GeneratedDate = waste.GeneratedDate,
            Location = waste.Location,
            ReporterId = waste.ReporterId,
            ReporterName = reporter?.Name,
            AttachmentsCount = waste.Attachments.Count
        };
    }
}
