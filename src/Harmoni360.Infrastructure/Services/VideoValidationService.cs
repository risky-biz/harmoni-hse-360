using Harmoni360.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

/// <summary>
/// Service for validating video files and extracting metadata
/// Note: This is a basic implementation. For production, consider using FFMpegCore or similar library
/// </summary>
public class VideoValidationService : IVideoValidationService
{
    private readonly ILogger<VideoValidationService> _logger;

    // Supported video formats
    private static readonly Dictionary<string, string[]> SupportedFormats = new()
    {
        { "video/mp4", new[] { ".mp4" } },
        { "video/webm", new[] { ".webm" } },
        { "video/avi", new[] { ".avi" } },
        { "video/quicktime", new[] { ".mov" } },
        { "video/x-msvideo", new[] { ".avi" } }
    };

    private const long MaxFileSizeBytes = 104_857_600; // 100MB
    private const int MaxDurationMinutes = 120; // 2 hours

    public VideoValidationService(ILogger<VideoValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<VideoValidationResult> ValidateVideoAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        try
        {
            // Basic file validation
            if (file == null || file.Length == 0)
            {
                errors.Add("No file provided");
                return VideoValidationResult.Failure(errors.ToArray());
            }

            // File size validation
            if (file.Length > MaxFileSizeBytes)
            {
                errors.Add($"File size ({file.Length / 1024 / 1024:F1} MB) exceeds maximum allowed size ({MaxFileSizeBytes / 1024 / 1024} MB)");
            }

            // Content type validation
            var contentType = file.ContentType?.ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType) || !SupportedFormats.ContainsKey(contentType))
            {
                errors.Add($"Unsupported content type: {contentType}. Supported types: {string.Join(", ", SupportedFormats.Keys)}");
            }

            // File extension validation
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(contentType) && SupportedFormats.ContainsKey(contentType))
            {
                if (!SupportedFormats[contentType].Contains(fileExtension))
                {
                    errors.Add($"File extension {fileExtension} does not match content type {contentType}");
                }
            }

            // If basic validation fails, return early
            if (errors.Any())
            {
                return VideoValidationResult.Failure(errors.ToArray());
            }

            // Create temporary file for metadata extraction
            var tempFilePath = Path.GetTempFileName();
            try
            {
                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Extract metadata
                var metadata = await ExtractMetadataAsync(tempFilePath, cancellationToken);

                // Duration validation
                if (metadata.Duration.TotalMinutes > MaxDurationMinutes)
                {
                    errors.Add($"Video duration ({metadata.Duration:hh\\:mm\\:ss}) exceeds maximum allowed duration ({MaxDurationMinutes} minutes)");
                }

                if (metadata.Duration <= TimeSpan.Zero)
                {
                    errors.Add("Invalid video duration");
                }

                if (errors.Any())
                {
                    return VideoValidationResult.Failure(errors.ToArray());
                }

                return VideoValidationResult.Success(metadata);
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating video file {FileName}", file?.FileName);
            errors.Add("An error occurred while validating the video file");
            return VideoValidationResult.Failure(errors.ToArray());
        }
    }

    public async Task<VideoMetadata> ExtractMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic file info
            var fileInfo = new FileInfo(filePath);
            
            // TODO: Implement proper video metadata extraction using FFMpegCore or similar library
            // For now, returning placeholder metadata
            var metadata = new VideoMetadata
            {
                FileSize = fileInfo.Length,
                Format = Path.GetExtension(filePath),
                Duration = TimeSpan.FromMinutes(5), // Placeholder - should be extracted from actual video
                Resolution = "1920x1080", // Placeholder - should be extracted from actual video
                Bitrate = 2000, // Placeholder - should be extracted from actual video
                Codec = "H.264", // Placeholder - should be extracted from actual video
                FrameRate = 30.0, // Placeholder - should be extracted from actual video
                HasAudio = true // Placeholder - should be extracted from actual video
            };

            _logger.LogInformation("Extracted metadata for video file {FilePath}: Duration={Duration}, Size={Size}MB", 
                filePath, metadata.Duration, metadata.FileSize / 1024.0 / 1024.0);

            return await Task.FromResult(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting metadata from video file {FilePath}", filePath);
            throw;
        }
    }

    public async Task<string?> GenerateThumbnailAsync(string videoFilePath, string thumbnailOutputPath, TimeSpan timeOffset = default, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement thumbnail generation using FFMpegCore or similar library
            // For now, returning null to indicate no thumbnail generated
            _logger.LogInformation("Thumbnail generation requested for {VideoPath} at {TimeOffset}", videoFilePath, timeOffset);
            
            return await Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for video {VideoPath}", videoFilePath);
            return null;
        }
    }
}

/// <summary>
/// Extension methods for video validation
/// </summary>
public static class VideoValidationExtensions
{
    /// <summary>
    /// Checks if a file extension is supported for video uploads
    /// </summary>
    public static bool IsSupportedVideoExtension(this string extension)
    {
        var supportedExtensions = new[] { ".mp4", ".webm", ".avi", ".mov" };
        return supportedExtensions.Contains(extension.ToLowerInvariant());
    }

    /// <summary>
    /// Gets the appropriate content type for a video file extension
    /// </summary>
    public static string? GetVideoContentType(this string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".avi" => "video/avi",
            ".mov" => "video/quicktime",
            _ => null
        };
    }
}