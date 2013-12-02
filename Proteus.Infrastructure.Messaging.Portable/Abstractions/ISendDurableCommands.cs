namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendDurableCommands
    {
        void SendTx<TCommand>(TCommand command) where TCommand : ICommand, IDurableMessage;
        void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IDurableMessage;
    }
}