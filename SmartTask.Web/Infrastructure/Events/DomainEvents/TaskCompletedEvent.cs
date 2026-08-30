/*
| Module      : Gamification
| Event       : TaskCompletedEvent
| Purpose     : رویداد تکمیل شدن یک تسک
*/

namespace SmartTask.Web.Infrastructure.Events.DomainEvents
{
    public class TaskCompletedEvent : IDomainEvent
    {
        public int AggregateId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => nameof(TaskCompletedEvent);

        public int TaskId { get; }
        public int UserId { get; }
        public int ProjectId { get; }
        public int Priority { get; }
        public int Complexity { get; }
        public DateTime CompletedAt { get; }

        public TaskCompletedEvent(int taskId, int userId, int projectId, int priority, int complexity, DateTime completedAt)
        {
            AggregateId = taskId;
            OccurredAt = DateTime.UtcNow;
            TaskId = taskId;
            UserId = userId;
            ProjectId = projectId;
            Priority = priority;
            Complexity = complexity;
            CompletedAt = completedAt;
        }
    }
}
