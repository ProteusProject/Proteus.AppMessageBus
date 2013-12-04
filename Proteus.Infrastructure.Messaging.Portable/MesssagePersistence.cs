using System.Threading.Tasks;
using PCLStorage;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class MesssagePersistence : IMesssagePersistence
    {
        private const string MessagesFolder = "Proteus.Messaging.Messages";
        private const string CommandsDatafile = "Commands.data";
        private const string EventsDatafile = "Events.data";

        public IFileSystemProvider FileSystemProvider { get; set; }

        public MesssagePersistence()
        {
            FileSystemProvider = new FileSystemProvider();
        }

        public async Task<string> LoadCommands()
        {
            return await GetTextFromFile(CommandsDatafile);
        }

        public async Task<string> LoadEvents()
        {
            return await GetTextFromFile(EventsDatafile);
        }

        public async Task SaveCommands(string commands)
        {
            var file = await CreateFile(CommandsDatafile);
            await SaveTextToFile(commands, file);
        }

        public async Task SaveEvents(string events)
        {
            var file = await CreateFile(EventsDatafile);
            await SaveTextToFile(events, file);
        }

        public async Task RemoveAllCommandsFromPersistence()
        {
            var folder = await GetFolder();
            await FileSystemProvider.DeleteFileAsync(folder, CommandsDatafile);
        }

        public async Task RemoveAllEventsFromPersistence()
        {
            var folder = await GetFolder();
            await FileSystemProvider.DeleteFileAsync(folder, EventsDatafile);
        }

        public async Task<bool> CheckForCommands()
        {
            var folder = await GetFolder();
            return await FileSystemProvider.GetFileAsync(folder, CommandsDatafile) != null;
        }

        public async Task<bool> CheckForEvents()
        {
            var folder = await GetFolder();
            return await FileSystemProvider.GetFileAsync(folder, EventsDatafile) != null;
        }

        private async Task<string> GetTextFromFile(string filename)
        {
            var folder = await GetFolder();
            var file = await FileSystemProvider.GetFileAsync(folder, filename);
            return await FileSystemProvider.ReadAllTextAsync(file);
        }

        private async Task SaveTextToFile(string text, IFile file)
        {
            await FileSystemProvider.WriteAllTextAsync(file, text);
        }

        private async Task<IFile> CreateFile(string filename)
        {
            var folder = await GetFolder();
            return await FileSystemProvider.CreateFileAsync(folder, filename, CreationCollisionOption.ReplaceExisting);
        }

        private async Task<IFolder> GetFolder()
        {
            return await FileSystemProvider.CreateFolderAsync(FileSystem.Current.LocalStorage, MessagesFolder, CreationCollisionOption.OpenIfExists);
        }
    }
}