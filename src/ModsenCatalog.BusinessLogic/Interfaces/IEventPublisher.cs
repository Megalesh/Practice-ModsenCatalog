namespace ModsenCatalog.BusinessLogic.Events;

public interface IEventPublisher
{
    void Publish<T>(T eventItem) where T : DomainEvent;

    void Subscribe<T>(Action<T> handler) where T : DomainEvent;
}