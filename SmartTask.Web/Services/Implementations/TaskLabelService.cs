using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class TaskLabelService : ITaskLabelService
    {
        private readonly ApplicationDbContext _context;

        public TaskLabelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Label>> GetLabelsForTaskAsync(int taskItemId)
        {
            return await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.Label)
                .Select(x => x.Label)
                .ToListAsync();
        }

        public async Task AssignLabelAsync(int taskItemId, int labelId)
        {
            var existing = await _context.TaskLabels
                .FirstOrDefaultAsync(x => x.TaskItemId == taskItemId && x.LabelId == labelId);

            if (existing != null)
            {
                if (existing.ViewState)
                    return;

                existing.ViewState = true;
                existing.ChangeDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return;
            }

            var taskLabel = new TaskLabel
            {
                TaskItemId = taskItemId,
                LabelId = labelId,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.TaskLabels.AddAsync(taskLabel);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLabelAsync(int taskItemId, int labelId)
        {
            var taskLabel = await _context.TaskLabels
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.LabelId == labelId &&
                    x.ViewState);

            if (taskLabel == null) return;

            taskLabel.ViewState = false;
            taskLabel.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}