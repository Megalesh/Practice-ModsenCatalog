namespace ModsenCatalog.BusinessLogic.Events;

public class EventPublisher : IEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : DomainEvent
    {
        var eventType = typeof(T);

        if (!handlers.ContainsKey(eventType))
        {
            handlers[eventType] = new List<Delegate>();
        }

        handlers[eventType].Add(handler);
    }

    public void Publish<T>(T eventItem) where T : DomainEvent
    {
        var eventType = typeof(T);

        if (handlers.TryGetValue(eventType, out var _handlers))
        {
            foreach (var handler in _handlers)
            {
                ((Action<T>)handler).Invoke(eventItem);
            }
        }
    }
}