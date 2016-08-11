using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IMessagePersistenceAsync
    {
        IFileSystemProviderAsync FileSystemProvider { set; }
        Task<string> LoadCommands();
        Task<string> LoadEvents();
        Task SaveCommands(string commands);
        Task SaveEvents(string events);
        Task RemoveAllCommandsFromPersistence();
        Task RemoveAllEventsFromPersistence();
        Task<bool> CheckForCommands();
        Task<bool> CheckForEvents();
    }
}