/*
| Module      : Gamification
| Class       : InMemoryEventBus
| Purpose     : پیاده‌سازی ساده Event Bus با استفاده از حافظه
*/

namespace SmartTask.Web.Infrastructure.Events
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InMemoryEventBus> _logger;

        public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task PublishAsync(IDomainEvent @event)
        {
            await PublishAsync(new[] { @event });
        }

        public async Task PublishAsync(IEnumerable<IDomainEvent> events)
        {
            foreach (var @event in events)
            {
                try
                {
                    _logger.LogInformation("Publishing event: {EventType} at {OccurredAt}", @event.EventType, @event.OccurredAt);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var handlers = scope.ServiceProvider.GetServices(
                            typeof(IEventHandler<>).MakeGenericType(@event.GetType()));

                        var tasks = handlers
                            .Cast<dynamic>()
                            .Select(h => (Task)h.HandleAsync((dynamic)@event))
                            .ToList();

                        await Task.WhenAll(tasks);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing event: {EventType}", @event.EventType);
                    throw;
                }
            }
        }
    }

    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent @event);
    }
}
