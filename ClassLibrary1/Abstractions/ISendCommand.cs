namespace ClassLibrary1.Abstractions
{
    public interface ISendCommand
    {
        void Send<TCommand>(TCommand command) where TCommand : Command;

    }
}