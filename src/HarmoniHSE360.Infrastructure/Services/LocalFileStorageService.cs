using HarmoniHSE360.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace HarmoniHSE360.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly string _uploadsPath;

    public LocalFileStorageService(IHostEnvironment environment)
    {
        _environment = environment;
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        // Ensure uploads directory exists
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
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

    public async Task<Stream> DownloadAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadsPath, filePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
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