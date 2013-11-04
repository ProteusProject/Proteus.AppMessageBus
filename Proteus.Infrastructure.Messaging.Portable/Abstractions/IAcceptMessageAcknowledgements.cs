namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IAcceptMessageAcknowledgements
    {
        void Acknowledge<TMessage>(TMessage message) where TMessage : IMessageTx;
    }
}