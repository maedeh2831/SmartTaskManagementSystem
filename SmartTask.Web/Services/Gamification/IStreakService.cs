/*
| Module      : Gamification
| Interface   : IStreakService
| Purpose     : تعریف قرارداد خدمات رشته‌های بهره‌وری
*/

namespace SmartTask.Web.Services.Gamification
{
    public interface IStreakService
    {
        Task<int> GetCurrentStreakAsync(int userId);
        Task<int> GetLongestStreakAsync(int userId);
        Task UpdateStreakAsync(int userId, int xpGained);
        Task ResetStreaksAsync();
        Task<(int current, int longest, int milestonesReached)> CheckMilestonesAsync(int userId);
        Task<DateTime> GetNextResetTimeAsync(int userId);
        Task SetUserTimeZoneAsync(int userId, string timeZone);
    }
}
