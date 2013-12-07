namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IAcknowledgeMessages
    {
        void Acknowledge<TMessage>(TMessage message) where TMessage : IDurableMessage;
    }
}