namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IHandle<in TMessage>
    {
        void Handle(TMessage message);
    }
}