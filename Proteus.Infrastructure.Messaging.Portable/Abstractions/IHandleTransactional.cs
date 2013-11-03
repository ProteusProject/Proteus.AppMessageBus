namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IHandleTransactional<TMessage> where TMessage : IMessage
    {
        void Handle(Envelope<TMessage> message);
    }
}