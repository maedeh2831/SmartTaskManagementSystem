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

        // OPTIMIZED: Use ExecuteUpdateAsync for toggle instead of load-modify-save
        public async Task ToggleCompleteAsync(int subTaskId)
        {
            var now = DateTime.Now;
            await _context.SubTaskItems
                .Where(x => x.Id == subTaskId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsCompleted, x => !x.IsCompleted)
                    .SetProperty(x => x.ChangeDate, now));
        }

        // OPTIMIZED: Use ExecuteUpdateAsync for soft delete instead of load-modify-save
        public new async Task DeleteAsync(int id)
        {
            await _context.SubTaskItems
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }
    }
}