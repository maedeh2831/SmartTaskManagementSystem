/*
| Module      : Gamification
| Class       : AchievementEngine
| Purpose     : پیاده‌سازی موتور دستاوردها و ردیابی پیشرفت
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class AchievementEngine : IAchievementEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly DomainEventPublisher _eventPublisher;
        private readonly ILogger<AchievementEngine> _logger;

        public AchievementEngine(ApplicationDbContext context, DomainEventPublisher eventPublisher, ILogger<AchievementEngine> logger)
        {
            _context = context;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task OnTaskCompletedAsync(int taskId, int userId)
        {
            try
            {
                var progression = await GetOrCreateProgressionAsync(userId);
                if (progression == null)
                    return;

                progression.TasksCompleted += 1;
                progression.LastProgressUpdate = DateTime.UtcNow;

                // Check achievement conditions
                await CheckTaskAchievementsAsync(userId, progression.Id, progression.TasksCompleted);

                // Check milestone progress
                await CheckMilestoneProgressAsync(userId, "TasksCompleted", 1);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing task completion for user {UserId}", userId);
            }
        }

        public async Task OnProjectCompletedAsync(int projectId, int userId)
        {
            try
            {
                var progression = await GetOrCreateProgressionAsync(userId);
                if (progression == null)
                    return;

                progression.ProjectsCompleted += 1;
                progression.LastProgressUpdate = DateTime.UtcNow;

                await CheckProjectAchievementsAsync(userId, progression.Id, progression.ProjectsCompleted);
                await CheckMilestoneProgressAsync(userId, "ProjectsCompleted", 1);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing project completion for user {UserId}", userId);
            }
        }

        public async Task OnSprintCompletedAsync(int sprintId, int userId)
        {
            try
            {
                var progression = await GetOrCreateProgressionAsync(userId);
                if (progression == null)
                    return;

                progression.SprintsCompleted += 1;
                progression.LastProgressUpdate = DateTime.UtcNow;

                await CheckSprintAchievementsAsync(userId, progression.Id, progression.SprintsCompleted);
                await CheckMilestoneProgressAsync(userId, "SprintsCompleted", 1);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sprint completion for user {UserId}", userId);
            }
        }

        /// <summary>
        /// دریافت پیشرفت کاربر و ساخت آن در صورت نبودن (کاربران قبل از افزودن گیمیفیکیشن)
        /// </summary>
        private async Task<UserProgression?> GetOrCreateProgressionAsync(int userId)
        {
            var progression = await _context.Set<UserProgression>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (progression != null)
                return progression;

            progression = new UserProgression
            {
                UserId = userId,
                CurrentLevel = 1,
                TotalExperience = 0,
                ExperienceForNextLevel = 1000,
                LastProgressUpdate = DateTime.UtcNow,
                JoinedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ViewState = true
            };

            _context.Set<UserProgression>().Add(progression);
            await _context.SaveChangesAsync();

            return progression;
        }

        public async Task CheckMilestoneProgressAsync(int userId, string milestoneCondition, int incrementBy = 1)
        {
            try
            {
                var milestones = await _context.Set<Milestone>()
                    .Where(m => m.Condition == milestoneCondition && m.IsActive)
                    .ToListAsync();

                foreach (var milestone in milestones)
                {
                    var userProgress = await _context.Set<UserMilestoneProgress>()
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.MilestoneId == milestone.Id);

                    if (userProgress == null)
                    {
                        userProgress = new UserMilestoneProgress
                        {
                            UserId = userId,
                            MilestoneId = milestone.Id,
                            CurrentProgress = 0,
                            TargetValue = milestone.TargetValue,
                            LastProgressUpdate = DateTime.UtcNow
                        };
                        _context.Set<UserMilestoneProgress>().Add(userProgress);
                    }

                    userProgress.CurrentProgress += incrementBy;
                    userProgress.LastProgressUpdate = DateTime.UtcNow;

                    if (userProgress.CurrentProgress >= milestone.TargetValue && !userProgress.IsCompleted)
                    {
                        userProgress.IsCompleted = true;
                        userProgress.CompletedDate = DateTime.UtcNow;

                        _logger.LogInformation("User {UserId} completed milestone {MilestoneId} - {MilestoneName}",
                            userId, milestone.Id, milestone.Name);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking milestone progress for user {UserId}", userId);
            }
        }

        private async Task CheckTaskAchievementsAsync(int userId, int progressionId, int tasksCompleted)
        {
            try
            {
                var achievements = await _context.Set<Achievement>()
                    .Where(a => a.Condition == "TasksCompleted" && a.IsActive)
                    .ToListAsync();

                foreach (var achievement in achievements)
                {
                    if (tasksCompleted >= achievement.ConditionValue)
                    {
                        await UnlockAchievementAsync(userId, progressionId, achievement.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking task achievements for user {UserId}", userId);
            }
        }

        private async Task CheckProjectAchievementsAsync(int userId, int progressionId, int projectsCompleted)
        {
            try
            {
                var achievements = await _context.Set<Achievement>()
                    .Where(a => a.Condition == "ProjectsCompleted" && a.IsActive)
                    .ToListAsync();

                foreach (var achievement in achievements)
                {
                    if (projectsCompleted >= achievement.ConditionValue)
                    {
                        await UnlockAchievementAsync(userId, progressionId, achievement.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking project achievements for user {UserId}", userId);
            }
        }

        private async Task CheckSprintAchievementsAsync(int userId, int progressionId, int sprintsCompleted)
        {
            try
            {
                var achievements = await _context.Set<Achievement>()
                    .Where(a => a.Condition == "SprintsCompleted" && a.IsActive)
                    .ToListAsync();

                foreach (var achievement in achievements)
                {
                    if (sprintsCompleted >= achievement.ConditionValue)
                    {
                        await UnlockAchievementAsync(userId, progressionId, achievement.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking sprint achievements for user {UserId}", userId);
            }
        }

        private async Task UnlockAchievementAsync(int userId, int progressionId, int achievementId)
        {
            try
            {
                var alreadyUnlocked = await _context.Set<UserAchievement>()
                    .AnyAsync(x => x.UserId == userId && x.AchievementId == achievementId);

                if (alreadyUnlocked)
                {
                    return; // Already unlocked
                }

                var achievement = await _context.Set<Achievement>().FindAsync(achievementId);
                if (achievement == null)
                {
                    return;
                }

                var userAchievement = new UserAchievement
                {
                    UserId = userId,
                    UserProgressionId = progressionId,
                    AchievementId = achievementId,
                    UnlockedDate = DateTime.UtcNow,
                    ProgressPercentage = 100,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId.ToString(),
                    ViewState = true
                };

                _context.Set<UserAchievement>().Add(userAchievement);

                // اعطای پاداش دستاورد به کیف پول کاربر
                var wallet = await _context.Set<UserWallet>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (wallet != null && achievement.RewardPoints > 0)
                {
                    wallet.TotalPoints += achievement.RewardPoints;
                    wallet.AvailablePoints += achievement.RewardPoints;
                    wallet.LastUpdated = DateTime.UtcNow;
                    wallet.ChangeDate = DateTime.UtcNow;

                    _context.Set<WalletTransaction>().Add(new WalletTransaction
                    {
                        UserWalletId = wallet.Id,
                        UserProgressionId = progressionId,
                        Amount = achievement.RewardPoints,
                        TransactionType = TransactionType.Earned,
                        Description = $"دستاورد «{achievement.Name}»",
                        TransactionDate = DateTime.UtcNow,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        ViewState = true
                    });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Achievement {AchievementId} unlocked for user {UserId}", achievementId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking achievement {AchievementId} for user {UserId}", achievementId, userId);
            }
        }

        public async Task<List<int>> GetUnlockedAchievementsAsync(int userId)
        {
            try
            {
                return await _context.Set<UserAchievement>()
                    .Where(x => x.UserId == userId)
                    .Select(x => x.AchievementId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unlocked achievements for user {UserId}", userId);
                return new List<int>();
            }
        }

        public async Task<List<int>> GetUnlockedMilestonesAsync(int userId)
        {
            try
            {
                return await _context.Set<UserMilestoneProgress>()
                    .Where(x => x.UserId == userId && x.IsCompleted)
                    .Select(x => x.MilestoneId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unlocked milestones for user {UserId}", userId);
                return new List<int>();
            }
        }
    }
}
