namespace Proteus.Infrastructure.Messaging.Abstractions
{
    public interface IPublishEvents
    {
        void Publish<TEvent>(TEvent @event) where TEvent : Event;
    }
}