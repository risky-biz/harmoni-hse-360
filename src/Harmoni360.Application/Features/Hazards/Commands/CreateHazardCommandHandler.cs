using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Domain.Events;

namespace Harmoni360.Application.Features.Hazards.Commands;

public class CreateHazardCommandHandler : IRequestHandler<CreateHazardCommand, HazardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public CreateHazardCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<HazardDto> Handle(CreateHazardCommand request, CancellationToken cancellationToken)
    {
        // Create GeoLocation value object if coordinates provided
        GeoLocation? geoLocation = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            geoLocation = GeoLocation.Create(request.Latitude.Value, request.Longitude.Value);
        }

        // Get reporter information
        var reporter = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ReporterId, cancellationToken);

        if (reporter == null)
        {
            throw new ArgumentException($"Reporter with ID {request.ReporterId} not found.");
        }

        // Create the hazard entity using static factory method
        var hazard = Hazard.Create(
            title: request.Title,
            description: request.Description,
            category: request.Category,
            type: request.Type,
            location: request.Location,
            severity: request.Severity,
            reporterId: request.ReporterId,
            reporterDepartment: request.ReporterDepartment,
            geoLocation: geoLocation
        );

        // Set expected resolution date if provided
        if (request.ExpectedResolutionDate.HasValue)
        {
            hazard.SetExpectedResolutionDate(request.ExpectedResolutionDate.Value);
        }

        // Add hazard to context
        _context.Hazards.Add(hazard);

        // Process file attachments
        var attachments = new List<HazardAttachment>();
        if (request.Attachments?.Any() == true)
        {
            foreach (var file in request.Attachments)
            {
                if (file.Length > 0)
                {
                    var uploadResult = await _fileStorageService.UploadAsync(
                        file.OpenReadStream(),
                        file.FileName,
                        file.ContentType,
                        "hazards");

                    var attachment = new HazardAttachment(
                        hazardId: 0, // Will be set after saving hazard
                        fileName: file.FileName,
                        filePath: uploadResult.FilePath,
                        fileSize: file.Length,
                        uploadedBy: _currentUserService.Name
                    );

                    attachments.Add(attachment);
                }
            }
        }

        // Save changes to get hazard ID
        await _context.SaveChangesAsync(cancellationToken);

        // Update attachment hazard IDs and add to context
        if (attachments.Any())
        {
            foreach (var attachment in attachments)
            {
                hazard.AddAttachment(attachment.FileName, attachment.FilePath, attachment.FileSize, attachment.UploadedBy);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Save final changes to persist domain events
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var hazardDto = new HazardDto
        {
            Id = hazard.Id,
            Title = hazard.Title,
            Description = hazard.Description,
            Category = hazard.Category.ToString(),
            Type = hazard.Type.ToString(),
            Location = hazard.Location,
            Latitude = hazard.GeoLocation?.Latitude,
            Longitude = hazard.GeoLocation?.Longitude,
            Status = hazard.Status.ToString(),
            Severity = hazard.Severity.ToString(),
            IdentifiedDate = hazard.IdentifiedDate,
            ExpectedResolutionDate = hazard.ExpectedResolutionDate,
            ReporterName = reporter.Name,
            ReporterEmail = reporter.Email,
            ReporterDepartment = hazard.ReporterDepartment,
            AttachmentsCount = attachments.Count,
            RiskAssessmentsCount = 0,
            MitigationActionsCount = 0,
            PendingActionsCount = 0,
            CreatedAt = hazard.CreatedAt,
            CreatedBy = hazard.CreatedBy,
            LastModifiedAt = hazard.LastModifiedAt,
            LastModifiedBy = hazard.LastModifiedBy,
            Reporter = new UserDto
            {
                Id = reporter.Id,
                Name = reporter.Name,
                Email = reporter.Email,
                Department = reporter.Department,
                Position = reporter.Position,
                EmployeeId = reporter.EmployeeId
            }
        };

        return hazardDto;
    }
}