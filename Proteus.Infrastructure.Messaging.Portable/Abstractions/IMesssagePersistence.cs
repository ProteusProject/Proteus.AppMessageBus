using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IMesssagePersistence
    {
        IFileSystemProvider FileSystemProvider { get; set; }
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