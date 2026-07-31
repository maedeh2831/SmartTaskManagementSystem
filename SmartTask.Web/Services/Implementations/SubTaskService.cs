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

        public async Task<bool> CanManageSubTaskAsync(int subTaskId, int userId)
        {
            var subTask = await _repository
                .Query()
                .FirstOrDefaultAsync(x => x.Id == subTaskId);

            if (subTask == null)
                return false;

            return await _taskService.CanManageTaskAsync(subTask.TaskItemId, userId);
        }

        public async Task ToggleCompleteAsync(int subTaskId)
        {
            var subTask = await _context.SubTaskItems
                .FirstOrDefaultAsync(x => x.Id == subTaskId);

            if (subTask == null)
                return;

            subTask.IsCompleted = !subTask.IsCompleted;
            subTask.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public new async Task DeleteAsync(int id)
        {
            var subTask = await _context.SubTaskItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subTask == null)
                return;

            subTask.ViewState = false;
            await _context.SaveChangesAsync();
        }
    }
}