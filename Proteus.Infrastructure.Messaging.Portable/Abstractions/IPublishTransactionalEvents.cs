namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IPublishTransactionalEvents
    {
        void PublishTx<TEvent>(TEvent @event) where TEvent : IEvent, IMessageTx;
        void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IEvent, IMessageTx;

    }
}