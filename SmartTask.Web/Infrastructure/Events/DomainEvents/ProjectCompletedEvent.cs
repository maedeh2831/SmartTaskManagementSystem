/*
| Module      : Gamification
| Event       : ProjectCompletedEvent
| Purpose     : رویداد تکمیل شدن یک پروژه
*/

namespace SmartTask.Web.Infrastructure.Events.DomainEvents
{
    public class ProjectCompletedEvent : IDomainEvent
    {
        public int AggregateId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => nameof(ProjectCompletedEvent);

        public int ProjectId { get; }
        public int WorkspaceId { get; }
        public DateTime CompletedAt { get; }
        public int TotalTasks { get; }

        public ProjectCompletedEvent(int projectId, int workspaceId, int totalTasks, DateTime completedAt)
        {
            AggregateId = projectId;
            OccurredAt = DateTime.UtcNow;
            ProjectId = projectId;
            WorkspaceId = workspaceId;
            CompletedAt = completedAt;
            TotalTasks = totalTasks;
        }
    }
}
