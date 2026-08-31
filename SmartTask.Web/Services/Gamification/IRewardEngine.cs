/*
| Module      : Gamification
| Interface   : IRewardEngine
| Purpose     : رابط برای موتور محاسبه و توزیع پاداش‌ها
*/

namespace SmartTask.Web.Services.Gamification
{
    public interface IRewardEngine
    {
        Task<int> CalculateTaskCompletionRewardAsync(int taskId, int userId, int priority, int complexity);
        Task<int> CalculateProjectCompletionRewardAsync(int projectId, int totalTasks);
        Task<int> CalculateSprintCompletionRewardAsync(int sprintId, int completedTasks, int totalTasks);
        Task AwardRewardAsync(int userId, int points, string description, int? relatedTaskId = null, int experience = 0);

        /// <summary>
        /// ایجاد کیف‌پول و پیشرفت کاربر در صورت نبودن (برای کاربران جدید)
        /// </summary>
        Task EnsureUserAccountsAsync(int userId);
        Task<(int BasePoints, int PriorityModifier, int ComplexityModifier, int StreakBonus, int TimeBonus)> GetRewardBreakdownAsync(int taskId, int userId, int priority, int complexity);
    }
}
