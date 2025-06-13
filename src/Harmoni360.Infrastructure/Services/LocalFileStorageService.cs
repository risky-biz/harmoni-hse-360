using Harmoni360.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Harmoni360.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        // Get uploads path from configuration, with fallback to default
        _uploadsPath = configuration["FileStorage:UploadsPath"] ?? "uploads";

        // Note: Directory creation is handled in Program.cs since we need access to WebRootPath
    }

    public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        // Create folder if it doesn't exist
        var folderPath = Path.Combine(_uploadsPath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique filename to avoid conflicts
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);
        var relativePath = Path.Combine(folder, uniqueFileName).Replace("\\", "/");

        // Save file
        using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput);

        var fileInfo = new FileInfo(filePath);

        return new FileUploadResult
        {
            FilePath = relativePath,
            Url = $"/uploads/{relativePath}",
            Size = fileInfo.Length
        };
    }

    public Task<Stream> DownloadAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadsPath, filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadsPath, filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadsPath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }
}