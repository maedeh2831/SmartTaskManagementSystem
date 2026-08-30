/*
| Module      : Gamification
| Event       : SprintCompletedEvent
| Purpose     : رویداد تکمیل شدن یک اسپرینت
*/

namespace SmartTask.Web.Infrastructure.Events.DomainEvents
{
    public class SprintCompletedEvent : IDomainEvent
    {
        public int AggregateId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => nameof(SprintCompletedEvent);

        public int SprintId { get; }
        public int ProjectId { get; }
        public DateTime CompletedAt { get; }
        public int CompletedTasks { get; }
        public int TotalTasks { get; }

        public SprintCompletedEvent(int sprintId, int projectId, int completedTasks, int totalTasks, DateTime completedAt)
        {
            AggregateId = sprintId;
            OccurredAt = DateTime.UtcNow;
            SprintId = sprintId;
            ProjectId = projectId;
            CompletedAt = completedAt;
            CompletedTasks = completedTasks;
            TotalTasks = totalTasks;
        }
    }
}
