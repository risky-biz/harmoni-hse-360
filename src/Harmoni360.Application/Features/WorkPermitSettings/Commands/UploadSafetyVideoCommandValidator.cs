using FluentValidation;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class UploadSafetyVideoCommandValidator : AbstractValidator<UploadSafetyVideoCommand>
{
    private static readonly string[] SupportedVideoTypes = { "video/mp4", "video/webm", "video/avi" };
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

    public UploadSafetyVideoCommandValidator()
    {
        RuleFor(x => x.WorkPermitSettingsId)
            .GreaterThan(0).WithMessage("Work Permit Settings ID must be provided.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters.");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("Original file name is required.")
            .MaximumLength(255).WithMessage("Original file name cannot exceed 255 characters.");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required.")
            .MaximumLength(500).WithMessage("File path cannot exceed 500 characters.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024} MB.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(contentType => SupportedVideoTypes.Contains(contentType.ToLowerInvariant()))
            .WithMessage($"Only the following video formats are supported: {string.Join(", ", SupportedVideoTypes)}.");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Video duration must be greater than zero.")
            .LessThan(TimeSpan.FromHours(2)).WithMessage("Video duration cannot exceed 2 hours.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ThumbnailPath)
            .MaximumLength(500).WithMessage("Thumbnail path cannot exceed 500 characters.");

        RuleFor(x => x.Resolution)
            .MaximumLength(20).WithMessage("Resolution cannot exceed 20 characters.")
            .Matches(@"^\d+x\d+$").WithMessage("Resolution must be in format 'widthxheight' (e.g., '1920x1080').")
            .When(x => !string.IsNullOrEmpty(x.Resolution));

        RuleFor(x => x.Bitrate)
            .GreaterThan(0).WithMessage("Bitrate must be greater than 0 if specified.")
            .When(x => x.Bitrate.HasValue);
    }
}