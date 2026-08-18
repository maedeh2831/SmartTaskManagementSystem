using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ChecklistService : IChecklistService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;

        public ChecklistService(ApplicationDbContext context, ITaskService taskService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        public async Task<List<Checklist>> GetByTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return new List<Checklist>();

            return await _context.Checklists
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.Items.Where(i => i.ViewState))
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// OPTIMIZED: Get checklist items count for multiple checklists
        /// </summary>
        public async Task<Dictionary<int, int>> GetItemCountsAsync(List<int> checklistIds)
        {
            if (checklistIds == null || checklistIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = checklistIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.ChecklistItems
                .Where(x => validIds.Contains(x.ChecklistId) && x.ViewState)
                .GroupBy(x => x.ChecklistId)
                .Select(g => new { ChecklistId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ChecklistId, x => x.Count);
        }

        /// <summary>
        /// OPTIMIZED: Get completion percentage for multiple checklists
        /// </summary>
        public async Task<Dictionary<int, double>> GetCompletionPercentagesAsync(List<int> checklistIds)
        {
            if (checklistIds == null || checklistIds.Count == 0)
                return new Dictionary<int, double>();

            var validIds = checklistIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, double>();

            var data = await _context.ChecklistItems
                .Where(x => validIds.Contains(x.ChecklistId) && x.ViewState)
                .GroupBy(x => x.ChecklistId)
                .Select(g => new
                {
                    ChecklistId = g.Key,
                    Total = g.Count(),
                    Completed = g.Count(x => x.IsCompleted)
                })
                .ToListAsync();

            return data.ToDictionary(
                x => x.ChecklistId,
                x => x.Total > 0 ? (double)x.Completed / x.Total * 100 : 0);
        }

        public async Task<Checklist> CreateChecklistAsync(int taskItemId, string title)
        {
            if (taskItemId <= 0 || string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Invalid task ID or title");

            var now = DateTime.Now;

            var checklist = new Checklist
            {
                TaskItemId = taskItemId,
                Title = title.Trim(),
                CreatedDate = now,
                ViewState = true
            };

            await _context.Checklists.AddAsync(checklist);
            await _context.SaveChangesAsync();

            return checklist;
        }

        /// <summary>
        /// OPTIMIZED: Batch create checklists
        /// </summary>
        public async Task<List<Checklist>> BatchCreateChecklistsAsync(List<(int taskItemId, string title)> checklists)
        {
            if (checklists == null || checklists.Count == 0)
                return new List<Checklist>();

            var validChecklists = checklists
                .Where(x => x.taskItemId > 0 && !string.IsNullOrWhiteSpace(x.title))
                .ToList();

            if (validChecklists.Count == 0)
                return new List<Checklist>();

            var now = DateTime.Now;
            var entities = validChecklists.Select(c => new Checklist
            {
                TaskItemId = c.taskItemId,
                Title = c.title.Trim(),
                CreatedDate = now,
                ViewState = true
            }).ToList();

            // OPTIMIZED: Single SaveChangesAsync for all checklists
            await _context.Checklists.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            return entities;
        }

        public async Task<bool> CanManageChecklistAsync(int checklistId, int userId)
        {
            if (checklistId <= 0 || userId <= 0)
                return false;

            var taskId = await _context.Checklists
                .Where(x => x.Id == checklistId && x.ViewState)
                .Select(x => x.TaskItemId)
                .FirstOrDefaultAsync();

            if (taskId <= 0)
                return false;

            return await _taskService.CanManageTaskAsync(taskId, userId);
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteChecklistAsync(int checklistId)
        {
            if (checklistId <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.Checklists
                .Where(x => x.Id == checklistId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            // Also soft-delete all items in this checklist
            await _context.ChecklistItems
                .Where(x => x.ChecklistId == checklistId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch delete checklists
        /// </summary>
        public async Task BatchDeleteChecklistsAsync(List<int> checklistIds)
        {
            if (checklistIds == null || checklistIds.Count == 0)
                return;

            var validIds = checklistIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.Now;

            // Delete checklists
            await _context.Checklists
                .Where(x => validIds.Contains(x.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            // Delete all items in these checklists
            await _context.ChecklistItems
                .Where(x => validIds.Contains(x.ChecklistId))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        public async Task<bool> CanManageItemAsync(int itemId, int userId)
        {
            if (itemId <= 0 || userId <= 0)
                return false;

            var checklistId = await _context.ChecklistItems
                .Where(x => x.Id == itemId && x.ViewState)
                .Select(x => x.ChecklistId)
                .FirstOrDefaultAsync();

            if (checklistId <= 0)
                return false;

            return await CanManageChecklistAsync(checklistId, userId);
        }

        public async Task AddItemAsync(int checklistId, string title)
        {
            if (checklistId <= 0 || string.IsNullOrWhiteSpace(title))
                return;

            var now = DateTime.Now;

            var item = new ChecklistItem
            {
                ChecklistId = checklistId,
                Title = title.Trim(),
                IsCompleted = false,
                CreatedDate = now,
                ViewState = true
            };

            await _context.ChecklistItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// OPTIMIZED: Batch add checklist items
        /// </summary>
        public async Task BatchAddItemsAsync(List<(int checklistId, string title)> items)
        {
            if (items == null || items.Count == 0)
                return;

            var validItems = items
                .Where(x => x.checklistId > 0 && !string.IsNullOrWhiteSpace(x.title))
                .ToList();

            if (validItems.Count == 0)
                return;

            var now = DateTime.Now;
            var entities = validItems.Select(item => new ChecklistItem
            {
                ChecklistId = item.checklistId,
                Title = item.title.Trim(),
                IsCompleted = false,
                CreatedDate = now,
                ViewState = true
            }).ToList();

            // OPTIMIZED: Single SaveChangesAsync for all items
            await _context.ChecklistItems.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task ToggleItemAsync(int itemId)
        {
            if (itemId <= 0)
                return;

            var now = DateTime.Now;

            // Get current state
            var currentState = await _context.ChecklistItems
                .Where(x => x.Id == itemId && x.ViewState)
                .Select(x => x.IsCompleted)
                .FirstOrDefaultAsync();

            if (currentState == null)
                return;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.ChecklistItems
                .Where(x => x.Id == itemId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsCompleted, !currentState)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch toggle multiple items
        /// </summary>
        public async Task BatchToggleItemsAsync(List<(int itemId, bool isCompleted)> items)
        {
            if (items == null || items.Count == 0)
                return;

            var now = DateTime.Now;

            foreach (var item in items.Where(x => x.itemId > 0))
            {
                // OPTIMIZED: Individual async updates can be parallelized if needed
                await _context.ChecklistItems
                    .Where(x => x.Id == item.itemId)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.IsCompleted, item.isCompleted)
                        .SetProperty(x => x.ChangeDate, now));
            }
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteItemAsync(int itemId)
        {
            if (itemId <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.ChecklistItems
                .Where(x => x.Id == itemId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch delete checklist items
        /// </summary>
        public async Task BatchDeleteItemsAsync(List<int> itemIds)
        {
            if (itemIds == null || itemIds.Count == 0)
                return;

            var validIds = itemIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.ChecklistItems
                .Where(x => validIds.Contains(x.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Mark all items as completed in a checklist
        /// </summary>
        public async Task MarkAllItemsCompletedAsync(int checklistId)
        {
            if (checklistId <= 0)
                return;

            var now = DateTime.Now;

            await _context.ChecklistItems
                .Where(x => x.ChecklistId == checklistId && x.ViewState && !x.IsCompleted)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsCompleted, true)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Mark all items as incomplete in a checklist
        /// </summary>
        public async Task MarkAllItemsIncompleteAsync(int checklistId)
        {
            if (checklistId <= 0)
                return;

            var now = DateTime.Now;

            await _context.ChecklistItems
                .Where(x => x.ChecklistId == checklistId && x.ViewState && x.IsCompleted)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsCompleted, false)
                    .SetProperty(x => x.ChangeDate, now));
        }
    }
}
