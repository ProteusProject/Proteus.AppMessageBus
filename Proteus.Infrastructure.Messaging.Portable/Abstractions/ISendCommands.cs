namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendCommands
    {
        void Send<TCommand>(TCommand command) where TCommand : Command;

    }
}