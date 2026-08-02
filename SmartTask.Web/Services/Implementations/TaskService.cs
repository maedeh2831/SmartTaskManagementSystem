using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class TaskService : BaseService<TaskItem>, ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserStoryService _userStoryService;

        public TaskService(
            IGenericRepository<TaskItem> repository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            IUserStoryService userStoryService)
            : base(repository, unitOfWork)
        {
            _context = context;
            _userStoryService = userStoryService;
        }

        public async Task<TaskItem?> GetDetailsAsync(int id)
        {
            return await _context.TaskItems
                .Include(x => x.UserStory)
                    .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
        }

        public async Task<List<TaskItem>> GetByUserStoryAsync(int userStoryId)
        {
            return await _context.TaskItems
                .Where(x => x.UserStoryId == userStoryId && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTitleAsync(int userStoryId, string title, int? excludeId = null)
        {
            var query = _repository
                .Query()
                .Where(x => x.UserStoryId == userStoryId && x.Title == title && x.ViewState);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanManageTaskAsync(int taskId, int userId)
        {
            var task = await _repository
                .Query()
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
                return false;

            return await _userStoryService.CanManageStoryAsync(task.UserStoryId, userId);
        }

        public async Task ChangeStatusAsync(int taskId, TaskStatusType status)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
                return;

            task.Status = status;
            task.ChangeDate = DateTime.Now;

            if (status == TaskStatusType.Done)
                task.CompletedDate = DateTime.Now;
            else if (task.CompletedDate.HasValue)
                task.CompletedDate = null;

            await _context.SaveChangesAsync();
        }

        public new async Task DeleteAsync(int id)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return;

            task.ViewState = false;
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskItem>> GetProjectBoardAsync(
        int projectId,
        int? assigneeId = null,
        TaskPriorityType? priority = null,
        TaskType? type = null,
        int? labelId = null)
            {
                var query = _context.TaskItems
                    .Where(x =>
                        x.ViewState &&
                        x.UserStory.ViewState &&
                        x.UserStory.ProjectId == projectId)
                    .Include(x => x.UserStory)
                    .Include(x => x.Assignments.Where(a => a.ViewState))
                        .ThenInclude(a => a.ApplicationUser)
                    .Include(x => x.TaskLabels.Where(tl => tl.ViewState))
                        .ThenInclude(tl => tl.Label)
                    .AsQueryable();

                if (assigneeId.HasValue)
                    query = query.Where(x => x.Assignments.Any(a => a.ViewState && a.ApplicationUserId == assigneeId.Value));

                if (priority.HasValue)
                    query = query.Where(x => x.Priority == priority.Value);

                if (type.HasValue)
                    query = query.Where(x => x.Type == type.Value);

                if (labelId.HasValue)
                    query = query.Where(x => x.TaskLabels.Any(tl => tl.ViewState && tl.LabelId == labelId.Value));

                return await query
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
            }
    }
}