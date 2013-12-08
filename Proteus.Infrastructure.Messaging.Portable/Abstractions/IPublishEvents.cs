namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IPublishEvents
    {
        void Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}