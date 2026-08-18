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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        public async Task<List<Attachment>> GetByTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return new List<Attachment>();

            return await _context.Attachments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// OPTIMIZED: Get attachments for multiple tasks in a single query
        /// </summary>
        public async Task<Dictionary<int, List<Attachment>>> GetByTasksAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, List<Attachment>>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, List<Attachment>>();

            var attachments = await _context.Attachments
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return attachments
                .GroupBy(x => x.TaskItemId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// OPTIMIZED: Validate file before upload
        /// </summary>
        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("فایل خالی است.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException($"فرمت این فایل مجاز نیست. فرمت‌های مجاز: {string.Join(", ", AllowedExtensions)}");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException($"حجم فایل نباید بیشتر از ۱۰ مگابایت باشد. حجم فایل شما: {file.Length / (1024 * 1024)} مگابایت");
        }

        public async Task<Attachment> UploadAsync(int taskItemId, int userId, IFormFile file)
        {
            if (taskItemId <= 0 || userId <= 0 || file == null)
                throw new ArgumentException("Invalid task ID, user ID, or file");

            ValidateFile(file);

            var relativePath = await _fileUploadService.SaveFileAsync(file, "attachments");

            var now = DateTime.Now;

            var attachment = new Attachment
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSize = file.Length,
                ContentType = file.ContentType,
                CreatedDate = now,
                ViewState = true
            };

            await _context.Attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();

            return attachment;
        }

        /// <summary>
        /// OPTIMIZED: Batch upload multiple files
        /// </summary>
        public async Task<List<Attachment>> BatchUploadAsync(int taskItemId, int userId, List<IFormFile> files)
        {
            if (taskItemId <= 0 || userId <= 0 || files == null || files.Count == 0)
                return new List<Attachment>();

            var validFiles = files.Where(f => f != null && f.Length > 0).ToList();
            if (validFiles.Count == 0)
                return new List<Attachment>();

            var now = DateTime.Now;
            var uploadedAttachments = new List<Attachment>();

            // Validate all files first
            foreach (var file in validFiles)
            {
                ValidateFile(file);
            }

            // OPTIMIZED: Upload files in parallel, then save all in single transaction
            var uploadTasks = validFiles.Select(file =>
                _fileUploadService.SaveFileAsync(file, "attachments")
                    .ContinueWith(task => (file, path: task.Result))
            ).ToArray();

            await Task.WhenAll(uploadTasks);

            var attachments = uploadTasks
                .Select(t => t.Result)
                .Select(x => new Attachment
                {
                    TaskItemId = taskItemId,
                    ApplicationUserId = userId,
                    FileName = x.file.FileName,
                    FilePath = x.path,
                    FileSize = x.file.Length,
                    ContentType = x.file.ContentType,
                    CreatedDate = now,
                    ViewState = true
                })
                .ToList();

            // OPTIMIZED: Single SaveChangesAsync for all attachments
            await _context.Attachments.AddRangeAsync(attachments);
            await _context.SaveChangesAsync();

            return attachments;
        }

        public async Task<bool> CanDeleteAttachmentAsync(int attachmentId, int userId)
        {
            if (attachmentId <= 0 || userId <= 0)
                return false;

            var attachment = await _context.Attachments
                .Where(x => x.Id == attachmentId && x.ViewState)
                .Select(x => new { x.ApplicationUserId, x.TaskItemId })
                .FirstOrDefaultAsync();

            if (attachment == null)
                return false;

            // Owner can delete
            if (attachment.ApplicationUserId == userId)
                return true;

            // Task manager can delete
            return await _taskService.CanManageTaskAsync(attachment.TaskItemId, userId);
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                return;

            // Get file path before deleting
            var filePath = await _context.Attachments
                .Where(x => x.Id == id && x.ViewState)
                .Select(x => x.FilePath)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(filePath))
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.Attachments
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            // Delete file from storage (fire-and-forget if safe)
            _fileUploadService.DeleteFile(filePath);
        }

        /// <summary>
        /// OPTIMIZED: Batch delete attachments
        /// </summary>
        public async Task BatchDeleteAsync(List<int> attachmentIds)
        {
            if (attachmentIds == null || attachmentIds.Count == 0)
                return;

            var validIds = attachmentIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            // Get all file paths before deleting
            var filePaths = await _context.Attachments
                .Where(x => validIds.Contains(x.Id) && x.ViewState)
                .Select(x => x.FilePath)
                .ToListAsync();

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.Attachments
                .Where(x => validIds.Contains(x.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            // OPTIMIZED: Delete files in parallel
            var deleteTasks = filePaths
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => Task.Run(() => _fileUploadService.DeleteFile(path)))
                .ToList();

            if (deleteTasks.Any())
                await Task.WhenAll(deleteTasks);
        }

        /// <summary>
        /// OPTIMIZED: Get attachment count for multiple tasks
        /// </summary>
        public async Task<Dictionary<int, int>> GetAttachmentCountsAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.Attachments
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .GroupBy(x => x.TaskItemId)
                .Select(g => new { TaskId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TaskId, x => x.Count);
        }

        /// <summary>
        /// OPTIMIZED: Get total attachment size for multiple tasks
        /// </summary>
        public async Task<Dictionary<int, long>> GetTotalSizeAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, long>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, long>();

            return await _context.Attachments
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .GroupBy(x => x.TaskItemId)
                .Select(g => new { TaskId = g.Key, TotalSize = g.Sum(x => x.FileSize) })
                .ToDictionaryAsync(x => x.TaskId, x => x.TotalSize);
        }

        /// <summary>
        /// OPTIMIZED: Delete all attachments for a task
        /// </summary>
        public async Task DeleteAllForTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return;

            // Get all file paths before deleting
            var filePaths = await _context.Attachments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Select(x => x.FilePath)
                .ToListAsync();

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.Attachments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            // OPTIMIZED: Delete files in parallel
            var deleteTasks = filePaths
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => Task.Run(() => _fileUploadService.DeleteFile(path)))
                .ToList();

            if (deleteTasks.Any())
                await Task.WhenAll(deleteTasks);
        }
    }
}
