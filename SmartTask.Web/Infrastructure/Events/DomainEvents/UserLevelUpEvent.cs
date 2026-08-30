/*
| Module      : Gamification
| Event       : UserLevelUpEvent
| Purpose     : رویداد ارتقاء سطح کاربر
*/

namespace SmartTask.Web.Infrastructure.Events.DomainEvents
{
    public class UserLevelUpEvent : IDomainEvent
    {
        public int AggregateId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => nameof(UserLevelUpEvent);

        public int UserId { get; }
        public int PreviousLevel { get; }
        public int NewLevel { get; }
        public int TotalExperience { get; }

        public UserLevelUpEvent(int userId, int previousLevel, int newLevel, int totalExperience)
        {
            AggregateId = userId;
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            TotalExperience = totalExperience;
        }
    }
}
