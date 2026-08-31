/*
| Module      : Gamification
| Interface   : IEventBus
| Purpose     : رابط برای انتشار و مدیریت رویدادهای دامنه
*/

namespace SmartTask.Web.Infrastructure.Events
{
    public interface IEventBus
    {
        Task PublishAsync(IDomainEvent @event);
        Task PublishAsync(IEnumerable<IDomainEvent> events);
    }
}
