using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IAcknowledgeMessages
    {
        Task Acknowledge<TMessage>(TMessage message) where TMessage : IDurableMessage;
    }
}