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
            _context = context;
            _taskService = taskService;
        }

        public async Task<List<Checklist>> GetByTaskAsync(int taskItemId)
        {
            return await _context.Checklists
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.Items.Where(i => i.ViewState))
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<Checklist> CreateChecklistAsync(int taskItemId, string title)
        {
            var checklist = new Checklist
            {
                TaskItemId = taskItemId,
                Title = title.Trim(),
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.Checklists.AddAsync(checklist);
            await _context.SaveChangesAsync();

            return checklist;
        }

        public async Task<bool> CanManageChecklistAsync(int checklistId, int userId)
        {
            var checklist = await _context.Checklists.FirstOrDefaultAsync(x => x.Id == checklistId);
            if (checklist == null) return false;

            return await _taskService.CanManageTaskAsync(checklist.TaskItemId, userId);
        }

        public async Task DeleteChecklistAsync(int checklistId)
        {
            var checklist = await _context.Checklists.FirstOrDefaultAsync(x => x.Id == checklistId);
            if (checklist == null) return;

            checklist.ViewState = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanManageItemAsync(int itemId, int userId)
        {
            var item = await _context.ChecklistItems.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item == null) return false;

            return await CanManageChecklistAsync(item.ChecklistId, userId);
        }

        public async Task AddItemAsync(int checklistId, string title)
        {
            var item = new ChecklistItem
            {
                ChecklistId = checklistId,
                Title = title.Trim(),
                IsCompleted = false,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.ChecklistItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleItemAsync(int itemId)
        {
            var item = await _context.ChecklistItems.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item == null) return;

            item.IsCompleted = !item.IsCompleted;
            item.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int itemId)
        {
            var item = await _context.ChecklistItems.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item == null) return;

            item.ViewState = false;
            await _context.SaveChangesAsync();
        }
    }
}