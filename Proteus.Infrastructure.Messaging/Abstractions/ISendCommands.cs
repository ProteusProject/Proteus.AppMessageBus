namespace Proteus.Infrastructure.Messaging.Abstractions
{
    public interface ISendCommands
    {
        void Send<TCommand>(TCommand command) where TCommand : Command;

    }
}