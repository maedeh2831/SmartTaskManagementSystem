/*
| Module      : Gamification
| Class       : TaskRewardCoordinator
| Purpose     : اعطای امتیاز، تجربه، سطح و دستاورد پس از تکمیل تسک + اطلاع‌رسانی
*/

using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Gamification
{
    public class TaskRewardCoordinator : ITaskRewardCoordinator
    {
        private readonly ApplicationDbContext _context;
        private readonly IRewardEngine _rewardEngine;
        private readonly IAchievementEngine _achievementEngine;
        private readonly INotificationService _notificationService;
        private readonly IEquippedCosmeticsService _cosmeticsService;
        private readonly IRewardEligibilityService _eligibilityService;
        private readonly ILogger<TaskRewardCoordinator> _logger;

        public TaskRewardCoordinator(
            ApplicationDbContext context,
            IRewardEngine rewardEngine,
            IAchievementEngine achievementEngine,
            INotificationService notificationService,
            IEquippedCosmeticsService cosmeticsService,
            IRewardEligibilityService eligibilityService,
            ILogger<TaskRewardCoordinator> logger)
        {
            _context = context;
            _rewardEngine = rewardEngine;
            _achievementEngine = achievementEngine;
            _notificationService = notificationService;
            _cosmeticsService = cosmeticsService;
            _eligibilityService = eligibilityService;
            _logger = logger;
        }

        public async Task EnsureUserGamificationAsync(int userId)
        {
            if (userId <= 0)
                return;

            var hasWallet = await _context.Set<UserWallet>().AnyAsync(x => x.UserId == userId);
            if (!hasWallet)
            {
                _context.Set<UserWallet>().Add(new UserWallet
                {
                    UserId = userId,
                    LastUpdated = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    ViewState = true
                });
            }

            var hasProgression = await _context.Set<UserProgression>().AnyAsync(x => x.UserId == userId);
            if (!hasProgression)
            {
                _context.Set<UserProgression>().Add(new UserProgression
                {
                    UserId = userId,
                    LastProgressUpdate = DateTime.UtcNow,
                    JoinedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    ViewState = true
                });
            }

            if (!hasWallet || !hasProgression)
                await _context.SaveChangesAsync();
        }

        public async Task HandleTaskCompletedAsync(
            int taskId,
            string taskTitle,
            IEnumerable<int> assigneeIds,
            TaskPriorityType priority,
            int estimate)
        {
            var targets = assigneeIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (targets.Count == 0)
                return;

            var complexity = MapEstimateToComplexity(estimate);

            foreach (var userId in targets)
            {
                try
                {
                    await EnsureUserGamificationAsync(userId);

                    // دروازه ضد‌سوء‌استفاده: پاداش تکراری، تسک جعلی، سقف نرخ و تعلیق
                    var eligibility = await _eligibilityService.CanRewardTaskAsync(userId, taskId);
                    if (!eligibility.IsAllowed)
                    {
                        _logger.LogInformation(
                            "Reward skipped for task {TaskId}, user {UserId}: {Reason}",
                            taskId, userId, eligibility.Reason);

                        // پیشرفت شمرده نمی‌شود تا دستاوردها هم قابل سوء‌استفاده نباشند
                        continue;
                    }

                    var points = await _rewardEngine.CalculateTaskCompletionRewardAsync(
                        taskId, userId, (int)priority, complexity);

                    if (points > 0)
                    {
                        await _rewardEngine.AwardRewardAsync(
                            userId, points, $"تکمیل Task «{taskTitle}»", taskId);
                    }

                    var beforeLevel = await GetCurrentLevelAsync(userId);

                    // پیشرفت، دستاوردها و نقاط عطف
                    await _achievementEngine.OnTaskCompletedAsync(taskId, userId);

                    // مزایای فعال فروشگاه ضریب تجربه را افزایش می‌دهند
                    var multiplier = await _cosmeticsService.GetExperienceMultiplierAsync(userId);
                    var experience = points > 0 ? (int)Math.Round(points * 2 * multiplier) : 0;
                    var afterLevel = await ApplyExperienceAsync(userId, experience);

                    await NotifyRewardAsync(userId, taskTitle, points, experience, multiplier);

                    if (afterLevel > beforeLevel)
                        await NotifyLevelUpAsync(userId, afterLevel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing task completion rewards for task {TaskId}, user {UserId}",
                        taskId, userId);
                }
            }
        }

        /// <summary>
        /// نگاشت تخمین ساعت به سطح پیچیدگی ۱ تا ۵
        /// </summary>
        private static int MapEstimateToComplexity(int estimate) => estimate switch
        {
            <= 0 => 3,
            <= 2 => 1,
            <= 4 => 2,
            <= 8 => 3,
            <= 16 => 4,
            _ => 5
        };

        public async Task HandleSprintCompletedAsync(int sprintId)
        {
            try
            {
                var sprint = await _context.Sprints
                    .FirstOrDefaultAsync(x => x.Id == sprintId);

                if (sprint == null)
                    return;

                // مشارکت‌کنندگان: مالکان استوری‌ها و افراد تخصیص‌داده‌شده به تسک‌های اسپرینت
                var storyOwners = await _context.UserStories
                    .Where(s => s.SprintId == sprintId && s.OwnerId != null)
                    .Select(s => s.OwnerId!.Value)
                    .ToListAsync();

                var taskAssignees = await _context.TaskItems
                    .Where(t => t.UserStory.SprintId == sprintId)
                    .SelectMany(t => t.Assignments.Where(a => a.ViewState))
                    .Select(a => a.ApplicationUserId)
                    .ToListAsync();

                var totalTasks = await _context.TaskItems
                    .CountAsync(t => t.UserStory.SprintId == sprintId);

                var completedTasks = await _context.TaskItems
                    .CountAsync(t => t.UserStory.SprintId == sprintId && t.Status == TaskStatusType.Done);

                var participants = storyOwners
                    .Concat(taskAssignees)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (participants.Count == 0)
                    return;

                var points = await _rewardEngine.CalculateSprintCompletionRewardAsync(
                    sprintId, completedTasks, totalTasks);

                foreach (var userId in participants)
                {
                    try
                    {
                        await EnsureUserGamificationAsync(userId);

                        if (points > 0)
                        {
                            await _rewardEngine.AwardRewardAsync(
                                userId, points, $"تکمیل اسپرینت «{sprint.Name}»");
                        }

                        var beforeLevel = await GetCurrentLevelAsync(userId);

                        // شمارنده اسپرینت و بررسی دستاوردهای مرتبط
                        await _achievementEngine.OnSprintCompletedAsync(sprintId, userId);

                        var multiplier = await _cosmeticsService.GetExperienceMultiplierAsync(userId);
                        var experience = points > 0 ? (int)Math.Round(points * 2 * multiplier) : 0;
                        var afterLevel = await ApplyExperienceAsync(userId, experience);

                        await _notificationService.CreateAsync(
                            userId,
                            "🏁 اسپرینت تکمیل شد",
                            $"اسپرینت «{sprint.Name}» بسته شد. {points} امتیاز و {experience} تجربه دریافت کردید.",
                            NotificationType.System);

                        if (afterLevel > beforeLevel)
                            await NotifyLevelUpAsync(userId, afterLevel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error processing sprint completion rewards for sprint {SprintId}, user {UserId}",
                            sprintId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling sprint completion for sprint {SprintId}", sprintId);
            }
        }

        public async Task HandleProjectCompletedAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects
                    .FirstOrDefaultAsync(x => x.Id == projectId);

                if (project == null)
                    return;

                var members = await _context.ProjectMembers
                    .Where(m => m.ProjectId == projectId && m.ViewState)
                    .Select(m => m.ApplicationUserId)
                    .Distinct()
                    .ToListAsync();

                var participants = members.Where(id => id > 0).ToList();
                if (participants.Count == 0)
                    return;

                var totalTasks = await _context.TaskItems
                    .CountAsync(t => t.UserStory.ProjectId == projectId);

                var points = await _rewardEngine.CalculateProjectCompletionRewardAsync(projectId, totalTasks);

                foreach (var userId in participants)
                {
                    try
                    {
                        await EnsureUserGamificationAsync(userId);

                        if (points > 0)
                        {
                            await _rewardEngine.AwardRewardAsync(
                                userId, points, $"تکمیل پروژه «{project.Name}»");
                        }

                        var beforeLevel = await GetCurrentLevelAsync(userId);

                        await _achievementEngine.OnProjectCompletedAsync(projectId, userId);

                        var multiplier = await _cosmeticsService.GetExperienceMultiplierAsync(userId);
                        var experience = points > 0 ? (int)Math.Round(points * 2 * multiplier) : 0;
                        var afterLevel = await ApplyExperienceAsync(userId, experience);

                        await _notificationService.CreateAsync(
                            userId,
                            "🎯 پروژه تکمیل شد",
                            $"پروژه «{project.Name}» تکمیل شد. {points} امتیاز و {experience} تجربه دریافت کردید.",
                            NotificationType.System);

                        if (afterLevel > beforeLevel)
                            await NotifyLevelUpAsync(userId, afterLevel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error processing project completion rewards for project {ProjectId}, user {UserId}",
                            projectId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling project completion for project {ProjectId}", projectId);
            }
        }

        private async Task<int> GetCurrentLevelAsync(int userId)
        {
            return await _context.Set<UserProgression>()
                .Where(x => x.UserId == userId)
                .Select(x => x.CurrentLevel)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// افزودن تجربه و ارتقای سطح در صورت رسیدن به آستانه
        /// </summary>
        private async Task<int> ApplyExperienceAsync(int userId, int experience)
        {
            var progression = await _context.Set<UserProgression>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (progression == null)
                return 0;

            if (experience > 0)
            {
                progression.TotalExperience += experience;

                while (progression.TotalExperience >= progression.ExperienceForNextLevel)
                {
                    progression.CurrentLevel += 1;
                    progression.ExperienceForNextLevel =
                        (int)(progression.ExperienceForNextLevel * 1.5);
                }

                progression.LastProgressUpdate = DateTime.UtcNow;
                progression.ChangeDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return progression.CurrentLevel;
        }

        private async Task NotifyRewardAsync(int userId, string taskTitle, int points, int experience, double multiplier)
        {
            if (points <= 0 && experience <= 0)
                return;

            // اگر مزیتی فعال باشد، در پیام ذکر می‌شود
            var bonusNote = multiplier > 1.0
                ? $" (شامل ضریب {multiplier:0.#}× از مزایای فعال)"
                : string.Empty;

            await _notificationService.CreateAsync(
                userId,
                "🎉 پاداش تکمیل Task",
                $"برای تکمیل Task «{taskTitle}» مقدار {points} امتیاز و {experience} تجربه دریافت کردید.{bonusNote}",
                NotificationType.System);
        }

        private async Task NotifyLevelUpAsync(int userId, int newLevel)
        {
            await _notificationService.CreateAsync(
                userId,
                "⭐ ارتقای سطح",
                $"تبریک! شما به سطح {newLevel} ارتقا یافتید.",
                NotificationType.System);
        }
    }
}
