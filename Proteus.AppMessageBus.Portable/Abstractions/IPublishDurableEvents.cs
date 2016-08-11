using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IPublishDurableEvents
    {
        Task PublishDurable<TEvent>(TEvent @event) where TEvent : IDurableEvent;
        Task PublishDurable<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IDurableEvent;

    }
}