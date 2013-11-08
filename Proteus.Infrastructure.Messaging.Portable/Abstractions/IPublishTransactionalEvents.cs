namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IPublishTransactionalEvents
    {
        void PublishTx<TEvent>(TEvent @event) where TEvent : IMessageTx;
        void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IMessageTx;

    }
}