using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IPublishEvents
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}