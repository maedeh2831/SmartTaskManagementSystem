using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public TaskAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationUser>> GetAssigneesAsync(int taskItemId)
        {
            return await _context.TaskAssignments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .Select(x => x.ApplicationUser)
                .ToListAsync();
        }

        public async Task<bool> IsAssignedAsync(int taskItemId, int userId)
        {
            return await _context.TaskAssignments
                .AnyAsync(x => x.TaskItemId == taskItemId && x.ApplicationUserId == userId && x.ViewState);
        }

        public async Task AssignUserAsync(int taskItemId, int userId)
        {
            // 👇 به‌جای فقط چک‌کردن رکورد فعال، رکورد قبلی (فعال یا غیرفعال) رو پیدا می‌کنیم
            var existing = await _context.TaskAssignments
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId);

            if (existing != null)
            {
                if (existing.ViewState)
                    return; // از قبل فعاله، کاری لازم نیست

                // رکورد قبلاً حذف‌شده رو دوباره فعال می‌کنیم به‌جای ساخت رکورد تکراری
                existing.ViewState = true;
                existing.AssignedDate = DateTime.Now;
                existing.ChangeDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return;
            }

            var assignment = new TaskAssignment
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                AssignedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.TaskAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserAsync(int taskItemId, int userId)
        {
            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId &&
                    x.ViewState);

            if (assignment == null)
                return;

            assignment.ViewState = false;
            assignment.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}