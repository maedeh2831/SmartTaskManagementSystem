/*
| Module      : Gamification
| Interface   : IAchievementEngine
| Purpose     : تعریف قراردادی برای موتور دستاوردها
*/

namespace SmartTask.Web.Services.Gamification
{
    public interface IAchievementEngine
    {
        Task OnTaskCompletedAsync(int taskId, int userId);
        Task OnProjectCompletedAsync(int projectId, int userId);
        Task OnSprintCompletedAsync(int sprintId, int userId);
        Task CheckMilestoneProgressAsync(int userId, string milestoneCondition, int incrementBy = 1);
        Task<List<int>> GetUnlockedAchievementsAsync(int userId);
        Task<List<int>> GetUnlockedMilestonesAsync(int userId);
    }
}
