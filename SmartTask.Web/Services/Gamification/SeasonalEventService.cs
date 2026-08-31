/*
| Module      : Gamification
| Class       : SeasonalEventService
| Purpose     : پیاده‌سازی خدمات رویدادهای فصلی محدود‌الزمان
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SmartTask.Web.Services.Gamification
{
    public class SeasonalEventService : ISeasonalEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeasonalEventService> _logger;

        public SeasonalEventService(ApplicationDbContext context, ILogger<SeasonalEventService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<dynamic>> GetActiveEventsAsync()
        {
            try
            {
                var events = await _context.Set<SeasonalEvent>()
                    .Where(e => e.IsActive && e.Status == EventStatus.Active)
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.Description,
                        e.Icon,
                        e.Color,
                        e.StartDate,
                        e.EndDate,
                        e.AchievementBonusMultiplier,
                        e.RewardBonusMultiplier,
                        e.ExtraPointsPerCompletion,
                        e.CurrentParticipants,
                        e.MaxParticipants
                    })
                    .ToListAsync();

                return events.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active events");
                return new List<dynamic>();
            }
        }

        public async Task<dynamic> GetEventAsync(int eventId)
        {
            try
            {
                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                    return null;

                return new
                {
                    eventEntity.Id,
                    eventEntity.Name,
                    eventEntity.Description,
                    eventEntity.Icon,
                    eventEntity.Color,
                    eventEntity.StartDate,
                    eventEntity.EndDate,
                    eventEntity.Status,
                    eventEntity.IsActive,
                    eventEntity.AchievementBonusMultiplier,
                    eventEntity.RewardBonusMultiplier,
                    eventEntity.ExtraPointsPerCompletion,
                    eventEntity.CurrentParticipants,
                    eventEntity.MaxParticipants,
                    eventEntity.HasEventLeaderboard
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event {EventId}", eventId);
                return null;
            }
        }

        public async Task CreateEventAsync(dynamic eventData)
        {
            try
            {
                var newEvent = new SeasonalEvent
                {
                    Name = eventData.Name,
                    Description = eventData.Description,
                    Icon = eventData.Icon,
                    Color = eventData.Color,
                    StartDate = eventData.StartDate,
                    EndDate = eventData.EndDate,
                    Status = EventStatus.Scheduled,
                    IsActive = false,
                    AchievementBonusMultiplier = eventData.AchievementBonusMultiplier ?? 1.0m,
                    RewardBonusMultiplier = eventData.RewardBonusMultiplier ?? 1.0m,
                    ExtraPointsPerCompletion = eventData.ExtraPointsPerCompletion ?? 0,
                    EligibilityCriteria = eventData.EligibilityCriteria ?? "{}",
                    MaxParticipants = eventData.MaxParticipants ?? -1,
                    HasEventLeaderboard = eventData.HasEventLeaderboard ?? true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _context.Set<SeasonalEvent>().Add(newEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created seasonal event: {EventName}", newEvent.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seasonal event");
            }
        }

        public async Task UpdateEventAsync(int eventId, dynamic eventData)
        {
            try
            {
                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                    return;

                eventEntity.Name = eventData.Name ?? eventEntity.Name;
                eventEntity.Description = eventData.Description ?? eventEntity.Description;
                eventEntity.Icon = eventData.Icon ?? eventEntity.Icon;
                eventEntity.Color = eventData.Color ?? eventEntity.Color;
                eventEntity.AchievementBonusMultiplier = eventData.AchievementBonusMultiplier ?? eventEntity.AchievementBonusMultiplier;
                eventEntity.RewardBonusMultiplier = eventData.RewardBonusMultiplier ?? eventEntity.RewardBonusMultiplier;
                eventEntity.ExtraPointsPerCompletion = eventData.ExtraPointsPerCompletion ?? eventEntity.ExtraPointsPerCompletion;
                eventEntity.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated seasonal event: {EventId}", eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating seasonal event {EventId}", eventId);
            }
        }

        public async Task DeleteEventAsync(int eventId)
        {
            try
            {
                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                    return;

                _context.Set<SeasonalEvent>().Remove(eventEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted seasonal event: {EventId}", eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting seasonal event {EventId}", eventId);
            }
        }

        public async Task ActivateEventAsync(int eventId)
        {
            try
            {
                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                    return;

                eventEntity.IsActive = true;
                eventEntity.Status = EventStatus.Active;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Activated seasonal event: {EventId}", eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating seasonal event {EventId}", eventId);
            }
        }

        public async Task DeactivateEventAsync(int eventId)
        {
            try
            {
                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                    return;

                eventEntity.IsActive = false;
                eventEntity.Status = EventStatus.Ended;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated seasonal event: {EventId}", eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating seasonal event {EventId}", eventId);
            }
        }

        public async Task JoinEventAsync(int userId, int eventId)
        {
            try
            {
                var existingProgress = await _context.Set<UserSeasonalEventProgress>()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.SeasonalEventId == eventId);

                if (existingProgress != null)
                    return;

                var progress = new UserSeasonalEventProgress
                {
                    UserId = userId,
                    SeasonalEventId = eventId,
                    JoinedDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                _context.Set<UserSeasonalEventProgress>().Add(progress);

                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity != null)
                {
                    eventEntity.CurrentParticipants++;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} joined event {EventId}", userId, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining event for user {UserId}", userId);
            }
        }

        public async Task LeaveEventAsync(int userId, int eventId)
        {
            try
            {
                var progress = await _context.Set<UserSeasonalEventProgress>()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.SeasonalEventId == eventId);

                if (progress == null)
                    return;

                progress.IsActive = false;

                var eventEntity = await _context.Set<SeasonalEvent>()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity != null && eventEntity.CurrentParticipants > 0)
                {
                    eventEntity.CurrentParticipants--;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} left event {EventId}", userId, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving event for user {UserId}", userId);
            }
        }

        public async Task UpdateUserProgressAsync(int userId, int eventId, int points)
        {
            try
            {
                var progress = await _context.Set<UserSeasonalEventProgress>()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.SeasonalEventId == eventId);

                if (progress == null)
                    return;

                progress.EventPoints += points;
                progress.TasksCompleted++;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated progress for user {UserId} in event {EventId}: +{Points} points",
                    userId, eventId, points);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user progress for event {EventId}", eventId);
            }
        }

        public async Task<List<dynamic>> GetEventLeaderboardAsync(int eventId)
        {
            try
            {
                var leaderboard = await _context.Set<UserSeasonalEventProgress>()
                    .Where(p => p.SeasonalEventId == eventId && p.IsActive)
                    .OrderByDescending(p => p.EventPoints)
                    .ThenByDescending(p => p.TasksCompleted)
                    .Select((p, index) => new
                    {
                        Rank = index + 1,
                        UserId = p.UserId,
                        UserName = p.User.UserName,
                        Points = p.EventPoints,
                        TasksCompleted = p.TasksCompleted,
                        AchievementsUnlocked = p.AchievementsUnlocked
                    })
                    .ToListAsync();

                return leaderboard.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event leaderboard for event {EventId}", eventId);
                return new List<dynamic>();
            }
        }

        public async Task ProcessSeasonalAwardsAsync()
        {
            try
            {
                var expiredEvents = await _context.Set<SeasonalEvent>()
                    .Where(e => e.EndDate <= DateTime.UtcNow && e.Status != EventStatus.Ended)
                    .ToListAsync();

                foreach (var eventEntity in expiredEvents)
                {
                    eventEntity.Status = EventStatus.Ended;
                    eventEntity.IsActive = false;
                }

                var upcomingEvents = await _context.Set<SeasonalEvent>()
                    .Where(e => e.StartDate <= DateTime.UtcNow && e.EndDate > DateTime.UtcNow && !e.IsActive)
                    .ToListAsync();

                foreach (var eventEntity in upcomingEvents)
                {
                    eventEntity.Status = EventStatus.Active;
                    eventEntity.IsActive = true;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Processed seasonal event status updates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing seasonal awards");
            }
        }

        public async Task UpdateEventStatusesAsync()
        {
            try
            {
                // Update events that should be activated
                var eventsToActivate = await _context.Set<SeasonalEvent>()
                    .Where(e => e.StartDate <= DateTime.UtcNow &&
                               e.EndDate > DateTime.UtcNow &&
                               e.Status == EventStatus.Scheduled &&
                               !e.IsActive)
                    .ToListAsync();

                foreach (var eventEntity in eventsToActivate)
                {
                    eventEntity.Status = EventStatus.Active;
                    eventEntity.IsActive = true;
                    _logger.LogInformation("Activated seasonal event: {EventName}", eventEntity.Name);
                }

                // Update events that should be deactivated
                var eventsToDeactivate = await _context.Set<SeasonalEvent>()
                    .Where(e => e.EndDate <= DateTime.UtcNow &&
                               e.Status == EventStatus.Active &&
                               e.IsActive)
                    .ToListAsync();

                foreach (var eventEntity in eventsToDeactivate)
                {
                    eventEntity.Status = EventStatus.Ended;
                    eventEntity.IsActive = false;
                    _logger.LogInformation("Deactivated seasonal event: {EventName}", eventEntity.Name);
                }

                if (eventsToActivate.Count > 0 || eventsToDeactivate.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated event statuses: {Activated} activated, {Deactivated} deactivated",
                        eventsToActivate.Count, eventsToDeactivate.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event statuses");
            }
        }
    }
}
