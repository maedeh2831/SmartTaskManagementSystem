using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class LabelService : ILabelService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserStoryService _userStoryService;

        public LabelService(ApplicationDbContext context, IUserStoryService userStoryService)
        {
            _context = context;
            _userStoryService = userStoryService;
        }

        public async Task<List<Label>> GetByProjectAsync(int projectId)
        {
            return await _context.Labels
                .Where(x => x.ProjectId == projectId && x.ViewState)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(int projectId, string name, int? excludeId = null)
        {
            var query = _context.Labels
                .Where(x => x.ProjectId == projectId && x.Name == name && x.ViewState);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanManageLabelsAsync(int projectId, int userId)
        {
            return await _userStoryService.CanManageBacklogAsync(projectId, userId);
        }

        public async Task CreateOrReactivateAsync(int projectId, string name, string color)
        {
            // 👇 چک می‌کنیم رکورد قبلی (فعال یا غیرفعال) با همین نام وجود داره یا نه
            var existing = await _context.Labels
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Name == name);

            if (existing != null)
            {
                if (existing.ViewState)
                    return; // از قبل فعاله، کاری لازم نیست (این حالت عملاً توسط ExistsByNameAsync قبلش گرفته میشه)

                // رکورد قبلاً حذف‌شده رو Reactivate می‌کنیم به‌جای Insert دوباره
                existing.ViewState = true;
                existing.Color = color;
                existing.ChangeDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return;
            }

            var label = new Label
            {
                ProjectId = projectId,
                Name = name,
                Color = color,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.Labels.AddAsync(label);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var label = await _context.Labels.FirstOrDefaultAsync(x => x.Id == id);
            if (label == null) return;

            label.ViewState = false;
            await _context.SaveChangesAsync();
        }
    }
}