/*
| Module      : Gamification
| Event       : AchievementUnlockedEvent
| Purpose     : رویداد باز شدن یک دستاورد
*/

namespace SmartTask.Web.Infrastructure.Events.DomainEvents
{
    public class AchievementUnlockedEvent : IDomainEvent
    {
        public int AggregateId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => nameof(AchievementUnlockedEvent);

        public int UserId { get; }
        public int AchievementId { get; }
        public string AchievementName { get; }
        public int RewardPoints { get; }

        public AchievementUnlockedEvent(int userId, int achievementId, string achievementName, int rewardPoints)
        {
            AggregateId = userId;
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            AchievementId = achievementId;
            AchievementName = achievementName;
            RewardPoints = rewardPoints;
        }
    }
}
