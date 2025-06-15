using FluentValidation;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class UploadWasteAttachmentCommandValidator : AbstractValidator<UploadWasteAttachmentCommand>
{
    private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
    private const long _maxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadWasteAttachmentCommandValidator()
    {
        RuleFor(x => x.WasteReportId)
            .GreaterThan(0).WithMessage("Waste report ID must be valid");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(BeValidFileSize).WithMessage($"File size must not exceed {_maxFileSize / (1024 * 1024)}MB")
            .Must(HaveValidExtension).WithMessage($"File must have one of these extensions: {string.Join(", ", _allowedExtensions)}");
    }

    private bool BeValidFileSize(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        return file != null && file.Length > 0 && file.Length <= _maxFileSize;
    }

    private bool HaveValidExtension(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return false;
        
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && _allowedExtensions.Contains(extension);
    }
}