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

        public async Task UpdateAsync(int id, string title, DateTime reminderDate)
        {
            var reminder = await _context.Reminders.FirstOrDefaultAsync(x => x.Id == id);
            if (reminder == null)
                return;

            reminder.Title = title.Trim();
            reminder.ReminderDate = reminderDate;
            reminder.IsSent = false; // اگه زمان تغییر کرد، باید دوباره ارسال بشه
            reminder.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reminder = await _context.Reminders.FirstOrDefaultAsync(x => x.Id == id);
            if (reminder == null)
                return;

            reminder.ViewState = false;
            reminder.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Reminder>> GetPendingManualRemindersAsync()
        {
            var now = DateTime.Now;

            return await _context.Reminders
                .Where(x => !x.IsSent && x.ViewState && x.ReminderDate <= now)
                .Include(x => x.TaskItem)
                .ToListAsync();
        }

        public async Task MarkAsSentAsync(int id)
        {
            var reminder = await _context.Reminders.FirstOrDefaultAsync(x => x.Id == id);
            if (reminder == null)
                return;

            reminder.IsSent = true;
            reminder.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
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