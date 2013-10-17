namespace Proteus.Infrastructure.Messaging.Abstractions
{
    public interface IHandle<in TMessage>
    {
        void Handle(TMessage message);
    }
}