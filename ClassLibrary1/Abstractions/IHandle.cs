namespace ClassLibrary1.Abstractions
{
    public interface IHandle<in TMessage>
    {
        void Handle(TMessage message);
    }
}