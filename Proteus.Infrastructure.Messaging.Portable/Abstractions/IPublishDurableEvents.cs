namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IPublishDurableEvents
    {
        void PublishTx<TEvent>(TEvent @event) where TEvent : IDurableMessage;
        void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IDurableMessage;

    }
}