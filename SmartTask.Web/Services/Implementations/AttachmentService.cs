using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Files;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ITaskService _taskService;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".txt" };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public AttachmentService(
            ApplicationDbContext context,
            IFileUploadService fileUploadService,
            ITaskService taskService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _taskService = taskService;
        }

        public async Task<List<Attachment>> GetByTaskAsync(int taskItemId)
        {
            return await _context.Attachments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<Attachment> UploadAsync(int taskItemId, int userId, IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("فرمت این فایل مجاز نیست.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.");

            var relativePath = await _fileUploadService.SaveFileAsync(file, "attachments");

            var attachment = new Attachment
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSize = file.Length,
                ContentType = file.ContentType,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.Attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();

            return attachment;
        }

        public async Task<bool> CanDeleteAttachmentAsync(int attachmentId, int userId)
        {
            var attachment = await _context.Attachments.FirstOrDefaultAsync(x => x.Id == attachmentId);
            if (attachment == null) return false;

            if (attachment.ApplicationUserId == userId)
                return true;

            return await _taskService.CanManageTaskAsync(attachment.TaskItemId, userId);
        }

        public async Task DeleteAsync(int id)
        {
            var attachment = await _context.Attachments.FirstOrDefaultAsync(x => x.Id == id);
            if (attachment == null) return;

            attachment.ViewState = false;
            await _context.SaveChangesAsync();

            _fileUploadService.DeleteFile(attachment.FilePath);
        }
    }
}