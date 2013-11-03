namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendTransactionalCommands
    {
        void SendTx<TCommand>(TCommand command) where TCommand : Command;
        void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : Command;
         
    }
}