/*
| Module      : Gamification
| Interface   : IDomainEvent
| Purpose     : رابط اساسی برای تمام رویدادهای دامنه (Domain Events)
*/

namespace SmartTask.Web.Infrastructure.Events
{
    public interface IDomainEvent
    {
        int AggregateId { get; }
        DateTime OccurredAt { get; }
        string EventType { get; }
    }
}
