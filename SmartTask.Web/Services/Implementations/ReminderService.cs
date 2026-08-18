using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ReminderService : IReminderService
    {
        private readonly ApplicationDbContext _context;

        public ReminderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reminder>> GetByUserAsync(int userId)
        {
            return await _context.Reminders
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .Include(x => x.TaskItem)
                .OrderBy(x => x.ReminderDate)
                .ToListAsync();
        }

        public async Task<Reminder?> GetByIdAsync(int id)
        {
            return await _context.Reminders
                .Include(x => x.TaskItem)
                .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
        }

        public async Task<List<TaskItem>> GetAssignedTasksAsync(int userId)
        {
            return await _context.TaskAssignments
                .Where(x => x.ApplicationUserId == userId && x.ViewState && x.TaskItem.ViewState)
                .Select(x => x.TaskItem)
                .Distinct()
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> CanManageReminderAsync(int id, int userId)
        {
            var reminder = await _context.Reminders.FirstOrDefaultAsync(x => x.Id == id);
            return reminder != null && reminder.ApplicationUserId == userId;
        }

        public async Task CreateAsync(int taskItemId, int userId, string title, DateTime reminderDate)
        {
            var reminder = new Reminder
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                Title = title.Trim(),
                ReminderDate = reminderDate,
                IsSent = false,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.Reminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
        }

        // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
        public async Task UpdateAsync(int id, string title, DateTime reminderDate)
        {
            await _context.Reminders
                .Where(x => x.Id == id && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.Title, title.Trim())
                    .SetProperty(x => x.ReminderDate, reminderDate)
                    .SetProperty(x => x.IsSent, false)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        // OPTIMIZED: Use ExecuteUpdateAsync for soft delete
        public async Task DeleteAsync(int id)
        {
            await _context.Reminders
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task<List<Reminder>> GetPendingManualRemindersAsync()
        {
            var now = DateTime.Now;

            return await _context.Reminders
                .Where(x => !x.IsSent && x.ViewState && x.ReminderDate <= now)
                .Include(x => x.TaskItem)
                .ToListAsync();
        }

        // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
        public async Task MarkAsSentAsync(int id)
        {
            await _context.Reminders
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsSent, true)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task<bool> AutoReminderExistsAsync(int taskItemId, int userId, string marker)
        {
            return await _context.Reminders
                .AnyAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId &&
                    x.Title == marker);
        }

        public async Task CreateAutoSentReminderAsync(int taskItemId, int userId, string title, DateTime reminderDate)
        {
            var reminder = new Reminder
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                Title = title,
                ReminderDate = reminderDate,
                IsSent = true,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.Reminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
        }
    }
}