using Microsoft.AspNetCore.Http;

namespace SmartTask.Web.Services.Files;

public interface IFileUploadService
{
    Task<string> SaveFileAsync(IFormFile file, string folderName);
    void DeleteFile(string? relativePath);
}