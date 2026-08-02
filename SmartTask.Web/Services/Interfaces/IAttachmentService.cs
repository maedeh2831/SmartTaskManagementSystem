using Microsoft.AspNetCore.Http;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface IAttachmentService
{
    Task<List<Attachment>> GetByTaskAsync(int taskItemId);
    Task<Attachment> UploadAsync(int taskItemId, int userId, IFormFile file);
    Task<bool> CanDeleteAttachmentAsync(int attachmentId, int userId);
    Task DeleteAsync(int id);
}