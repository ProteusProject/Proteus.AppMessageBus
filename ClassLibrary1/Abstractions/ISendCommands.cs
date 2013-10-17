namespace ClassLibrary1.Abstractions
{
    public interface ISendCommands
    {
        void Send<TCommand>(TCommand command) where TCommand : Command;

    }
}