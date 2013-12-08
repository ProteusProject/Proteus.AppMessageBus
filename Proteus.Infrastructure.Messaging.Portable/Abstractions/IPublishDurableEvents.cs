namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IPublishDurableEvents
    {
        void PublishDurable<TEvent>(TEvent @event) where TEvent : IDurableEvent;
        void PublishDurable<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IDurableEvent;

    }
}