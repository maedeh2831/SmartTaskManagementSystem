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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Label>> GetLabelsForTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return new List<Label>();

            return await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.Label)
                .Select(x => x.Label)
                .ToListAsync();
        }

        /// <summary>
        /// OPTIMIZED: Get labels for multiple tasks in a single query
        /// </summary>
        public async Task<Dictionary<int, List<Label>>> GetLabelsForTasksAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, List<Label>>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, List<Label>>();

            var taskLabels = await _context.TaskLabels
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .Include(x => x.Label)
                .ToListAsync();

            return taskLabels
                .GroupBy(x => x.TaskItemId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Label).ToList());
        }

        /// <summary>
        /// OPTIMIZED: Single query to check existence and reactivate
        /// </summary>
        public async Task AssignLabelAsync(int taskItemId, int labelId)
        {
            if (taskItemId <= 0 || labelId <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single query to check existence
            var existing = await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && x.LabelId == labelId)
                .Select(x => new { x.Id, x.ViewState })
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new task label
                var taskLabel = new TaskLabel
                {
                    TaskItemId = taskItemId,
                    LabelId = labelId,
                    CreatedDate = now,
                    ViewState = true
                };

                await _context.TaskLabels.AddAsync(taskLabel);
                await _context.SaveChangesAsync();
            }
            else if (!existing.ViewState)
            {
                // OPTIMIZED: Use ExecuteUpdateAsync to reactivate instead of load-modify-save
                await _context.TaskLabels
                    .Where(x => x.Id == existing.Id)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.ViewState, true)
                        .SetProperty(x => x.ChangeDate, now));
            }
            // If already assigned and active, do nothing
        }

        /// <summary>
        /// OPTIMIZED: Batch assign labels to a task
        /// </summary>
        public async Task BatchAssignLabelsAsync(int taskItemId, List<int> labelIds)
        {
            if (taskItemId <= 0 || labelIds == null || labelIds.Count == 0)
                return;

            var validLabelIds = labelIds.Where(id => id > 0).Distinct().ToList();
            if (validLabelIds.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Get all existing assignments for this task
            var existing = await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && validLabelIds.Contains(x.LabelId))
                .Select(x => new { x.Id, x.LabelId, x.ViewState })
                .ToListAsync();

            var existingDict = existing.ToDictionary(x => x.LabelId);
            var toAdd = new List<TaskLabel>();
            var toReactivate = new List<int>();

            foreach (var labelId in validLabelIds)
            {
                if (existingDict.TryGetValue(labelId, out var existingLabel))
                {
                    if (!existingLabel.ViewState)
                        toReactivate.Add(existingLabel.Id);
                }
                else
                {
                    toAdd.Add(new TaskLabel
                    {
                        TaskItemId = taskItemId,
                        LabelId = labelId,
                        CreatedDate = now,
                        ViewState = true
                    });
                }
            }

            // OPTIMIZED: Single SaveChangesAsync for all new assignments
            if (toAdd.Count > 0)
                await _context.TaskLabels.AddRangeAsync(toAdd);

            // OPTIMIZED: Single ExecuteUpdateAsync for all reactivations
            if (toReactivate.Count > 0)
            {
                await _context.TaskLabels
                    .Where(x => toReactivate.Contains(x.Id))
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.ViewState, true)
                        .SetProperty(x => x.ChangeDate, now));
            }

            if (toAdd.Count > 0)
                await _context.SaveChangesAsync();
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task RemoveLabelAsync(int taskItemId, int labelId)
        {
            if (taskItemId <= 0 || labelId <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && x.LabelId == labelId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch remove labels from a task
        /// </summary>
        public async Task BatchRemoveLabelsAsync(int taskItemId, List<int> labelIds)
        {
            if (taskItemId <= 0 || labelIds == null || labelIds.Count == 0)
                return;

            var validLabelIds = labelIds.Where(id => id > 0).ToList();
            if (validLabelIds.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all removals
            await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && validLabelIds.Contains(x.LabelId) && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Remove all labels from a task
        /// </summary>
        public async Task RemoveAllLabelsAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all removals
            await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Replace all labels for a task (remove old, add new)
        /// </summary>
        public async Task ReplaceLabelsAsync(int taskItemId, List<int> newLabelIds)
        {
            if (taskItemId <= 0)
                return;

            var validLabelIds = newLabelIds?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            var now = DateTime.Now;

            // Get current labels
            var currentLabels = await _context.TaskLabels
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Select(x => new { x.Id, x.LabelId })
                .ToListAsync();

            var currentLabelIds = currentLabels.Select(x => x.LabelId).ToList();

            // Labels to remove (in current but not in new)
            var toRemove = currentLabels
                .Where(x => !validLabelIds.Contains(x.LabelId))
                .Select(x => x.Id)
                .ToList();

            // Labels to add (in new but not in current)
            var toAdd = validLabelIds
                .Where(id => !currentLabelIds.Contains(id))
                .ToList();

            // OPTIMIZED: Single ExecuteUpdateAsync for all removals
            if (toRemove.Count > 0)
            {
                await _context.TaskLabels
                    .Where(x => toRemove.Contains(x.Id))
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.ViewState, false)
                        .SetProperty(x => x.ChangeDate, now));
            }

            // OPTIMIZED: Single SaveChangesAsync for all new assignments
            if (toAdd.Count > 0)
            {
                var newAssignments = toAdd.Select(labelId => new TaskLabel
                {
                    TaskItemId = taskItemId,
                    LabelId = labelId,
                    CreatedDate = now,
                    ViewState = true
                }).ToList();

                await _context.TaskLabels.AddRangeAsync(newAssignments);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// OPTIMIZED: Get task count per label
        /// </summary>
        public async Task<Dictionary<int, int>> GetTaskCountPerLabelAsync(List<int> labelIds)
        {
            if (labelIds == null || labelIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = labelIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.TaskLabels
                .Where(x => validIds.Contains(x.LabelId) && x.ViewState)
                .GroupBy(x => x.LabelId)
                .Select(g => new { LabelId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.LabelId, x => x.Count);
        }

        /// <summary>
        /// OPTIMIZED: Get label count per task
        /// </summary>
        public async Task<Dictionary<int, int>> GetLabelCountPerTaskAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.TaskLabels
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .GroupBy(x => x.TaskItemId)
                .Select(g => new { TaskId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TaskId, x => x.Count);
        }

        /// <summary>
        /// OPTIMIZED: Check if task has specific label
        /// </summary>
        public async Task<bool> HasLabelAsync(int taskItemId, int labelId)
        {
            if (taskItemId <= 0 || labelId <= 0)
                return false;

            return await _context.TaskLabels
                .AnyAsync(x => x.TaskItemId == taskItemId && x.LabelId == labelId && x.ViewState);
        }

        /// <summary>
        /// OPTIMIZED: Check if task has any of the labels
        /// </summary>
        public async Task<bool> HasAnyLabelsAsync(int taskItemId, List<int> labelIds)
        {
            if (taskItemId <= 0 || labelIds == null || labelIds.Count == 0)
                return false;

            var validIds = labelIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return false;

            return await _context.TaskLabels
                .AnyAsync(x => x.TaskItemId == taskItemId && validIds.Contains(x.LabelId) && x.ViewState);
        }

        /// <summary>
        /// OPTIMIZED: Check if task has all the labels
        /// </summary>
        public async Task<bool> HasAllLabelsAsync(int taskItemId, List<int> labelIds)
        {
            if (taskItemId <= 0 || labelIds == null || labelIds.Count == 0)
                return true;

            var validIds = labelIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return true;

            var count = await _context.TaskLabels
                .CountAsync(x => x.TaskItemId == taskItemId && validIds.Contains(x.LabelId) && x.ViewState);

            return count == validIds.Count;
        }
    }
}
