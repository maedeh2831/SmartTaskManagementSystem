/*
| Module      : Gamification
| Interface   : IMilestoneService
| Purpose     : تعریف قراردادی برای سرویس نقاط عطف
*/

using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Gamification
{
    public interface IMilestoneService
    {
        Task<List<Milestone>> GetAllMilestonesAsync();
        Task<Milestone> GetMilestoneByIdAsync(int id);
        Task<List<UserMilestoneProgress>> GetUserMilestoneProgressAsync(int userId);
        Task<UserMilestoneProgress> GetUserMilestoneProgressByIdAsync(int userId, int milestoneId);
        Task<int> GetUserMilestoneCompletionPercentageAsync(int userId, int milestoneId);
    }
}
