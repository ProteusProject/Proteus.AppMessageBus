using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISendCommands
    {
        Task Send<TCommand>(TCommand command) where TCommand : ICommand;

    }
}