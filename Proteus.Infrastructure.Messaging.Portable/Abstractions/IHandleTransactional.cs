namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IHandleTransactional<in TMessage> where TMessage : IMessageTx
    {
        void Handle(TMessage message);
    }
}