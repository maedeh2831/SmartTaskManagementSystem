/*
| Module      : Gamification
| Class       : DomainEventPublisher
| Purpose     : کلاس کمکی برای انتشار رویدادهای دامنه
*/

namespace SmartTask.Web.Infrastructure.Events
{
    public class DomainEventPublisher
    {
        private readonly List<IDomainEvent> _events = new();

        public void AddEvent(IDomainEvent @event)
        {
            _events.Add(@event);
        }

        public IReadOnlyCollection<IDomainEvent> GetEvents()
        {
            return _events.AsReadOnly();
        }

        public void ClearEvents()
        {
            _events.Clear();
        }

        public async Task PublishAllAsync(IEventBus eventBus)
        {
            if (_events.Count == 0)
                return;

            var eventsCopy = _events.ToList();
            _events.Clear();

            await eventBus.PublishAsync(eventsCopy);
        }
    }
}
