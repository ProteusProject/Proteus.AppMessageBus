using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IPublishEvents
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}