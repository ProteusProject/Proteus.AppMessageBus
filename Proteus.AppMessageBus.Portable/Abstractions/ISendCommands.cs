using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface ISendCommands
    {
        Task Send<TCommand>(TCommand command) where TCommand : ICommand;

    }
}