namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IDurableCommand : ICommand, IDurableMessage
    {
    }
}