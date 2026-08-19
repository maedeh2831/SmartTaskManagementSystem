using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class SubTaskService : BaseService<SubTaskItem>, ISubTaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;

        public SubTaskService(
            IGenericRepository<SubTaskItem> repository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            ITaskService taskService)
            : base(repository, unitOfWork)
        {
            _context = context;
            _taskService = taskService;
        }

        public async Task<List<SubTaskItem>> GetByTaskAsync(int taskItemId)
        {
            return await _context.SubTaskItems
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }

        // OPTIMIZED: Project only needed field instead of loading full entity
        public async Task<bool> CanManageSubTaskAsync(int subTaskId, int userId)
        {
            var taskId = await _context.SubTaskItems
                .Where(x => x.Id == subTaskId)
                .Select(x => x.TaskItemId)
                .FirstOrDefaultAsync();

            return taskId > 0 && await _taskService.CanManageTaskAsync(taskId, userId);
        }

        public async Task ToggleCompleteAsync(int subTaskId)
        {
            var item = await _context.SubTaskItems
                .FirstOrDefaultAsync(x => x.Id == subTaskId && x.ViewState);
            if (item == null) return;

            item.IsCompleted = !item.IsCompleted;
            item.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public new async Task DeleteAsync(int id)
        {
            var item = await _context.SubTaskItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return;

            item.ViewState = false;
            item.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}