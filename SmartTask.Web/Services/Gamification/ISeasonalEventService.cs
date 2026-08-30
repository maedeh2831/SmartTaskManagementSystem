/*
| Module      : Gamification
| Interface   : ISeasonalEventService
| Purpose     : تعریف قرارداد خدمات رویدادهای فصلی
*/

namespace SmartTask.Web.Services.Gamification
{
    public interface ISeasonalEventService
    {
        Task<List<dynamic>> GetActiveEventsAsync();
        Task<dynamic> GetEventAsync(int eventId);
        Task CreateEventAsync(dynamic eventData);
        Task UpdateEventAsync(int eventId, dynamic eventData);
        Task DeleteEventAsync(int eventId);
        Task ActivateEventAsync(int eventId);
        Task DeactivateEventAsync(int eventId);
        Task JoinEventAsync(int userId, int eventId);
        Task LeaveEventAsync(int userId, int eventId);
        Task UpdateUserProgressAsync(int userId, int eventId, int points);
        Task<List<dynamic>> GetEventLeaderboardAsync(int eventId);
        Task ProcessSeasonalAwardsAsync();
        Task UpdateEventStatusesAsync();
    }
}
