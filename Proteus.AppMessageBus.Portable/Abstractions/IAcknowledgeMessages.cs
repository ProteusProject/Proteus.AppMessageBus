using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IAcknowledgeMessages
    {
        Task Acknowledge<TMessage>(TMessage message) where TMessage : IDurableMessage;
    }
}