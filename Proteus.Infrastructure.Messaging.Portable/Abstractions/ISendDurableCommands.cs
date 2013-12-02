namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendDurableCommands
    {
        void SendDurable<TCommand>(TCommand command) where TCommand : ICommand, IDurableMessage;
        void SendDurable<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IDurableMessage;
    }
}