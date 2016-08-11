namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IDurableCommand : ICommand, IDurableMessage
    {
    }
}