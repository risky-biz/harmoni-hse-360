using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.WorkPermitSettings.Queries;

public class GetActiveWorkPermitSettingQuery : IRequest<WorkPermitSettingDto?>
{
    public bool IncludeSafetyVideos { get; set; } = true;
}

public class GetActiveWorkPermitSettingQueryHandler : IRequestHandler<GetActiveWorkPermitSettingQuery, WorkPermitSettingDto?>
{
    private readonly IApplicationDbContext _context;

    public GetActiveWorkPermitSettingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkPermitSettingDto?> Handle(GetActiveWorkPermitSettingQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkPermitSettings.AsQueryable();

        // Include safety videos if requested
        if (request.IncludeSafetyVideos)
        {
            query = query.Include(wps => wps.SafetyVideos);
        }

        var setting = await query
            .Where(wps => wps.IsActive)
            .Select(wps => new WorkPermitSettingDto
            {
                Id = wps.Id,
                RequireSafetyInduction = wps.RequireSafetyInduction,
                EnableFormValidation = wps.EnableFormValidation,
                AllowAttachments = wps.AllowAttachments,
                MaxAttachmentSizeMB = wps.MaxAttachmentSizeMB,
                FormInstructions = wps.FormInstructions,
                IsActive = wps.IsActive,
                SafetyVideos = request.IncludeSafetyVideos 
                    ? wps.SafetyVideos.Select(sv => new WorkPermitSafetyVideoDto
                    {
                        Id = sv.Id,
                        FileName = sv.FileName,
                        OriginalFileName = sv.OriginalFileName,
                        FilePath = sv.FilePath,
                        FileSize = sv.FileSize,
                        ContentType = sv.ContentType,
                        Duration = sv.Duration,
                        IsActive = sv.IsActive,
                        Description = sv.Description,
                        ThumbnailPath = sv.ThumbnailPath,
                        Resolution = sv.Resolution,
                        Bitrate = sv.Bitrate,
                        CreatedAt = sv.CreatedAt,
                        CreatedBy = sv.CreatedBy,
                        LastModifiedAt = sv.LastModifiedAt,
                        LastModifiedBy = sv.LastModifiedBy
                    }).ToList()
                    : new List<WorkPermitSafetyVideoDto>(),
                CreatedAt = wps.CreatedAt,
                CreatedBy = wps.CreatedBy,
                LastModifiedAt = wps.LastModifiedAt,
                LastModifiedBy = wps.LastModifiedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        return setting;
    }
}