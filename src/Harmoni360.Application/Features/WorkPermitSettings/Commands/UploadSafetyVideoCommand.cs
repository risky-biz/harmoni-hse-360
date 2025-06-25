using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class UploadSafetyVideoCommand : IRequest<WorkPermitSafetyVideoDto>
{
    public int WorkPermitSettingsId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Resolution { get; set; }
    public int? Bitrate { get; set; }
    public bool SetAsActive { get; set; } = true;
}