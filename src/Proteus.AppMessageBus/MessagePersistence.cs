#region License

/*
 * Copyright © 2013-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Threading.Tasks;
using Plugin.NetStandardStorage.Abstractions.Interfaces;
using Plugin.NetStandardStorage.Abstractions.Types;
using Plugin.NetStandardStorage.Implementations;
using Proteus.AppMessageBus.Abstractions;

namespace Proteus.AppMessageBus
{
    public class MessagePersistence : IMessagePersistenceAsync
    {
        private const string MessagesFolder = "Proteus.Messaging.Messages";
        private const string CommandsDatafile = "Commands.data";
        private const string EventsDatafile = "Events.data";

        public IFileSystemProviderAsync FileSystemProvider { private get; set; }

        public MessagePersistence()
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
            //TODO: check for FileSystem.LocalStorage returns an exception and fallback to attempt some local filesystem access as needed
            return await FileSystemProvider.CreateFolderAsync(FileSystemProvider.LocalStorage, MessagesFolder, CreationCollisionOption.OpenIfExists);
        }
    }
}