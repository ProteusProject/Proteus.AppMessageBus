namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IHandleDurable<in TMessage> where TMessage : IDurableMessage
    {
        void Handle(TMessage message);
    }
}