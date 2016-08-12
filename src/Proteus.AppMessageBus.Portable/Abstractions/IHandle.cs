namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IHandle<in TMessage> where TMessage: IMessage
    {
        void Handle(TMessage message);
    }
}