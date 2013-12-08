namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendDurableCommands
    {
        void SendDurable<TCommand>(TCommand command) where TCommand : IDurableCommand;
        void SendDurable<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : IDurableCommand;
    }
}