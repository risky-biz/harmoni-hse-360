namespace HarmoniHSE360.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, string folder);
    Task<Stream> DownloadAsync(string filePath);
    Task DeleteAsync(string filePath);
    Task<bool> ExistsAsync(string filePath);
}

public class FileUploadResult
{
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
}