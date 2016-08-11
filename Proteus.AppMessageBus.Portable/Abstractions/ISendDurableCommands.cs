using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendDurableCommands
    {
        Task SendDurable<TCommand>(TCommand command) where TCommand : IDurableCommand;
        Task SendDurable<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : IDurableCommand;
    }
}