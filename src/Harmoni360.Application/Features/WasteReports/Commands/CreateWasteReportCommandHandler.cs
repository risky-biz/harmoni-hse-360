using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class CreateWasteReportCommandHandler : IRequestHandler<CreateWasteReportCommand, WasteReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAntivirusScanner _antivirusScanner;
    private readonly ILogger<CreateWasteReportCommandHandler> _logger;

    public CreateWasteReportCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IAntivirusScanner antivirusScanner,
        ILogger<CreateWasteReportCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _antivirusScanner = antivirusScanner;
        _logger = logger;
    }

    public async Task<WasteReportDto> Handle(CreateWasteReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating waste report: {Title} for user: {UserId}", request.Title, _currentUserService.UserId);

            var reporter = request.ReporterId.HasValue ? 
                await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReporterId.Value, cancellationToken) : null;

            var waste = WasteReport.Create(
                request.Title, 
                request.Description, 
                request.Category, 
                request.GeneratedDate, 
                request.Location, 
                request.ReporterId, 
                _currentUserService.Email);
            
            _context.WasteReports.Add(waste);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.Attachments?.Any() == true)
            {
                _logger.LogInformation("Processing {Count} attachments for waste report {Id}", request.Attachments.Count(), waste.Id);
                
                foreach (var file in request.Attachments)
                {
                    try
                    {
                        await _antivirusScanner.ScanAsync(file.OpenReadStream(), cancellationToken);
                        var upload = await _fileStorageService.UploadAsync(
                            file.OpenReadStream(),
                            file.FileName,
                            file.ContentType,
                            "waste");
                        waste.AddAttachment(file.FileName, upload.FilePath, file.Length, _currentUserService.Email);
                        
                        _logger.LogInformation("Attachment {FileName} uploaded successfully", file.FileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process attachment {FileName}", file.FileName);
                        throw;
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Waste report created successfully with ID: {Id}", waste.Id);

            return new WasteReportDto
            {
                Id = waste.Id,
                Title = waste.Title,
                Description = waste.Description,
                Category = waste.Category.ToString(),
                Status = waste.DisposalStatus.ToString(),
                GeneratedDate = waste.GeneratedDate,
                Location = waste.Location,
                ReporterId = waste.ReporterId,
                ReporterName = reporter?.Name,
                AttachmentsCount = waste.Attachments.Count,
                CreatedAt = waste.CreatedAt,
                CreatedBy = waste.CreatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating waste report: {Title}", request.Title);
            throw;
        }
    }
}
