using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Services;

/// <summary>
/// Service for validating video files and extracting metadata
/// </summary>
public interface IVideoValidationService
{
    /// <summary>
    /// Validates a video file format, size, and content
    /// </summary>
    Task<VideoValidationResult> ValidateVideoAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts metadata from a video file
    /// </summary>
    Task<VideoMetadata> ExtractMetadataAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a thumbnail for a video file
    /// </summary>
    Task<string?> GenerateThumbnailAsync(string videoFilePath, string thumbnailOutputPath, TimeSpan timeOffset = default, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of video validation
/// </summary>
public class VideoValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public VideoMetadata? Metadata { get; set; }

    public static VideoValidationResult Success(VideoMetadata metadata) => new()
    {
        IsValid = true,
        Metadata = metadata
    };

    public static VideoValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}

/// <summary>
/// Video metadata extracted from file
/// </summary>
public class VideoMetadata
{
    public TimeSpan Duration { get; set; }
    public string? Resolution { get; set; }
    public int? Bitrate { get; set; }
    public string? Format { get; set; }
    public long FileSize { get; set; }
    public string? Codec { get; set; }
    public double? FrameRate { get; set; }
    public bool HasAudio { get; set; }
}