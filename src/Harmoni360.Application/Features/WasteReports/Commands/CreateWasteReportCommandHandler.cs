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
    private readonly IWasteAuditService _auditService;
    private readonly ILogger<CreateWasteReportCommandHandler> _logger;

    public CreateWasteReportCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IAntivirusScanner antivirusScanner,
        IWasteAuditService auditService,
        ILogger<CreateWasteReportCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _antivirusScanner = antivirusScanner;
        _auditService = auditService;
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
                _currentUserService.Email,
                request.EstimatedQuantity,
                request.QuantityUnit,
                request.DisposalMethod,
                request.DisposalDate,
                request.DisposalCost,
                request.ContractorName,
                request.ManifestNumber,
                request.Treatment,
                request.Notes);
            
            _context.WasteReports.Add(waste);
            await _context.SaveChangesAsync(cancellationToken);

            // Log waste report creation
            await _auditService.LogWasteReportCreatedAsync(waste);

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
                        
                        // Log attachment upload
                        await _auditService.LogAttachmentAsync(waste.Id, "Added", file.FileName);
                        
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
                Classification = (Domain.Enums.WasteClassification)(int)waste.Category,
                ClassificationDisplay = waste.Category.ToString(),
                Status = Domain.Enums.WasteReportStatus.Draft,
                StatusDisplay = waste.DisposalStatus.ToString(),
                ReportDate = waste.GeneratedDate,
                ReportedBy = reporter?.Name ?? "Unknown",
                Location = waste.Location,
                EstimatedQuantity = waste.EstimatedQuantity,
                QuantityUnit = waste.QuantityUnit,
                DisposalMethod = waste.DisposalMethod,
                DisposalDate = waste.DisposalDate,
                DisposedBy = waste.DisposedBy,
                DisposalCost = waste.DisposalCost,
                ContractorName = waste.ContractorName,
                ManifestNumber = waste.ManifestNumber,
                Treatment = waste.Treatment,
                Notes = waste.Notes,
                CreatedAt = waste.CreatedAt,
                CreatedBy = waste.CreatedBy,
                Comments = new List<DTOs.WasteCommentDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating waste report: {Title}", request.Title);
            throw;
        }
    }
}
