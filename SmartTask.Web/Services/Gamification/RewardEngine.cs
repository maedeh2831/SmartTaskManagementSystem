/*
| Module      : Gamification
| Class       : RewardEngine
| Purpose     : پیاده‌سازی موتور محاسبه و توزیع پاداش‌ها
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class RewardEngine : IRewardEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly RewardCalculator _calculator;
        private readonly ILogger<RewardEngine> _logger;

        public RewardEngine(ApplicationDbContext context, ILogger<RewardEngine> logger)
        {
            _context = context;
            _calculator = new RewardCalculator();
            _logger = logger;
        }

        public async Task<int> CalculateTaskCompletionRewardAsync(int taskId, int userId, int priority, int complexity)
        {
            try
            {
                var task = await _context.TaskItems.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", taskId);
                    return 0;
                }

                var userProgression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (userProgression == null)
                {
                    _logger.LogWarning("UserProgression for user {UserId} not found", userId);
                    return 0;
                }

                var priorityModifier = _calculator.GetPriorityModifier(priority);
                var complexityModifier = _calculator.GetComplexityModifier(complexity);
                var streakBonus = _calculator.CalculateStreakBonus(userProgression.TasksCompleted);
                var timeBonus = _calculator.CalculateTimeBonus(task.CreatedDate, DateTime.UtcNow);

                return _calculator.CalculateTaskReward(priority, complexity, priorityModifier, complexityModifier, streakBonus, timeBonus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating task reward for task {TaskId}, user {UserId}", taskId, userId);
                return 100; // Default reward
            }
        }

        public async Task<int> CalculateProjectCompletionRewardAsync(int projectId, int totalTasks)
        {
            try
            {
                return _calculator.CalculateProjectReward(totalTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating project reward for project {ProjectId}", projectId);
                return 500; // Default reward
            }
        }

        public async Task<int> CalculateSprintCompletionRewardAsync(int sprintId, int completedTasks, int totalTasks)
        {
            try
            {
                return _calculator.CalculateSprintReward(completedTasks, totalTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating sprint reward for sprint {SprintId}", sprintId);
                return 300; // Default reward
            }
        }

        public async Task EnsureUserAccountsAsync(int userId)
        {
            try
            {
                var wallet = await _context.Set<UserWallet>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (wallet == null)
                {
                    _context.Set<UserWallet>().Add(new UserWallet
                    {
                        UserId = userId,
                        TotalPoints = 0,
                        AvailablePoints = 0,
                        SpentPoints = 0,
                        LastUpdated = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        ViewState = true
                    });
                }

                var progression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (progression == null)
                {
                    _context.Set<UserProgression>().Add(new UserProgression
                    {
                        UserId = userId,
                        CurrentLevel = 1,
                        TotalExperience = 0,
                        ExperienceForNextLevel = 1000,
                        LastProgressUpdate = DateTime.UtcNow,
                        JoinedDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        ViewState = true
                    });
                }

                if (wallet == null || progression == null)
                    await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring gamification accounts for user {UserId}", userId);
            }
        }

        public async Task AwardRewardAsync(int userId, int points, string description, int? relatedTaskId = null, int experience = 0)
        {
            try
            {
                await EnsureUserAccountsAsync(userId);

                var wallet = await _context.Set<UserWallet>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (wallet == null)
                {
                    _logger.LogWarning("UserWallet for user {UserId} not found", userId);
                    return;
                }

                var progression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (progression == null)
                {
                    _logger.LogWarning("UserProgression for user {UserId} not found", userId);
                    return;
                }

                wallet.TotalPoints += points;
                wallet.AvailablePoints += points;
                wallet.LastUpdated = DateTime.UtcNow;
                wallet.ChangeDate = DateTime.UtcNow;

                // XP و ارتقای سطح
                if (experience > 0)
                {
                    progression.TotalExperience += experience;

                    while (progression.TotalExperience >= progression.ExperienceForNextLevel)
                    {
                        progression.TotalExperience -= progression.ExperienceForNextLevel;
                        progression.CurrentLevel += 1;
                        progression.ExperienceForNextLevel = progression.CurrentLevel * 1000;
                    }

                    progression.LastProgressUpdate = DateTime.UtcNow;
                }

                var transaction = new WalletTransaction
                {
                    UserWalletId = wallet.Id,
                    UserProgressionId = progression.Id,
                    Amount = points,
                    TransactionType = TransactionType.Earned,
                    Description = description,
                    RelatedTaskId = relatedTaskId,
                    TransactionDate = DateTime.UtcNow,
                    CreatedBy = userId.ToString(),
                    CreatedDate = DateTime.UtcNow
                };

                _context.Set<WalletTransaction>().Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Awarded {Points} points to user {UserId} for {Description}", points, userId, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding reward to user {UserId}", userId);
            }
        }

        public async Task<(int BasePoints, int PriorityModifier, int ComplexityModifier, int StreakBonus, int TimeBonus)> GetRewardBreakdownAsync(int taskId, int userId, int priority, int complexity)
        {
            try
            {
                var task = await _context.TaskItems.FindAsync(taskId);
                if (task == null)
                    return (0, 0, 0, 0, 0);

                var userProgression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (userProgression == null)
                    return (0, 0, 0, 0, 0);

                var basePoints = 100;
                var priorityMod = _calculator.GetPriorityModifier(priority);
                var complexityMod = _calculator.GetComplexityModifier(complexity);
                var streakBonus = _calculator.CalculateStreakBonus(userProgression.TasksCompleted);
                var timeBonus = _calculator.CalculateTimeBonus(task.CreatedDate, DateTime.UtcNow);

                return (basePoints, priorityMod, complexityMod, streakBonus, timeBonus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reward breakdown for task {TaskId}", taskId);
                return (0, 0, 0, 0, 0);
            }
        }
    }
}
