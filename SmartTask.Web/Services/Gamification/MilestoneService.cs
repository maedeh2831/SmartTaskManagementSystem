/*
| Module      : Gamification
| Class       : MilestoneService
| Purpose     : پیاده‌سازی سرویس نقاط عطف
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MilestoneService> _logger;

        public MilestoneService(ApplicationDbContext context, ILogger<MilestoneService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Milestone>> GetAllMilestonesAsync()
        {
            try
            {
                return await _context.Set<Milestone>()
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all milestones");
                return new List<Milestone>();
            }
        }

        public async Task<Milestone> GetMilestoneByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Milestone>()
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting milestone {MilestoneId}", id);
                return null;
            }
        }

        public async Task<List<UserMilestoneProgress>> GetUserMilestoneProgressAsync(int userId)
        {
            try
            {
                return await _context.Set<UserMilestoneProgress>()
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Milestone)
                    .OrderBy(x => x.Milestone.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting milestone progress for user {UserId}", userId);
                return new List<UserMilestoneProgress>();
            }
        }

        public async Task<UserMilestoneProgress> GetUserMilestoneProgressByIdAsync(int userId, int milestoneId)
        {
            try
            {
                return await _context.Set<UserMilestoneProgress>()
                    .Include(x => x.Milestone)
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.MilestoneId == milestoneId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting milestone progress for user {UserId}, milestone {MilestoneId}", userId, milestoneId);
                return null;
            }
        }

        public async Task<int> GetUserMilestoneCompletionPercentageAsync(int userId, int milestoneId)
        {
            try
            {
                var progress = await GetUserMilestoneProgressByIdAsync(userId, milestoneId);
                if (progress == null)
                    return 0;

                var percentage = (progress.CurrentProgress * 100) / progress.TargetValue;
                return Math.Min(percentage, 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating milestone completion percentage");
                return 0;
            }
        }
    }
}
