namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendTransactionalCommands
    {
        void SendTx<TCommand>(TCommand command) where TCommand : ICommand, IMessageTx;
        void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IMessageTx;
    }
}